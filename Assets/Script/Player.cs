using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public class Player : MonoBehaviour
{
    public event Action OnChunkGroundChange = null; 
    [SerializeField] BoxCollider boxCollider;
    [SerializeField] Rigidbody rb;
    [SerializeField] Vector3 moveDirection;
    [SerializeField] Vector3 velocity;
    [SerializeField] LayerMask groundLayer = 0;
    [SerializeField] float height = 2;
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float jumpHigh = 400;
    [SerializeField] float sizeFeetDetection = 0.75f;
    [SerializeField] float gravityScale = 1f;
    [SerializeField] Transform orientation = null;
    [SerializeField] int radius = 3;
    [SerializeField] ChunkUpgrade chunkGround = null;
    Vector3 forwardOrientation;
    Vector3 rightOrientation;
    Vector3Int pointCube;
    bool isGrounded = false;
    private void Start()
    {
        float tempGravityScale = gravityScale;
        gravityScale = 0;
        ChunkManagerUpgrade.Instance.OnFinishLoad += () => { gravityScale = tempGravityScale; };
        //OnChunkGroundChange += () => { ChunkManagerUpgrade.Instance.UpdateChunkFromChunk(chunkGround); };
    }
    private void Update()
    {
        forwardOrientation = new Vector3(orientation.forward.x, 0, orientation.forward.z);
        rightOrientation = new Vector3(orientation.right.x, 0, orientation.right.z);
        velocity = Input.GetAxisRaw("Horizontal") * rightOrientation + Input.GetAxisRaw("Vertical") * forwardOrientation;
        rb.velocity = velocity.normalized * moveSpeed + new Vector3(0,rb.velocity.y,0);
        Collider[] _colliders = Physics.OverlapBox(transform.position - transform.up * height,Vector3.one * sizeFeetDetection, Quaternion.identity, groundLayer);
        isGrounded = _colliders.Length != 0;
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
            rb.AddForce(Vector3.up * jumpHigh,ForceMode.Impulse);
        if (!isGrounded)
            rb.AddForce(Vector3.down * gravityScale);
        else if(velocity.y < 0)
            rb.velocity = new Vector3(velocity.x,0,velocity.z);
        bool _hit = Physics.Raycast(Camera.main.transform.position, orientation.forward,out RaycastHit _raycastHit, 100,groundLayer);
        Debug.DrawRay(Camera.main.transform.position, orientation.forward * 100, Color.red);
        if (!_hit) return;
        ChunkUpgrade _chunk = _raycastHit.collider.GetComponent<ChunkUpgrade>();
        if (!_chunk) return;
        pointCube = _chunk.GetPositionBlockFromWorldPosition(_raycastHit.point, _raycastHit.normal);
        if(Input.GetMouseButtonDown(0))
        {
            _chunk.DestroyBlockProfondeur(pointCube, radius);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pointCube, Vector3.one);
        if(isGrounded)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position - transform.up * height, Vector3.one * sizeFeetDetection);
    }
}
