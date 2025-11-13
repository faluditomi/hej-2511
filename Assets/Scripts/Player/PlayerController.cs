using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _characterController;

    private Vector2 moveVector = Vector2.zero;

    [SerializeField] private float movementSpeed = 10f;

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

    public void SetMoveVector(Vector2 moveVector)
    {
        this.moveVector = moveVector; 
    }

    public void Move()
    {
        Vector3 move = transform.forward * moveVector.y + transform.right * moveVector.x;
        move = move * movementSpeed * Time.deltaTime;
        _characterController.Move(move);
    }
}
