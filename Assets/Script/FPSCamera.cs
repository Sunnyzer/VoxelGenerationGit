using UnityEngine;

public class FPSCamera : MonoBehaviour
{
    [SerializeField] Transform target = null;
    [SerializeField] float angleX = 0;
    [SerializeField] float angleY = 0;
    [SerializeField] float rotateSpeed = 10;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        angleX += Input.GetAxisRaw("Mouse X") * rotateSpeed;
        angleY -= Input.GetAxisRaw("Mouse Y") * rotateSpeed;
        angleX = angleX > 360 ? 0 : angleX < 0 ? 360 : angleX;
        angleY = Mathf.Clamp(angleY, -90, 90);
        Quaternion nextRotation = Quaternion.Euler(new Vector3(angleY, angleX));
        transform.rotation = nextRotation;
        target.rotation = transform.rotation;
    }
}
