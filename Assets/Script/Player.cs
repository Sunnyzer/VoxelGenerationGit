using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

public class Player : MonoBehaviour
{
    [SerializeField] BoxCollider boxCollider;
    [SerializeField] Rigidbody rb;
    [SerializeField] Vector3 moveDirection;
    [SerializeField] Vector3 velocity;
    [SerializeField] LayerMask groundLayer = 0;
    [SerializeField] LayerMask playerLayer = 0;
    [SerializeField] float height = 2;
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float jumpHigh = 400;
    [SerializeField] float sizeFeetDetection = 0.75f;
    [SerializeField] public float gravityScale = 1f;
    [SerializeField] Transform orientation = null;
    [SerializeField] int radius = 3;
    [SerializeField] float cooldownBreakMax;
    [SerializeField] Slider radiusSlider;
    [SerializeField] Slider cooldownSlider;
    [SerializeField] TextMeshProUGUI radiusText;
    [SerializeField] TextMeshProUGUI cooldownText;
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
        gravityScale = 0;
        ChunkManagerFinal.Instance.OnFinishLoad += () => { gravityScale = tempGravityScale; };
    }
    private void Update()
    {
        radius = (int)radiusSlider.value;
        cooldownBreakMax = cooldownSlider.value;
        radiusText.text = radius.ToString();
        cooldownText.text = cooldownBreakMax.ToString();
        forwardOrientation = new Vector3(orientation.forward.x, 0, orientation.forward.z);
        rightOrientation = new Vector3(orientation.right.x, 0, orientation.right.z);
        velocity = Input.GetAxisRaw("Horizontal") * rightOrientation + Input.GetAxisRaw("Vertical") * forwardOrientation;
        rb.velocity = velocity.normalized * moveSpeed + new Vector3(0, rb.velocity.y, 0);
        Collider[] _colliders = Physics.OverlapBox(transform.position - transform.up * height, Vector3.one * sizeFeetDetection, Quaternion.identity, groundLayer);
        isGrounded = _colliders.Length != 0;
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            rb.AddForce(Vector3.up * jumpHigh * Time.deltaTime, ForceMode.Impulse);
        if (!isGrounded)
            rb.AddForce(Vector3.down * gravityScale);
        else if (velocity.y < 0)
            rb.velocity = new Vector3(velocity.x, 0, velocity.z);

        bool _hit = Physics.Raycast(Camera.main.transform.position, orientation.forward, out RaycastHit _raycastHit, 1000, groundLayer);
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
                {
                    Collider[] _overlapGameObject = Physics.OverlapSphere(pointPoint, 1, playerLayer);
                    if(_overlapGameObject.Length == 0)
                        _chunk.CreateWorldPositionBlock(pointPoint, _raycastHit.normal);
                }
                if (Input.GetMouseButton(2))
                    _chunk.DestroyWorldPositionRadius(pointPoint, _raycastHit.normal, radius);
                cooldownBreak = 0;
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position - transform.up * height, Vector3.one * sizeFeetDetection);
        if (!Application.isPlaying) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pointPoint, Vector3.one * 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pointCube, Vector3.one * 1);
    }
}
