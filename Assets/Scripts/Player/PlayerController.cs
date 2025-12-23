using System;
using System.Collections;
using DG.Tweening;
using UnityEditor.Splines;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //TODO clean up the fields, make editor sections, tooltips, conditional rendering
    private enum DelayTillGroundedType
    {
        MOVE_VECTOR,
        SPRINT_CROUCH,
    }

    private Transform followTarget;

    private Coroutine
        delayMoveVectorTillGroundedCoroutine,
        delaySprintOrCrouchTillGroundedCoroutine,
        crouchCoroutine,
        dashCoroutine,
        dashCooldownCoroutine;

    private CharacterController _characterController;

    private InputHandler inputHandler;
    
    [Tooltip("Layers that block the player from standing up")]
    [SerializeField] private LayerMask standBlockLayers;

    private Tweener
        crouchDownScaleTweener,
        crouchDownHeightTweener,
        crouchUpScaleTweener,
        crouchUpHeightTweener;

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

    private string _crouchTweenIdLiteral = "crouch";

    /// <summary>
    /// Controls whether the player has access to this mechanic.
    /// </summary>
    [SerializeField]
    private bool
        isSprintActive = true,
        isJumpActive = true,
        isDashActive = true,
        isCrouchActive = true;

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
        inputHandler = GetComponent<InputHandler>();
    }

    private void Start()
    {
        followTarget = transform.Find("Follow Target");

        originalLocalScale = transform.localScale;
        originalHeight = _characterController.height;
        currentGravity = baseGravity;

        InitTweeners();
    }

    private void FixedUpdate()
    {
        Move();
    }
        
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(isDashing) StopDash(Vector3.ProjectOnPlane(_characterController.velocity, Vector3.up));
    }

    #endregion

    #region Movement Logic

    private void InitTweeners()
    {
        DOTween.Init(true, true, LogBehaviour.Default);

        crouchDownScaleTweener = transform.DOScaleY(originalLocalScale.y * crouchHeightMultiplier, crouchTransitionDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => isCrouching = true)
            .SetId(_crouchTweenIdLiteral)
            .SetAutoKill(false)
            .Pause();

        crouchDownHeightTweener = DOTween.To(() => _characterController.height, x => _characterController.height = x, originalHeight * crouchHeightMultiplier, crouchTransitionDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => isCrouching = true)
            .SetId(_crouchTweenIdLiteral)
            .SetAutoKill(false)
            .Pause();

        crouchUpScaleTweener = transform.DOScaleY(originalLocalScale.y, crouchTransitionDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => isCrouching = false)
            .SetId(_crouchTweenIdLiteral)
            .SetAutoKill(false)
            .Pause();

        crouchUpHeightTweener = DOTween.To(() => _characterController.height, x => _characterController.height = x, originalHeight, crouchTransitionDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => isCrouching = false)
            .SetId(_crouchTweenIdLiteral)
            .SetAutoKill(false)
            .Pause();
    }

    public void StartDash(Vector3 dashDir)
    {
        if(!CanDash()) return;
        
        // If the player was crouching, stop it
        QuickResetCrouch();

        dashCoroutine = StartCoroutine(DashBehaviour(dashDir));
    }

    private void StopDash(Vector3 dashDir)
    {
        if(dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashCoroutine = null;
        }

        // Help make movement look continuous. After dashing, player starts falling in dash direction.
        dashDir.Normalize();
        moveVector = new Vector3(Vector3.Dot(dashDir, transform.right), Vector3.Dot(dashDir, transform.forward), 0f);

        // Once the player lands, they should immediately take control of the character
        DelayTillGrounded(() => SetMoveVector(inputHandler.GetCurrentMove()), DelayTillGroundedType.MOVE_VECTOR);

        isDashing = false;
        currentGravity = baseGravity;
        dashCooldownCoroutine = StartCoroutine(CooldownTimer(dashCooldownDuration, () => dashCooldownCoroutine = null));
    }

    public void Jump(bool isStarted)
    {
        if(!CanJump()) return;

        // If the player was crouching, stop it
        QuickResetCrouch();

        if(isJumpSimple)
        {
            if(_characterController.isGrounded && isStarted)
            {   
                playerVelocity.y = Mathf.Sqrt(maxJumpHeight * currentGravity);
            }
        }
        else
        {
            if(_characterController.isGrounded && isStarted)
            {
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
        if(!CanCrouch()) return;

        // Don't let the player crouch while in-air
        if(isStarted && !_characterController.isGrounded)
        {
            return;
        }

        if(crouchCoroutine != null)
        {
            StopCoroutine(crouchCoroutine);
            crouchCoroutine = null;
        }

        crouchCoroutine = StartCoroutine(CrouchBehaviour(isStarted));
    }

    private void QuickResetCrouch()
    {
        DOTween.Pause(_crouchTweenIdLiteral);
        _characterController.height = originalHeight;
        transform.localScale = originalLocalScale;
        isCrouching = false;
    }

    private void Move()
    {
        playerVelocity.y -= currentGravity * Time.deltaTime;
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        Vector3 localMoveVector = transform.forward * moveVector.y + transform.right * moveVector.x;
        Vector3 move = ((localMoveVector * currentSpeed) + playerVelocity) * Time.deltaTime;
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

    private IEnumerator CrouchBehaviour(bool isStarted)
    {
        // Checking for when the player is standing up
        if(!isStarted)
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

        // If a crouch transition is running, cancel it and start a new one.
        DOTween.Pause(_crouchTweenIdLiteral);

        if(isStarted && !isCrouching)
        {
            crouchDownScaleTweener.Restart();
            crouchDownHeightTweener.Restart();
        }
        else if(!isStarted && isCrouching)
        {
            crouchUpScaleTweener.Restart();
            crouchUpHeightTweener.Restart();
        }

        yield return new WaitForSeconds(crouchTransitionDuration);

        crouchCoroutine = null;
    }

    private IEnumerator DashBehaviour(Vector3 dashDir)
    {
        isDashing = true;
        currentGravity = 0f;

        if(dashDir.Equals(Vector3.zero))
        {
            dashDir = -followTarget.forward;
        }
        else
        {
            dashDir = followTarget.TransformDirection(dashDir);
        }

        float distanceTraveled = 0f;
        moveVector = Vector3.zero;

        while(distanceTraveled < dashDistance)
        {
            float step = dashSpeed * Time.deltaTime;
            _characterController.Move(dashDir * step);
            distanceTraveled += step;

            yield return null;
        }

        // If in the future we want the player to start losing altitude right after dashing, just project dashDir onto the Vector3.up plane
        StopDash(dashDir);
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

    #endregion

    #region Setters & Getters

    private bool CanSprint()
    {
        return isSprintActive;
    }

    private bool CanJump()
    {
        return isJumpActive;
    }

    private bool CanDash()
    {
        return isDashActive && !isDashing && dashCooldownCoroutine == null;
    }

    private bool CanCrouch()
    {
        return isCrouchActive;
    }

    public void SetMoveVector(Vector2 moveVector)
    {
        if(_characterController.isGrounded)
        {
            this.moveVector = moveVector;
        }
        else
        {
            DelayTillGrounded(() => this.moveVector = moveVector, DelayTillGroundedType.MOVE_VECTOR);
        }
    }

    public void SetSprint(bool isSprinting)
    {
        if(!CanSprint())
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
