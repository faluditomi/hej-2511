using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _characterController;

    /// <summary>
    /// The input values from the player
    /// </summary>
    private Vector3 moveVector = Vector3.zero;

    /// <summary>
    /// Used for applying velocity on the y axis (e.g.: jump, gravity)
    /// </summary>
    private Vector3 playerVelocity = Vector3.zero;

    [SerializeField] private float
        moveSpeed = 8f,
        sprintSpeed = 15f,
        maxJumpHeight = 6.5f,
        gravityValue = 9.81f;

    [SerializeField]
    private bool
        isSprintActive = true,
        isJumpActive = true;

    /// <summary>
    /// Simple jump ->      Press once, jump of a maxJumpHeight.
    /// Non-simple jump ->  Pressing activates jump, letting go stops the player from elevating.
    ///                     If the player doesn't let go, the jump goes to maxJumpHeight.
    /// </summary>
    [SerializeField] private bool isJumpSimple = true;

    private bool isSprinting;

    #region MonoBehaviour Methods

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    #endregion

    #region Movement Logic

    public void Jump(bool isStarted)
    {
        if(!isJumpActive) return;

        if(isJumpSimple)
        {
            if(_characterController.isGrounded && isStarted)
            {
                playerVelocity.y = Mathf.Sqrt(maxJumpHeight * gravityValue);
            }
        }
        else
        {
            if(_characterController.isGrounded && isStarted)
            {
                playerVelocity.y = Mathf.Sqrt(maxJumpHeight * gravityValue);
            }
            else if(!_characterController.isGrounded && !isStarted && _characterController.velocity.y > 0f)
            {
                playerVelocity.y = 0f;
            }
        }
    }

    private void Move()
    {
        playerVelocity.y -= gravityValue * Time.deltaTime;
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        Vector3 move = ((moveVector * currentSpeed) + playerVelocity) * Time.deltaTime;
        _characterController.Move(move);
        if(_characterController.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
    }

    #endregion

    #region Setters & Getters

    public void SetMoveVector(Vector2 moveVector)
    {
        this.moveVector = transform.forward * moveVector.y + transform.right * moveVector.x;
    }
    
    public void SetSprint(bool isSprinting)
    {
        if(!isSprintActive)
        {
            this.isSprinting = false;
            return;
        }
        
        this.isSprinting = isSprinting;
    }

    #endregion
}
