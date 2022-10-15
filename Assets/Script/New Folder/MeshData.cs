using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;
using UnityEngine.Assertions.Must;

[Serializable]
public class Test<T>
{
    public Vector3 key;
    public T value;
    public Test(Vector3 _key, T _value)
    {
        key = _key;
        value = _value;
    }
}
[Serializable]
public class Dic<T>
{
    public List<Test<T>> dic = new List<Test<T>>();
    public List<Vector3> vertices = new List<Vector3>();
    public List<T> normal = new List<T>();

    public bool Contains(Vector3 _vertex)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vector3.Distance(vertices[i], _vertex) < 0.1f)
                return true;
        }
        return false;
    }
    public int GetIndex(Vector3 _vertex)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vector3.Distance(vertices[i], _vertex) < 0.1f)
                return i;
        }
        return -1;
    }
    public T GetContains(Vector3 _vertex)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vector3.Distance(vertices[i], _vertex) < 0.1f)
                return normal[i];
        }
        return default(T);
    }
    public void Add(Vector3 _vertex, T _normal)
    {
        dic.Add(new Test<T>(_vertex, _normal));
        vertices.Add(_vertex);
        normal.Add(_normal);
    }
    public T this[Vector3 _index]
    {
        get => GetContains(_index);
        set => normal[GetIndex(_index)] = value;
    }
}

[Serializable]
public class MeshData
{
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<int> triangles = new List<int>();
    [SerializeField] List<Vector2> uvs = new List<Vector2>();
    //[SerializeField] Dictionary<Vector3, Vector3> verticesNormal = new Dictionary<Vector3, Vector3>();
    [SerializeField] Dic<Vector3> verticesNormal = new Dic<Vector3>();
    //[SerializeField] ChunkFinal owner;
    //Dictionary<Vector3, int> verticesIndex = new Dictionary<Vector3, int>();
    Dic<int> verticesIndex = new Dic<int>();

    Mesh mesh;
    public Mesh Mesh => mesh;
    
    public MeshData(ChunkFinal _chunk)
    {
        mesh = new Mesh();
    } 
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
    public Face AddFace(Vector3 _faceCenter,Vector3Int _direction, float _sizeFace = 1)
    {
        Vector3 _directionFloat = _direction;
        Vector3 _directionFace = _directionFloat * (_sizeFace / 2);
        bool _upVector = Vector3Int.up == _direction || Vector3Int.down == _direction;
        Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3Int.right : Vector3Int.up) * _directionFace;
        Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * (_sizeFace / 2);

        Vector3 _faceRightUp = _faceCenter + _directionRight + _directionUp;
        vertices.Add(_faceRightUp);
        Vector3 _faceRightDown = _faceCenter + _directionRight - _directionUp;
        vertices.Add(_faceRightDown);
        Vector3 _faceLeftUp = _faceCenter - _directionRight + _directionUp;
        vertices.Add(_faceLeftUp);
        Vector3 _faceLeftDown = _faceCenter - _directionRight - _directionUp;
        vertices.Add(_faceLeftDown);

        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 4);

        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 3);

        Face _face = new Face(new Vector3[4] { _faceRightUp, _faceRightDown, _faceLeftUp, _faceLeftDown }, triangles.GetRange(triangles.Count - 6, 6).ToArray());
        return _face;
    }
    public void ResetVerticesAndTriangles()
    {
        triangles.Clear();
        vertices.Clear();
    }
    void AddVerticesAndNormalIfNotContains(Vector3 _vertice, Vector3Int _normal)
    {
        if(!verticesIndex.Contains(_vertice))
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
}
