using System;
using System.Collections.Generic;
using UnityEngine;
struct NeighborVertices
{
    public int vertice;
    public List<int> neighbor;
    public List<int> neighborAngle;
    public NeighborVertices(int _vertice, List<int> _neighbor, List<int> _neighborAngle)
    {
        vertice = _vertice;
        neighbor = _neighbor;
        neighborAngle = _neighborAngle;
    }
}

public class Cube : MonoBehaviour
{
    #region Field/Properties
    #region NonSerialize
    Mesh mesh;
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    public static Dictionary<Vector3, int> dirValue = new Dictionary<Vector3, int>()
    {
        { Vector3.forward, 1 },
        { Vector3.right, 2 },
        { Vector3.up, 4 },
    };
    public static Dictionary<int, Vector3> valueCoef = new Dictionary<int, Vector3>()
    {
        { 0, new Vector3Int(1, 1, 1) },
        { 1, new Vector3(-1, 1, 1) },
        { 2, new Vector3(1, 1,-1) },
        { 3, new Vector3(-1, 1, -1) },
        { 4, new Vector3(1, -1, 1) },
        { 5, new Vector3(-1, -1, 1) },
        { 6, new Vector3(1, -1, -1) },
        { 7, new Vector3(-1, -1, -1) },
    };
    List<NeighborVertices> neighborVertices = new List<NeighborVertices>();
    #endregion NonSerialize
    #endregion Field/Properties
    #region Methods
    #region UnityMethods
    private void Start()
    {
        mesh = new Mesh();
        DrawCube();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void DrawCube()
    {
        vertices.Add(Vector3.zero);
        vertices.Add(Vector3.right);
        vertices.Add(Vector3.up);
        vertices.Add((Vector3.up + Vector3.right));

        vertices.Add(Vector3.forward);
        vertices.Add((Vector3.forward + Vector3.right));
        vertices.Add((Vector3.forward + Vector3.up));
        vertices.Add((Vector3.forward + Vector3.up + Vector3.right) );
        SetTriangleToMakeCube();
    }
    void SetTriangleToMakeCube()
    {
        int _baseStart = 0;
        int _baseEnd = 7;
        List<int> neighborVertice0 = new List<int>() { _baseStart + dirValue[Vector3.forward] * (int)valueCoef[_baseStart].x, _baseStart + dirValue[Vector3.right] * (int)valueCoef[_baseStart].y, _baseStart + dirValue[Vector3.up] * (int)valueCoef[_baseStart].x };
        List<int> neighborVertice7 = new List<int>() { _baseEnd + dirValue[Vector3.up] * (int)valueCoef[_baseEnd].z, _baseEnd + dirValue[Vector3.right] * (int)valueCoef[_baseEnd].y, _baseEnd + dirValue[Vector3.forward] * (int)valueCoef[_baseEnd].x };
        neighborVertices.Add(new NeighborVertices(_baseStart, neighborVertice0, neighborVertice7));
        neighborVertices.Add(new NeighborVertices(_baseEnd, neighborVertice7, neighborVertice0));
        for (int i = 0; i < 2; i++)
        {
            int z = 2;
            int y = 0;
            int x = 1;
            NeighborVertices _neighborVertices = neighborVertices[i];
            for (int j = 0; j < 3; j++)
            {
                triangles.Add(_neighborVertices.vertice);
                triangles.Add(_neighborVertices.neighbor[j]);
                triangles.Add(_neighborVertices.neighbor[z]);//2 //0 // 1

                triangles.Add(_neighborVertices.neighborAngle[j]);
                triangles.Add(_neighborVertices.neighbor[y]);//0 //2 //1
                triangles.Add(_neighborVertices.neighbor[x]);//1 //0 //2 
                z = z > 1 ? 0 : z + 1;
                y = y <= 0 ? 2 : y - 1;
                x = x <= 0 ? 2 : x - 1;
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < vertices.Count; i++)
            Gizmos.DrawCube(vertices[i],Vector3.one * 0.05f);
    }
    #endregion UnityMethods
    #region CustomMethods
    #endregion CustomMethods
    #endregion Methods
}
