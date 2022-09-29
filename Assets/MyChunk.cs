using System.Collections.Generic;
using UnityEngine;

public class MyChunk : MonoBehaviour
{
    int chunkHeight = 30;
    int chunkSize = 8;
    float noiseScale = 0.03f;
    Mesh chunkMesh;
    [SerializeField] int waterThreshold = 20;
    [SerializeField] BlockType[] blocks;
    [SerializeField] int blockSize = 1;
    int countBlocks = 0;
    [SerializeField] bool drawGizmos = true;
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
        blocks = new BlockType[countBlocks];
        blockSize = ChunkManager.Instance.blockSize;
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                float noiseValue = Mathf.PerlinNoise((ChunkManager.x + transform.position.x + x) * noiseScale, (ChunkManager.y + transform.position.z + z) * noiseScale);
                int groundPosition = Mathf.RoundToInt(noiseValue * chunkHeight);
                for (int y = 0; y < chunkHeight; y++)
                {
                    BlockType voxelType = BlockType.Dirt;
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
                    int index = GetIndexFromPosition(x, y, z);
                    blocks[index] = voxelType;
                }
            }
        }
        for (int i = 0; i < countBlocks; i++)
        {
            Vector3Int _cubePos = GetPositionFromIndex(i);
            Vector3Int[] directionToFill = IsBlockFill(_cubePos);
            if(directionToFill.Length != 0 && blocks[i] != BlockType.Air)
            foreach (Vector3 _dir in directionToFill)
            {
                Vector3 _test = _dir;
                bool _upVector = Vector3.up == _test || Vector3.down == _test;
                Vector3 _right = Quaternion.AngleAxis(90, _upVector ? Vector3.right : Vector3.up) * _test;
                Vector3 _addUp = _upVector ? Vector3.right : Vector3.up;
                vertices.Add( _cubePos + _test * 0.5f + _addUp * 0.5f + _right * 0.5f);
                vertices.Add( _cubePos + _test * 0.5f + _addUp * 0.5f - _right * 0.5f);

                vertices.Add( _cubePos + _test * 0.5f - _addUp * 0.5f + _right * 0.5f);
                vertices.Add( _cubePos + _test * 0.5f - _addUp * 0.5f - _right * 0.5f);

                triangle.Add(vertices.Count - 4);   
                triangle.Add(vertices.Count - 3);   
                triangle.Add(vertices.Count - 2);
                    
                triangle.Add(vertices.Count - 3);   
                triangle.Add(vertices.Count - 1);   
                triangle.Add(vertices.Count - 2);   
            }
        }
        chunkMesh = new Mesh();
        chunkMesh.vertices = vertices.ToArray();
        chunkMesh.triangles = triangle.ToArray();
        chunkMesh.SetUVs(0,uvs);
        GetComponent<MeshFilter>().mesh = chunkMesh;
        GetComponent<MeshCollider>().sharedMesh = chunkMesh;
    }
    public int GetIndexFromPosition(int x,int y,int z)
    {
        return x + y * chunkSize + chunkSize * chunkHeight * z;
    }
    public int GetIndexFromPosition(Vector3Int _pos)
    {
        return _pos.x + _pos.y * chunkSize + chunkSize * chunkHeight * _pos.z;
    }
    public Vector3Int GetPositionFromIndex(int _index) 
    {
        int chunkHeightPas = chunkSize * chunkHeight;
        int z = _index / chunkHeightPas;
        _index = _index - chunkHeightPas * z;
        int y = _index / chunkSize;
        _index = _index - chunkSize * y;
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
                int _index = GetIndexFromPosition(_posDir.x, _posDir.y, _posDir.z);
                if (blocks[_index] == BlockType.Air || blocks[_index] == BlockType.Water)
                    _directionFill.Add(_dir);
            }
        }
        return _directionFill.ToArray();
    }
    private void OnDrawGizmos()
    {
        for (int i = 0; i < countBlocks; i++)
        {
            Gizmos.color = Color.black;
            if (i < vertices.Count)
                Gizmos.DrawCube(transform.position + vertices[i], Vector3.one * 0.05f);
            Vector3 _coef = Vector3.zero;
            Vector3 _scale = Vector3.one * blockSize;
            switch (blocks[i])
            {
                case BlockType.Nothing:
                    continue;
                case BlockType.Air:
                    continue;
                case BlockType.Grass_Dirt:
                    Gizmos.color = Color.green;
                    break;
                case BlockType.Dirt:
                    Gizmos.color = Color.yellow;
                    break;
                case BlockType.Water:
                    _scale = new Vector3(1,0.8f,1);
                    _coef = new Vector3(0,-0.2f,0);
                    Gizmos.color = Color.blue;
                    break;
            }
            Vector3Int _posBlock = GetPositionFromIndex(i);
            if (IsBlockFill(_posBlock).Length == 0) continue;
            if (!drawGizmos) continue;
            Gizmos.DrawCube(transform.position /*+ new Vector3(0.5f, 0.5f, 0.5f)*/ + _posBlock + _coef, _scale);
        }
    }
}
