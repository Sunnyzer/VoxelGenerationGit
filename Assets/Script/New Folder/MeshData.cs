using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class MeshData
{
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<int> triangles = new List<int>();
    [SerializeField] List<Vector2> uvs = new List<Vector2>();
    [SerializeField] Dictionary<Vector3, Vector3> verticesNormal = new Dictionary<Vector3, Vector3>();
    [SerializeField] List<Vector3> normals = new List<Vector3>();
    [SerializeField] ChunkFinal owner;
    Dictionary<Vector3, int> verticesIndex = new Dictionary<Vector3, int>();
    Mesh mesh;
    public Mesh Mesh => mesh;
    
    public MeshData(ChunkFinal _chunk)
    {
        owner = _chunk;
        mesh = new Mesh();
        verticesIndex = new Dictionary<Vector3, int>();
    } 
    public void UpdateMesh(MeshCollider _meshCollider, MeshFilter _meshFilter)
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        for (int i = 0; i < vertices.Count; i++)
        {
            verticesNormal[vertices[i]] += Vector3.up * 100;
            //verticesNormal[vertices[i]] = verticesNormal[vertices[i]].normalized;
        }
        mesh.normals = verticesNormal.Values.ToArray();
       // mesh.RecalculateNormals();
        //normals = mesh.normals.ToList();
        mesh.SetUVs(0, uvs);
        _meshCollider.sharedMesh = mesh;
        _meshFilter.mesh = mesh;
    }
    public Face AddFace(Vector3 _faceCenter,Vector3Int _direction, float _sizeFace = 1)
    {
        Vector3 _directionFloat = _direction;
        Vector3 _directionFace = _directionFloat * (_sizeFace / 2);
        bool _upVector = Vector3Int.up == _direction || Vector3Int.down == _direction;
        Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3Int.right : Vector3Int.up) * _directionFace;
        Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * (_sizeFace / 2);

        Vector3 _faceRightUp = _faceCenter + _directionRight + _directionUp;
        AddVerticesAndNormalIfNotContains(_faceRightUp, _direction);
        Vector3 _faceRightDown = _faceCenter + _directionRight - _directionUp;
        AddVerticesAndNormalIfNotContains(_faceRightDown, _direction);
        Vector3 _faceLeftUp = _faceCenter - _directionRight + _directionUp;
        AddVerticesAndNormalIfNotContains(_faceLeftUp, _direction);
        Vector3 _faceLeftDown = _faceCenter - _directionRight - _directionUp;
        AddVerticesAndNormalIfNotContains(_faceLeftDown, _direction);

        triangles.Add(verticesIndex[_faceLeftUp]);
        triangles.Add(verticesIndex[_faceRightDown]);
        triangles.Add(verticesIndex[_faceRightUp]);

        triangles.Add(verticesIndex[_faceLeftUp]);
        triangles.Add(verticesIndex[_faceLeftDown]);
        triangles.Add(verticesIndex[_faceRightDown]);

        return new Face(new Vector3[4] { _faceRightUp, _faceRightDown, _faceLeftUp, _faceLeftDown }, triangles.GetRange(triangles.Count - 6, 6).ToArray());
    }
    void AddVerticesAndNormalIfNotContains(Vector3 _vertice, Vector3 _normal)
    {
        if(!verticesIndex.ContainsKey(_vertice))
        {
            verticesIndex.Add(_vertice, vertices.Count);
            vertices.Add(_vertice);
            verticesNormal.Add(_vertice, _normal);
        }
        else
        {
            verticesNormal[_vertice] += _normal;
        }
    }
    public void DebugMesh()
    {
        foreach (var item in verticesNormal)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(owner.transform.position + item.Key, item.Value);
        }
        //for (int i = 0; i < normals.Count; i++)
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawRay(owner.transform.position + vertices[i], normals[i]);
        //}
    }
}
