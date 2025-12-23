using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

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

        _inputActions.Player.Crouch.performed += Crouch;
        _inputActions.Player.Crouch.canceled += Crouch;

        _inputActions.Player.DashUniversal.performed += Dash;
        _inputActions.Player.DashForward.performed += Dash;
        _inputActions.Player.DashBackwards.performed += Dash;
        _inputActions.Player.DashLeft.performed += Dash;
        _inputActions.Player.DashRight.performed += Dash;
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

        _inputActions.Player.Crouch.performed -= Crouch;
        _inputActions.Player.Crouch.canceled -= Crouch;

        _inputActions.Player.DashUniversal.performed -= Dash;
        _inputActions.Player.DashForward.performed -= Dash;
        _inputActions.Player.DashBackwards.performed -= Dash;
        _inputActions.Player.DashLeft.performed -= Dash;
        _inputActions.Player.DashRight.performed -= Dash;
    }

    private void Move(InputAction.CallbackContext input)
    {
        Vector2 moveVector = input.ReadValue<Vector2>();
        playerController.SetMoveVector(moveVector);
    }

    public Vector2 GetCurrentMove()
    {
        return _inputActions.Player.Move.ReadValue<Vector2>();
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

    private void Crouch(InputAction.CallbackContext input)
    {
        if(input.canceled && input.interaction is PressInteraction)
        {
            return;
        }

        if(input.interaction is PressInteraction)
        {
            playerController.Crouch(!playerController.GetCrouch());
        }
        else if(input.interaction is HoldInteraction)
        {
            playerController.Crouch(input.performed);
        }
    }

    private void Dash(InputAction.CallbackContext input)
    {
        if(input.action.name.Equals(_inputActions.Player.DashUniversal.name)) playerController.StartDash(Vector3.zero);
        if(input.action.name.Equals(_inputActions.Player.DashForward.name)) playerController.StartDash(Vector3.forward);
        if(input.action.name.Equals(_inputActions.Player.DashBackwards.name)) playerController.StartDash(Vector3.back);
        if(input.action.name.Equals(_inputActions.Player.DashLeft.name)) playerController.StartDash(Vector3.left);
        if(input.action.name.Equals(_inputActions.Player.DashRight.name)) playerController.StartDash(Vector3.right);
    }
}
