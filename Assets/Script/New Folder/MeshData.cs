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
    Dictionary<Vector3, int> verticesIndex = new Dictionary<Vector3, int>();
    Mesh mesh = new Mesh();
    [SerializeField] ChunkFinal owner;
    public Mesh Mesh => mesh;
    
    public MeshData(ChunkFinal _chunk)
    {
        owner = _chunk;
        mesh = new Mesh();
    } 
    public void UpdateMesh(MeshCollider _meshCollider, MeshFilter _meshFilter)
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.SetUVs(0, uvs);
        _meshCollider.sharedMesh = mesh;
        _meshFilter.mesh = mesh;
    }
    public void AddFace(Vector3 _faceCenter,Vector3Int _direction, float _sizeFace = 1)
    {
        Vector3 _directionFloat = _direction;
        Vector3 _directionFace = _directionFloat * (_sizeFace / 2);
        bool _upVector = Vector3Int.up == _direction || Vector3Int.down == _direction;
        Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3Int.right : Vector3Int.up) * _directionFace;
        Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * (_sizeFace / 2);

        Vector3 _faceRightUp = _faceCenter + _directionRight + _directionUp;
        AddVertices(_faceRightUp);
        Vector3 _faceRightDown = _faceCenter + _directionRight - _directionUp;
        AddVertices(_faceRightDown);
        Vector3 _faceLeftUp = _faceCenter - _directionRight + _directionUp;
        AddVertices(_faceLeftUp);
        Vector3 _faceLeftDown = _faceCenter - _directionRight - _directionUp;
        AddVertices(_faceLeftDown);

        triangles.Add(verticesIndex[_faceRightUp]);
        triangles.Add(verticesIndex[_faceRightDown]);
        triangles.Add(verticesIndex[_faceLeftUp]);

        triangles.Add(verticesIndex[_faceRightDown]);
        triangles.Add(verticesIndex[_faceLeftUp]);
        triangles.Add(verticesIndex[_faceLeftDown]);
    }
    void AddVertices(Vector3 _vertice)
    {
        if(!verticesIndex.ContainsKey(_vertice))
        {
            verticesIndex.Add(_vertice, vertices.Count);
            vertices.Add(_vertice);
        }
    }
}
