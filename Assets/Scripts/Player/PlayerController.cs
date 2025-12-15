using System;
using System.Collections;
using UnityEditor.Splines;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //TODO clean up the fields, make editor sections, tooltips, conditional rendering
    private enum DelayTillGroundedType
    {
        MOVE_VECTOR,
        SPRINT_CROUCH
    }

    private Coroutine
        delayMoveVectorTillGroundedCoroutine,
        delaySprintOrCrouchTillGroundedCoroutine,
        crouchCoroutine,
        dashCoroutine,
        dashCooldownCoroutine;

    private CharacterController _characterController;
    
    [Tooltip("Layers that block the player from standing up")]
    [SerializeField] private LayerMask standBlockLayers;

    /// <summary>
    /// The inputs from the player
    /// </summary>
    private Vector3 moveVector = Vector3.zero;

    /// <summary>
    /// Used for applying velocity on the y axis (e.g.: jump, gravity)
    /// </summary>
    private Vector3 playerVelocity = Vector3.zero;

    private Vector3 originalLocalScale;

    private float 
        originalHeight,
        currentGravity;

    [SerializeField]
    private float
        moveSpeed = 8f,
        sprintSpeed = 15f,
        maxJumpHeight = 6.5f,
        baseGravity = 9.81f,
        crouchHeightMultiplier = 0.4f,
        crouchTransitionDuration = 0.2f,
        dashDistance = 5f,
        dashSpeed = 40f,
        dashCooldownDuration = 1f;

    /// <summary>
    /// Controls whether the player has access to this mechanic.
    /// </summary>
    [SerializeField]
    private bool
        isSprintActive = true,
        isJumpActive = true;

    /// <summary>
    /// Simple jump = [Press once, jump of a maxJumpHeight.] Non-simple jump = [Pressing activates jump, 
    /// letting go stops the player from elevating. If the player doesn't let go, the jump goes to maxJumpHeight.]
    /// </summary>
    [SerializeField] private bool isJumpSimple = true;

    private bool
        isSprinting,
        isCrouching,
        isDashing;

    #region MonoBehaviour Methods

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        originalLocalScale = transform.localScale;
        originalHeight = _characterController.height;
        currentGravity = baseGravity;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void OnCollisionEnter(Collision collision)
    {
            Debug.Log("stopped dashing");
        if(isDashing)
        {
            StopDash();
        }
    }

    #endregion

    #region Movement Logic

    public void StartDash(Vector3 dashDir)
    {
        //TODO this should cancel crouching

        if(!isDashing && dashCooldownCoroutine == null)
        {
            dashCoroutine = StartCoroutine(DashBehaviour(dashDir));
        }
    }

    private void StopDash()
    {
        isDashing = false;
        currentGravity = baseGravity;
        dashCooldownCoroutine = StartCoroutine(CooldownTimer(dashCooldownDuration, () => dashCooldownCoroutine = null));
        dashCoroutine = null;
    }

    public void Jump(bool isStarted)
    {
        if(!isJumpActive) return;

        if(isJumpSimple)
        {
            if(_characterController.isGrounded && isStarted)
            {
                // If the player was crouching, stop it
                if(isCrouching)
                {
                    Crouch(false);
                }
                
                playerVelocity.y = Mathf.Sqrt(maxJumpHeight * currentGravity);
            }
        }
        else
        {
            if(_characterController.isGrounded && isStarted)
            {
                // If the player was crouching, stop it
                if(isCrouching)
                {
                    Crouch(false);
                }

                playerVelocity.y = Mathf.Sqrt(maxJumpHeight * currentGravity);
            }
            else if(!_characterController.isGrounded && !isStarted && _characterController.velocity.y > 0f)
            {
                playerVelocity.y = 0f;
            }
        }
    }

    public void Crouch(bool isStarted)
    {
        // Don't let the player crouch while in-air
        if(isStarted && !_characterController.isGrounded)
        {
            return;
        }

        // If already crouched or a crouch transition is running, cancel it and start a new one.
        if(crouchCoroutine != null)
        {
            StopCoroutine(crouchCoroutine);
            crouchCoroutine = null;
        }

        Vector3 targetScale = isStarted ? originalLocalScale - Vector3.up * crouchHeightMultiplier : originalLocalScale;
        float targetHeight = isStarted ? originalHeight * crouchHeightMultiplier : originalHeight;
        isCrouching = isStarted;
        crouchCoroutine = StartCoroutine(ScaleTo(targetScale, targetHeight, crouchTransitionDuration));
    }

    //TODO instead of calling Crouch(false) when jump/dash/etc. happens, call this reset
    private void QuickResetCrouch()
    {
        // if()
    }

    private void Move()
    {
        playerVelocity.y -= currentGravity * Time.deltaTime;
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        Vector3 move = ((moveVector * currentSpeed) + playerVelocity) * Time.deltaTime;
        _characterController.Move(move);

        if(_characterController.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
    }

    /// <summary>
    /// Used to delay behaviours like sprint switching, crouching, etc. until the player reaches the ground.
    /// </summary>
    /// <param name="callback">The code that will be run once the player reaches the ground.</param>
    private void DelayTillGrounded(Action callback, DelayTillGroundedType type)
    {
        switch(type)
        {
            case DelayTillGroundedType.MOVE_VECTOR:
                if(delayMoveVectorTillGroundedCoroutine != null)
                {
                    StopCoroutine(delayMoveVectorTillGroundedCoroutine);
                }

                delayMoveVectorTillGroundedCoroutine = StartCoroutine(DelayTillGroundedBehaviour(callback, type));
                break;

            case DelayTillGroundedType.SPRINT_CROUCH:
                if(delaySprintOrCrouchTillGroundedCoroutine != null)
                {
                    StopCoroutine(delaySprintOrCrouchTillGroundedCoroutine);
                }

                delaySprintOrCrouchTillGroundedCoroutine = StartCoroutine(DelayTillGroundedBehaviour(callback, type));
                break;

            default:
                return;
        }
    }

    #endregion

    #region Coroutines

    private IEnumerator DashBehaviour(Vector3 dashDir)
    {
        isDashing = true;
        currentGravity = 0f;
        
        //TODO if the player hits something while dashing, stop dashing

        if(dashDir.Equals(Vector3.zero))
        {
            dashDir = -transform.forward;
        }
        else
        {
            dashDir = transform.TransformDirection(dashDir);
        }

        float distanceTraveled = 0f;

        while(distanceTraveled < dashDistance)
        {
            float step = dashSpeed * Time.deltaTime;
            _characterController.Move(dashDir * step);
            distanceTraveled += step;

            yield return null;
        }

        StopDash();
    }

    /// <summary>
    /// Used to keep track of user mechanic cooldowns. After 'duration', the 'onComplete' action will run.
    /// The action should probably be something like () => dashCooldownCoroutine = null 
    /// </summary>
    private IEnumerator CooldownTimer(float duration, Action onComplete)
    {
        yield return new WaitForSeconds(duration);

        onComplete?.Invoke();
    }

    private IEnumerator DelayTillGroundedBehaviour(Action callback, DelayTillGroundedType type)
    {
        yield return new WaitUntil(() => _characterController.isGrounded);
        callback?.Invoke();

        switch(type)
        {
            case DelayTillGroundedType.MOVE_VECTOR:
                delayMoveVectorTillGroundedCoroutine = null;
                break;

            case DelayTillGroundedType.SPRINT_CROUCH:
                delaySprintOrCrouchTillGroundedCoroutine = null;
                break;
        }
    }
    
    private IEnumerator ScaleTo(Vector3 targetScale, float targetHeight, float duration)
    {
        // Checking for !isCrouching since that is the target state of this transition
        if(!isCrouching)
        {
            // Cast a ray from the player's head upward to check for obstacles
            RaycastHit hit;
            yield return new WaitUntil(() =>
            {
                Vector3 tipOfTheHead = new Vector3(transform.position.x, _characterController.bounds.max.y, transform.position.z);
                float rayLength = originalLocalScale.y - (originalLocalScale.y * crouchHeightMultiplier);
                return !Physics.Raycast(tipOfTheHead, Vector3.up, out hit, rayLength, standBlockLayers);
            });
        }

        Vector3 startScale = transform.localScale;
        float startHeight = _characterController.height;

        if(Mathf.Approximately(duration, 0f))
        {
            transform.localScale = targetScale;
            _characterController.height = targetHeight;
            crouchCoroutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Smoothstep easing
            float eased = t * t * (3f - 2f * t);

            // Interpolate scale
            Vector3 newScale = Vector3.Lerp(startScale, targetScale, eased);
            transform.localScale = newScale;

            yield return null;
        }

        transform.localScale = targetScale;

        crouchCoroutine = null;
    }

    #endregion

    #region Setters & Getters

    public void SetMoveVector(Vector2 moveVector)
    {
        if(_characterController.isGrounded)
        {
            this.moveVector = transform.forward * moveVector.y + transform.right * moveVector.x;
        }
        else
        {
            DelayTillGrounded(() => this.moveVector = transform.forward * moveVector.y + transform.right * moveVector.x, DelayTillGroundedType.MOVE_VECTOR);
        }
    }

    public void SetSprint(bool isSprinting)
    {
        if(!isSprintActive)
        {
            this.isSprinting = false;
            return;
        }

        if(_characterController.isGrounded)
        {
            this.isSprinting = isSprinting;
        }
        else
        {
            DelayTillGrounded(() => this.isSprinting = isSprinting, DelayTillGroundedType.SPRINT_CROUCH);
        }
    }

    public bool GetCrouch()
    {
        return this.isCrouching;
    }

    #endregion
}
