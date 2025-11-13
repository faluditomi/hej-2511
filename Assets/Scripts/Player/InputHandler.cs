using UnityEngine;
using UnityEngine.InputSystem;

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
    }

    private void OnDisable()
    {
        _inputActions.Disable();

        _inputActions.Player.Move.performed -= Move;
        _inputActions.Player.Move.canceled -= Move;

        _inputActions.Player.Look.performed -= Look;
        _inputActions.Player.Look.canceled -= Look;
    }

    private void Move(InputAction.CallbackContext input)
    {
        if (playerController == null) return;

        Vector2 moveVector = input.ReadValue<Vector2>();
        playerController.SetMoveVector(moveVector);
    }
    
    private void Look(InputAction.CallbackContext input)
    {
        if (cameraController == null) return;

        Vector2 lookVector = input.ReadValue<Vector2>();
        cameraController.Look(lookVector);
    }
}
