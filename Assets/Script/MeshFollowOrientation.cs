using UnityEngine;

public class MeshFollowOrientation : MonoBehaviour
{
    [SerializeField] Transform orientation = null;
    void Update()
    {
        transform.forward = new Vector3(orientation.forward.x, 0, orientation.forward.z);
    }
}
