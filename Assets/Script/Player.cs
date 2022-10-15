using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public class Player : MonoBehaviour
{
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
    [SerializeField] float cooldownBreakMax;
    Vector3 forwardOrientation;
    Vector3 rightOrientation;
    Vector3 pointPoint;
    Vector3Int pointCube;
    bool isGrounded = false;
    bool isPressedRight = false;
    bool isPressedLeft = false;
    float cooldownBreak;
    float radiusCube = 1;
    BlockData pointBlockData;

    private void Start()
    {
        float tempGravityScale = gravityScale;
        //gravityScale = 0;
        //ChunkManager.Instance.OnFinishLoad += () => { gravityScale = tempGravityScale; };
    }
    private void Update()
    {
        forwardOrientation = new Vector3(orientation.forward.x, 0, orientation.forward.z);
        rightOrientation = new Vector3(orientation.right.x, 0, orientation.right.z);
        velocity = Input.GetAxisRaw("Horizontal") * rightOrientation + Input.GetAxisRaw("Vertical") * forwardOrientation;
        rb.velocity = velocity.normalized * moveSpeed + new Vector3(0, rb.velocity.y, 0);
        Collider[] _colliders = Physics.OverlapBox(transform.position - transform.up * height, Vector3.one * sizeFeetDetection, Quaternion.identity, groundLayer);
        isGrounded = _colliders.Length != 0;
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            rb.AddForce(Vector3.up * jumpHigh, ForceMode.Impulse);
        if (!isGrounded)
            rb.AddForce(Vector3.down * gravityScale);
        else if (velocity.y < 0)
            rb.velocity = new Vector3(velocity.x, 0, velocity.z);
        bool _hit = Physics.Raycast(Camera.main.transform.position, orientation.forward, out RaycastHit _raycastHit, 100, groundLayer);
        Debug.DrawRay(Camera.main.transform.position, orientation.forward * 100, Color.red);
        cooldownBreak += Time.deltaTime;
        if (!_hit) return;
        ChunkFinal _chunk = _raycastHit.collider.GetComponent<ChunkFinal>();
        pointPoint = _raycastHit.point;
        if (_chunk)
        {
            if (cooldownBreak >= cooldownBreakMax)
            {
                if (Input.GetMouseButton(0))
                    _chunk.DestroyWorldPositionBlock(pointPoint, _raycastHit.normal);
                if (Input.GetMouseButton(1))
                    _chunk.CreateWorldPositionBlock(pointPoint, _raycastHit.normal);
                cooldownBreak = 0;
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pointPoint, Vector3.one * 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pointCube, Vector3.one * 1);
        Gizmos.color = Color.magenta;
        if(pointBlockData)
        {
            foreach (var item in pointBlockData.blocksNeighbor)
            {
                Vector3 _direction = item.Key;
                Vector3 _posCube = item.Value.owner.transform.position + pointCube + _direction;
                Gizmos.DrawWireCube( _posCube, Vector3.one * ChunkManager.sizeBlock);
            }
        }
    }
}
