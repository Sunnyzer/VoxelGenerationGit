using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MeshData
{
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<int> triangles = new List<int>();
    [SerializeField] List<Vector2> uvs = new List<Vector2>();
    [SerializeField] List<Vector3> normals = new List<Vector3>();
    Mesh mesh = new Mesh();
    public Mesh Mesh => mesh;

    public void UpdateMesh(MeshCollider _meshCollider, MeshFilter _meshFilter)
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.SetUVs(0, uvs);
        _meshCollider.sharedMesh = mesh;
        _meshFilter.mesh = mesh;
    }
}
