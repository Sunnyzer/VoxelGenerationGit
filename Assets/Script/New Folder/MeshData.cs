using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MeshData
{
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<int> triangles = new List<int>();
    [SerializeField] List<Vector2> uvs = new List<Vector2>();

    Mesh mesh;
    public Mesh Mesh => mesh;
    
    public void UpdateMesh(MeshCollider _meshCollider, MeshFilter _meshFilter)
    {
        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;
        _meshCollider.sharedMesh = mesh;
    }
    public Face AddFace(Vector3 _faceCenter,Vector3Int _direction, BlockType _blockType)
    {
        Vector3 _directionFloat = _direction;
        Vector3 _directionFace = _directionFloat * 0.5f;
        bool _upVector = Vector3Int.up == _direction || Vector3Int.down == _direction;
        Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3Int.right : Vector3Int.up) * _directionFace;
        Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * 0.5f;

        Vector3 _faceRightUp = _faceCenter + _directionRight + _directionUp;
        vertices.Add(_faceRightUp);
        Vector3 _faceLeftUp = _faceCenter - _directionRight + _directionUp;
        vertices.Add(_faceLeftUp);
        Vector3 _faceRightDown = _faceCenter + _directionRight - _directionUp;
        vertices.Add(_faceRightDown);
        Vector3 _faceLeftDown = _faceCenter - _directionRight - _directionUp;
        vertices.Add(_faceLeftDown);

        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);

        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 2);

        uvs.AddRange(BlockManager.Instance.FaceUVs(_direction, _blockType));
        Face _face = new Face(new Vector3[4] { _faceRightUp, _faceLeftUp, _faceRightDown, _faceLeftDown }, triangles.GetRange(triangles.Count - 6, 6).ToArray());
        return _face;
    }
    public void ResetVerticesAndTriangles()
    {
        triangles.Clear();
        vertices.Clear();
        uvs.Clear();
    }

    public void SetVerticesAndTriangles(List<Vector3> _vertices, List<int> _triangles)
    {
        triangles = _triangles;
        vertices = _vertices;
    }
}
