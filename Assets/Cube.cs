using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public struct Dir
{
    public int pas;
    public int move;
    public int first;
    public int end;
    public Dir(int _pas, int _move, int _first, int _end)
    {
        pas = _pas;
        move = _move;
        first = _first;
        end = _end;
    }

}

enum EShape
{
    CubeShape,
    TriangleShape,
}
public class Cube : MonoBehaviour
{
    #region Field/Properties
    #region Event
    #endregion Event
    #region NonSerialize
    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    Dictionary<Vector3, Dir> dirValue = new Dictionary<Vector3, Dir>()
    {
        { Vector3.forward, new Dir(4,1,0,3) },
        { Vector3.right, new Dir(1,2,0,6) },
        { Vector3.up, new Dir(2,4,0,5) },
    };

    #endregion NonSerialize
    #region Serialize
    [SerializeField,Range(0.1f, 3)] float cubeSize = 1;
    #endregion Serialize
    #region Properties
    #endregion Properties
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
    }

    private void DrawCube()
    {
        vertices.Add(Vector3.zero);
        vertices.Add(Vector3.right * cubeSize);
        vertices.Add(Vector3.up * cubeSize);
        vertices.Add((Vector3.up + Vector3.right) * cubeSize);

        vertices.Add(Vector3.forward * cubeSize);
        vertices.Add((Vector3.forward + Vector3.right) * cubeSize);
        vertices.Add((Vector3.forward + Vector3.up) * cubeSize);
        vertices.Add((Vector3.forward + Vector3.up + Vector3.right) * cubeSize);
        SetTriangleToMakeCube();
    }
    void SetTriangleToMakeCube()
    {
        foreach (var item in dirValue)
        {
            int move = item.Value.move;
            int pas = item.Value.pas;
            int first = item.Value.first;
            int end = item.Value.end;
            int tempFirst = first;
            int tempEnd = end;
            for (int i = 0; i < 2; i++)
            {
                SetFace(tempFirst,first,tempEnd,end,move,pas);
                first += pas;
                end += pas;
                tempFirst = end;
                tempEnd = first;
            }
        }
    }
    void SetFace(int tempFirst, int first, int tempEnd, int end, int move, int pas)
    {
        triangles.Add(tempFirst); triangles.Add(end - move); triangles.Add(first + move);
        triangles.Add(tempEnd); triangles.Add(first + move); triangles.Add(end - move);
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
