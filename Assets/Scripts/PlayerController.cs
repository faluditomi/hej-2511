using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions input;

    private Rigidbody myRigidbody;

    private Transform followTarget;

    private Vector2 moveVector = Vector2.zero;

    private float yRotation = 0f;
    private float xRotation = 0f;
    [Tooltip("The speed at which the player can move quietly.")]
    [SerializeField] private float sneakSpeed = 1f;
    [Tooltip("The speed that the player moves at when they are sprinting and making noise.")]
    [SerializeField] private float sprintSpeed = 3f;

    private bool isSprinting;

    void Awake()
    {
        input = new PlayerInputActions();

        myRigidbody = GetComponent<Rigidbody>();

        followTarget = transform.Find("Follow Target");

        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;
    }

    void OnEnable()
    {
        input.Enable();

        input.Gameplay.Movement.performed += Move;
        input.Gameplay.Movement.canceled += Move;

        input.Gameplay.Sprint.performed += Sprint;
        input.Gameplay.Sprint.canceled += Sprint;

        input.Gameplay.Look.performed += Look;
        input.Gameplay.Look.canceled += Look;
    }

    void OnDisable()
    {
        input.Disable();

        input.Gameplay.Movement.performed -= Move;
        input.Gameplay.Movement.canceled -= Move;

        input.Gameplay.Sprint.performed -= Sprint;
        input.Gameplay.Sprint.canceled -= Sprint;

        input.Gameplay.Look.performed -= Look;
        input.Gameplay.Look.canceled -= Look;
    }

    private void FixedUpdate()
    {
        followTarget.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        
        if(moveVector != null)
        {
            if(isSprinting)
            {
                myRigidbody.AddRelativeForce(new Vector3(moveVector.x, 0f, moveVector.y) * sprintSpeed, ForceMode.VelocityChange);
            }
            else
            {
                myRigidbody.AddRelativeForce(new Vector3(moveVector.x, 0f, moveVector.y) * sneakSpeed, ForceMode.VelocityChange);
            }
        }
    }

    private void Move(InputAction.CallbackContext input)
    {
        moveVector = input.ReadValue<Vector2>();
    }

    private void Sprint(InputAction.CallbackContext input)
    {
        if(input.performed)
        {
            isSprinting = true;
        }
        else if(input.canceled)
        {
            isSprinting = false;
        }
    }

    private void Look(InputAction.CallbackContext input)
    {
        //The transform rotation can only be caught up to the follow transform's rotation
        //once the cinemachine scripts have had time to run.
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        //Once the player is rotated properly, we have to reset the follow target and thus
        //align the camera.
        followTarget.localEulerAngles = new Vector3(xRotation, 0f, 0f);

        xRotation += -input.ReadValue<Vector2>().y;

        xRotation = Mathf.Clamp(xRotation, -25f, 70f);

        yRotation += input.ReadValue<Vector2>().x;
    }

    public bool GetIsSprinting()
    {
        return isSprinting;
    }
}
