using UnityEngine;

public class PlayerCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target = null;
    [SerializeField] private float rotateSpeed = 1;
    [SerializeField] private float radius = 2;
    [SerializeField] private float angleX = 0;
    [SerializeField] private float angleY = 0;
    [SerializeField] private Vector3 currentPos;
    [SerializeField] private Quaternion currentRotation;
    [SerializeField] private Vector3 smoothVelocity;
    [SerializeField] private float smoothTime;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        angleX += Input.GetAxisRaw("Mouse X") * rotateSpeed;
        angleY -= Input.GetAxisRaw("Mouse Y") * rotateSpeed;
        angleX = angleX > 360 ? 0 : angleX < 0 ? 360 : angleX;
        //angleX = Mathf.Clamp(angleX, -360, 360);
        angleY = Mathf.Clamp(angleY, -90, 90);

        Quaternion nextRotation = Quaternion.Euler(new Vector3(angleY, angleX));

        currentRotation = Quaternion.Lerp(currentRotation, nextRotation, smoothTime);
        transform.rotation = currentRotation;

        Vector3 _forward = transform.forward;
        transform.position = target.transform.position - _forward * radius + target.transform.right * 1;
        target.transform.forward = _forward;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.position);
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}
