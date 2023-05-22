using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] float destroyRate = 0.1f;
    float time = 0;
    void Update()
    {
        time += Time.deltaTime;
        if (!Input.GetMouseButton(0)) return;
        if (time < destroyRate) return;
        time = 0;
        bool _hit = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, 10, obstacleLayer);
        Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.red, 2f);
        if(_hit)
        {
            Chunk _chunk = hitInfo.collider.GetComponent<Chunk>();
            if (!_chunk) return;
            _chunk.DestroyBlock(hitInfo.point - hitInfo.normal * 0.01f);
        }
    }
}
