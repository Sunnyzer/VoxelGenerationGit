using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float ratioSprint = 2.0f;
    bool isSprinting = false;
    float axisX = 0;
    float axisY = 0;
    float axisZ = 0;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        axisZ = Input.GetAxis("Vertical");
        axisX = Input.GetAxis("Horizontal");
        axisY = -Input.GetAxis("Up");
        float _mouseX = Input.GetAxis("Mouse X");
        float _mouseY = Input.GetAxis("Mouse Y");
        transform.eulerAngles += Vector3.up * _mouseX + Vector3.right * -_mouseY;
        if (Input.GetKeyDown(KeyCode.LeftShift))
            isSprinting = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            isSprinting = false;
    }
    private void FixedUpdate()
    {
        Move();
    }
    void Move()
    {       
        Vector3 _direction = transform.forward * axisZ + transform.right * axisX + transform.up * axisY;
        transform.position += _direction.normalized * (isSprinting ? moveSpeed * ratioSprint : moveSpeed) * Time.fixedDeltaTime;
    }
}
