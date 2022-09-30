using UnityEngine;

public class PlayerCameraFollow : MonoBehaviour
{
    [SerializeField] private Player target = null;
    [SerializeField] private float rotateSpeedX = 0;
    [SerializeField] private float rotateSpeedY = 0;
    private float axisX = 0;
    private float axisY = 0;
    private float yRotation;
    private float xRotation;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        axisX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * rotateSpeedX;
        axisY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * rotateSpeedY;

        yRotation += axisX;
        xRotation -= axisY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
        transform.position = target.transform.position + Vector3.up * target.Height/2;
        target.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0); 
    }
}
