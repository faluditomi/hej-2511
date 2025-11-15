using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController), typeof(CameraController))]
public class InputHandler : MonoBehaviour
{
    private PlayerController playerController;
    private CameraController cameraController;
    private InputSystem_Actions _inputActions;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        cameraController = GetComponent<CameraController>();
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();

        _inputActions.Player.Move.performed += Move;
        _inputActions.Player.Move.canceled += Move;

        _inputActions.Player.Look.performed += Look;
        _inputActions.Player.Look.canceled += Look;

        _inputActions.Player.Sprint.performed += Sprint;
        _inputActions.Player.Sprint.canceled += Sprint;

        _inputActions.Player.Jump.performed += Jump;
        _inputActions.Player.Jump.canceled += Jump;
    }

    private void OnDisable()
    {
        _inputActions.Disable();

        _inputActions.Player.Move.performed -= Move;
        _inputActions.Player.Move.canceled -= Move;

        _inputActions.Player.Look.performed -= Look;
        _inputActions.Player.Look.canceled -= Look;

        _inputActions.Player.Sprint.performed -= Sprint;
        _inputActions.Player.Sprint.canceled -= Sprint;

        _inputActions.Player.Jump.performed -= Jump;
        _inputActions.Player.Jump.canceled -= Jump;
    }

    private void Move(InputAction.CallbackContext input)
    {
        Vector2 moveVector = input.ReadValue<Vector2>();
        playerController.SetMoveVector(moveVector);
    }

    private void Look(InputAction.CallbackContext input)
    {
        Vector2 lookVector = input.ReadValue<Vector2>();
        cameraController.Look(lookVector);
    }

    private void Sprint(InputAction.CallbackContext input)
    {
        playerController.SetSprint(input.performed);
    }

    private void Jump(InputAction.CallbackContext input)
    {
        playerController.Jump(input.performed);
    }
}
