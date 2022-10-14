using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;

[Serializable]
struct ColorVector
{
    public Color color;
    public Vector3 vector;
    public ColorVector(Color _color, Vector3 _vector)
    {
        color = _color;
        vector = _vector;
    }
}

[Serializable]
public class MeshData
{
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<int> triangles = new List<int>();
    [SerializeField] List<Vector2> uvs = new List<Vector2>();
    [SerializeField] Dictionary<Vector3, Vector3> verticesNormal = new Dictionary<Vector3, Vector3>();
    [SerializeField] List<UnityEngine.Vector3> normals = new List<Vector3>();
    [SerializeField] ChunkFinal owner;
    Dictionary<Vector3, int> verticesIndex = new Dictionary<Vector3, int>();
    Mesh mesh;
    bool debug = false;
    [SerializeField] List<ColorVector> debugList = new List<ColorVector>();
    public Mesh Mesh => mesh;
    
    public MeshData(ChunkFinal _chunk)
    {
        owner = _chunk;
        mesh = new Mesh();
        verticesIndex = new Dictionary<Vector3, int>();
    } 
    public void UpdateMesh(MeshCollider _meshCollider, MeshFilter _meshFilter)
    {
        mesh.vertices = verticesIndex.Keys.ToArray();
        mesh.triangles = triangles.ToArray();
        for (int i = 0; i < vertices.Count; i++)
        {
            //verticesNormal[vertices[i]] = verticesNormal[vertices[i]].normalized;// = verticesNormal[vertices[i]].Normalize();
        }
        mesh.normals = verticesNormal.Values.ToArray();
        normals = mesh.normals.ToList();
       // mesh.RecalculateNormals();
        //normals = mesh.normals.ToList();
        mesh.SetUVs(0, uvs);
        _meshCollider.sharedMesh = mesh;
        _meshFilter.mesh = mesh;
    }
    public Face AddFace(Vector3Int _faceCenter, Vector3Int _direction, float _sizeFace = 1)
    {
        Vector3 _directionFloat = _direction;
        Vector3 _directionFace = _directionFloat;
        bool _upVector = Vector3Int.up == _direction || Vector3Int.down == _direction;
        Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3Int.right : Vector3Int.up) * _directionFace;
        Vector3Int _directionUp = (_upVector ? Vector3Int.right : Vector3Int.up);

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
        //return SimpleVertices(_faceCenter, _directionRight, _directionUp, _direction);
        return new Face(new Vector3[4] { _faceRightUp, _faceRightDown, _faceLeftUp, _faceLeftDown }, triangles.GetRange(triangles.Count - 6, 6).ToArray());
    }
    Face SimpleVertices(Vector3Int _faceCenter, Vector3 _directionRight, Vector3 _directionUp,Vector3 _direction)
    {
        Vector3 _faceRightUp = _faceCenter + _directionRight + _directionUp;
        if (!verticesIndex.ContainsKey(_faceRightUp))
        {
            verticesIndex.Add(_faceRightUp, vertices.Count);
            vertices.Add(_faceRightUp);
            verticesNormal.Add(_faceRightUp, _direction);
        }
        else
        {
            verticesNormal[_faceRightUp] += _direction;
        }
        Vector3 _faceRightDown = _faceCenter + _directionRight - _directionUp;
        if (!verticesIndex.ContainsKey(_faceRightDown))
        {
            verticesIndex.Add(_faceRightDown, vertices.Count);
            vertices.Add(_faceRightDown);
            verticesNormal.Add(_faceRightDown, _direction);
        }
        else
        {
            verticesNormal[_faceRightDown] += _direction;
        }
        Vector3 _faceLeftUp = _faceCenter - _directionRight + _directionUp;
        if (!verticesIndex.ContainsKey(_faceLeftUp))
        {
            verticesIndex.Add(_faceLeftUp, vertices.Count);
            vertices.Add(_faceLeftUp);
            verticesNormal.Add(_faceLeftUp, _direction);
        }
        else
        {
            verticesNormal[_faceLeftUp] += _direction;
        }
        Vector3 _faceLeftDown = _faceCenter - _directionRight - _directionUp;
        if (!verticesIndex.ContainsKey(_faceLeftDown))
        {
            verticesIndex.Add(_faceLeftDown, vertices.Count);
            vertices.Add(_faceLeftDown);
            verticesNormal.Add(_faceLeftDown, _direction);
        }
        else
        {
            verticesNormal[_faceLeftDown] += _direction;
        }

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
        if(!Contains(_vertice))
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
    bool Contains(Vector3 _verticePos)
    {
        if (verticesIndex.ContainsKey(_verticePos))
            return true;
        foreach (var item in verticesIndex.Keys)
        {
            if((item - _verticePos).sqrMagnitude < 10f)
            {
                return true;
            }
        }
        return false;
    }
    public void DebugMesh()
    {
        if(!debug)
        {
            debug = true;
            foreach (var item in verticesNormal)
            {
                debugList.Add(new ColorVector(Random.ColorHSV(), Random.insideUnitSphere));
            }
        }
        int i = 0;
        foreach (var item in verticesNormal)
        {
            Gizmos.color = debugList[i].color;
            Vector3 _rand = debugList[i].vector;
            Gizmos.DrawRay(owner.transform.position + item.Key, item.Value + _rand);
            ++i;
        }
    }
}
