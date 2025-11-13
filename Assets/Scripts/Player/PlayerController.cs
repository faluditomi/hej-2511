using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _characterController;

    private Vector2 moveVector = Vector2.zero;

    [SerializeField] private float moveSpeed = 8f, sprintSpeed = 15f;

    private bool isSprinting;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        if(moveVector != Vector2.zero)
        {
            Move();
        }
    }

    private void Move()
    {
        Vector3 move = transform.forward * moveVector.y + transform.right * moveVector.x;
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        move = move * currentSpeed * Time.deltaTime;
        _characterController.Move(move);
    }

    #region Setters & Getters

    public void SetMoveVector(Vector2 moveVector)
    {
        this.moveVector = moveVector;
    }
    
    public void SetSprint(bool isSprinting)
    {
        this.isSprinting = isSprinting;
    }

    #endregion
}
