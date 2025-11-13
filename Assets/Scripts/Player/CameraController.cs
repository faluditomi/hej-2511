using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform followTarget;

    [SerializeField] private float lookSensitivity = 5f;
    private float xRotation = 0f, yRotation = 0f;

    private void Awake()
    {
        followTarget = transform.Find("Follow Target");
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Look(Vector2 lookVector)
    {
        //The transform rotation can only be caught up to the follow transform's rotation
        //once the cinemachine scripts have had time to run.
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        //Once the player is rotated properly, we have to reset the follow target and thus
        //align the camera.
        followTarget.localEulerAngles = new Vector3(xRotation, 0f, 0f);

        xRotation += -lookVector.y * lookSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += lookVector.x * lookSensitivity * Time.deltaTime;
    }
}
