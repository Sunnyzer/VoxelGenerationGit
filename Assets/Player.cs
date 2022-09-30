using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.XR;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10;
    [SerializeField] LayerMask groundLayer = 0;
    [SerializeField] bool isGrounded = false;
    [SerializeField] float height = 2;
    [SerializeField] Vector3 moveDirection;
    [SerializeField] CharacterController character;
    public float Height => height;
    private void Start()
    {
        
    }
    private void Update()
    {
        moveDirection = Input.GetAxisRaw("Horizontal") * transform.right + Input.GetAxisRaw("Vertical") * transform.forward;
        isGrounded = Physics.Raycast(transform.position, -transform.up, height,groundLayer);
    }
    private void FixedUpdate()
    {
        character.SimpleMove(moveDirection.normalized * moveSpeed * Time.deltaTime);
    }
}
