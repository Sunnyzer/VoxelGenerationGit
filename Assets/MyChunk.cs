using System.Collections.Generic;
using UnityEngine;

public class MyChunk : MonoBehaviour
{
    int chunkHeight = 30;
    int chunkSize = 8;
    float noiseScale = 0.03f;
    Mesh chunkMesh;
    [SerializeField] int waterThreshold = 20;
    [SerializeField] BlockType[,,] blocks;
    int blockSize = 1;
    int countBlocks = 0;
    [SerializeField] bool drawGizmos = true;
    [SerializeField] bool testDrawGizmos = false;
    [SerializeField] List<MyChunk> chunkNeighbor = new List<MyChunk>();
    public static List<Vector3Int> direction = new List<Vector3Int>()
    { 
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down,
    };
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangle = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    public void Init(float _noiseScale, int _chunkSize,int _chunckHeight)
    {
        noiseScale = _noiseScale;
        chunkHeight = _chunckHeight;
        chunkSize = _chunkSize;
        GenerateVoxels();
    }
    private void GenerateVoxels()
    {
        countBlocks = chunkSize * chunkSize * chunkHeight;
        blocks = new BlockType[chunkSize,chunkHeight,chunkSize];
        GenerateBlocksChunk();
        MakeMesh();
        chunkMesh = new Mesh();
        chunkMesh.vertices = vertices.ToArray();
        chunkMesh.triangles = triangle.ToArray();
        chunkMesh.SetUVs(0,uvs);
        chunkMesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = chunkMesh;
        GetComponent<MeshCollider>().sharedMesh = chunkMesh;
    }
    void GenerateBlocksChunk()
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                float noiseValue = Mathf.PerlinNoise((ChunkManager.x + transform.position.x + x) * noiseScale, (ChunkManager.y + transform.position.z + z) * noiseScale);
                int groundPosition = Mathf.RoundToInt(noiseValue * chunkHeight);
                BlockType voxelType = BlockType.Dirt;
                for (int y = 0; y < chunkHeight; y++)
                {
                    if (y > groundPosition)
                    {
                        if (y < waterThreshold)
                            voxelType = BlockType.Water;
                        else
                            voxelType = BlockType.Air;
                    }
                    else if (y == groundPosition)
                    {
                        voxelType = BlockType.Grass_Dirt;
                    }
                    //int index = GetIndexFromPosition(x, y, z);
                    blocks[x,y,z] = voxelType;
                }
            }
        }
    }
    void MakeMesh()
    {
        for (int x = 0; x < chunkSize; x++)
            for (int z = 0; z < chunkSize; z++)
                for (int y = 0; y < chunkHeight; y++)
                {
                    Vector3Int[] directionToFill = IsBlockFill(new Vector3Int(x,y,z));
                    if (directionToFill.Length != 0 && blocks[x,y,z] != BlockType.Air)
                        foreach (Vector3 _dir in directionToFill)
                        {
                            Vector3 _test = _dir;
                            bool _upVector = Vector3.up == _test || Vector3.down == _test;
                            Vector3 _right = Quaternion.AngleAxis(90, _upVector ? Vector3.right : Vector3.up) * _test;
                            Vector3 _addUp = _upVector ? Vector3.right : Vector3.up;
                            vertices.Add(new Vector3Int(x, y, z) + _test * 0.5f + _addUp * 0.5f + _right * 0.5f);
                            vertices.Add(new Vector3Int(x, y, z) + _test * 0.5f + _addUp * 0.5f - _right * 0.5f);

                            vertices.Add(new Vector3Int(x, y, z) + _test * 0.5f - _addUp * 0.5f + _right * 0.5f);
                            vertices.Add(new Vector3Int(x, y, z) + _test * 0.5f - _addUp * 0.5f - _right * 0.5f);

                            triangle.Add(vertices.Count - 4);
                            triangle.Add(vertices.Count - 3);
                            triangle.Add(vertices.Count - 2);

                            triangle.Add(vertices.Count - 3);
                            triangle.Add(vertices.Count - 1);
                            triangle.Add(vertices.Count - 2);
                        }
                }
    }
    public int GetIndexFromPosition(int x,int y,int z)
    {
        return x + z * chunkSize + chunkHeight * y;
    }
    public Vector3Int GetPositionFromIndex(int _index) 
    {
        int y = _index / chunkHeight;
        _index = _index - chunkHeight * y;
        int z = _index / chunkSize;
        _index = _index - chunkSize * z;
        int x = _index;
        return new Vector3Int(x,y,z);
    }    
    bool IsPositionInChunk(Vector3Int _position)
    {
        if(_position.z < 0 || _position.z >= chunkSize)
            return false;
        if(_position.x < 0 || _position.x >= chunkSize)
            return false;
        if(_position.y < 0 || _position.y >= chunkHeight)
            return false;
        return true;
    }
    Vector3Int[] IsBlockFill(Vector3Int _pos)
    {
        List<Vector3Int> _directionFill = new List<Vector3Int>();
        foreach (Vector3Int _dir in direction)
        {
            Vector3Int _posDir = _pos + _dir;
            if (IsPositionInChunk(_posDir))
            {
                if (blocks[_posDir.x, _posDir.y, _posDir.z] == BlockType.Air || blocks[_posDir.x, _posDir.y, _posDir.z] == BlockType.Water)
                {
                    _directionFill.Add(_dir);
                }
            }
            //else if (_dir != Vector3.up && _dir != Vector3.down)
            //{
            //    Vector3 _posCube = transform.position + _dir * chunkSize;
            //    MyChunk _chunkNeighbor = ChunkManager.Instance.GetChunk((int)_posCube.x, (int)_posCube.z);
            //    if (!_chunkNeighbor) continue;
            //    if (!chunkNeighbor.Contains(_chunkNeighbor))
            //        chunkNeighbor.Add(_chunkNeighbor);
            //    Vector3Int _cal =  _pos - new Vector3Int((chunkSize-1) * _dir.x, _dir.y, (chunkSize - 1) * _dir.z);
            //    if (_chunkNeighbor.blocks[_cal.x, _cal.y, _cal.z] == BlockType.Air)
            //    {
            //        _directionFill.Add(_dir);
            //    }
            //}
        }
        return _directionFill.ToArray();
    }
    private void OnDrawGizmos()
    {
        if (!testDrawGizmos) return;
        for (int i = 0; i < vertices.Count; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawCube(transform.position + vertices[i], Vector3.one * 0.05f);
        }
        if (!drawGizmos) return;
        for (int y = 0; y < chunkHeight; y++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    switch (blocks[x,y,z])
                    {
                        case BlockType.Air:
                            continue;
                        case BlockType.Grass_Dirt:
                            Gizmos.color = Color.green;
                            break;
                        case BlockType.Dirt:
                            Gizmos.color = Color.yellow;
                            break;
                        case BlockType.Water:
                            Gizmos.color = Color.blue;
                            break;
                    }
                    Gizmos.DrawCube(transform.position + new Vector3(x,y,z),Vector3.one);
                }
            }
        }
    }
}
