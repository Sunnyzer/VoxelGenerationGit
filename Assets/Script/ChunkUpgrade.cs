using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

[Serializable]
public class BlockData
{
    public Vector3Int position;
    public BlockType blockType = BlockType.Nothing;
    public BlockData[] neighborBlockData;
    public List<int> placementTriangles = new List<int>();
    public List<int> placementVertices = new List<int>(); 
    public ChunkUpgrade owner;
    public BlockData(BlockType _blockType, Vector3Int _pos,ChunkUpgrade _owner)
    {
        blockType = _blockType;
        position = _pos;
        owner = _owner;
    }
}

public class ChunkUpgrade : MonoBehaviour
{
    public static List<Vector3Int> diagonalDirection = new List<Vector3Int>()
    {
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.forward + Vector3Int.left,
        Vector3Int.forward + Vector3Int.right,
        Vector3Int.back + Vector3Int.right,
        Vector3Int.back + Vector3Int.left
    };
    public static List<Vector3Int> direction = new List<Vector3Int>()
    {
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.right,
        Vector3Int.left
    };
    public static List<Vector3Int> allDirection = new List<Vector3Int>()
    {
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down
    };
    
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] List<Vector3> chunksVertices = new List<Vector3>();
    [SerializeField] List<int> chunksTriangles = new List<int>();
    [SerializeField] List<Vector2> uvs = new List<Vector2>();
    [SerializeField] public Vector2Int chunksIndex;
    Dictionary<Vector3Int, ChunkUpgrade> neighborChunk = new Dictionary<Vector3Int, ChunkUpgrade>()
    {
        { Vector3Int.forward, null },
        { Vector3Int.back, null },
        { Vector3Int.right, null },
        { Vector3Int.left, null },
        { Vector3Int.forward + Vector3Int.left, null },
        { Vector3Int.forward + Vector3Int.right, null },
        { Vector3Int.back + Vector3Int.right, null },
        { Vector3Int.back + Vector3Int.left, null },
    };
    BlockData[,,] blocks;
    public BlockData[,,] BlockDatas => blocks;
    Mesh chunkMesh;
    float noiseScale = 0.03f;
    int chunkHeight = 30;
    int chunkSize = 8;
    int waterThreshold = 20;

    BlockData blockDebug;
    [SerializeField] private bool onDebug;

    public IEnumerator Init(float _noiseScale, int _chunkSize, int _chunckHeight)
    {
        noiseScale = _noiseScale;
        chunkHeight = _chunckHeight;
        chunkSize = _chunkSize;
        chunksIndex = new Vector2Int((int)transform.position.x / _chunkSize, (int)transform.position.z / _chunkSize);
        yield return GenerateVoxelsBlocks();
    }
    private IEnumerator GenerateVoxelsBlocks()
    {
        blocks = new BlockData[chunkSize, chunkHeight, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                float noiseValue = Mathf.PerlinNoise((ChunkManagerUpgrade.x + transform.position.x + x) * noiseScale, (ChunkManagerUpgrade.y + transform.position.z + z) * noiseScale);
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
                    blocks[x, y, z] = new BlockData(voxelType, new Vector3Int(x, y, z), this);
                }
            }
            yield return null;
        }
    }
    private void SetNeighBorChunk()
    {
        foreach (Vector3Int dir in diagonalDirection)
        {
            Vector2Int _chunkPos = new Vector2Int(chunksIndex.x + dir.x, chunksIndex.y + dir.z);
            ChunkUpgrade _chunkNeighbor = ChunkManagerUpgrade.Instance.GetChunk(_chunkPos.x, _chunkPos.y);
            if (!_chunkNeighbor) continue;
            neighborChunk[dir] = _chunkNeighbor;
        }
    }
    public IEnumerator SetMakeMesh()
    {
        SetNeighBorChunk();
        chunksVertices.Clear();
        chunksTriangles.Clear();
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                for (int y = 0; y < chunkHeight; y++)
                {
                    Vector3Int _blockPos = new Vector3Int(x, y, z);
                    GetAllNeighBorBlock(_blockPos, out blocks[x, y, z].neighborBlockData);
                    MakeCube(_blockPos);
                }
            }
        }
        RenderMesh();
        yield break;
    }
    public void UpdateMesh()
    {
        chunksVertices.Clear();
        chunksTriangles.Clear();
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                for (int y = 0; y < chunkHeight; y++)
                {
                    Vector3Int _blockPos = new Vector3Int(x, y, z);
                    MakeCube(_blockPos);
                }
            }
        }
        RenderMesh();
    }

    public void DestroyBlock(Vector3Int _pos)
    {
        BlockData _blockData = ChunkManagerUpgrade.Instance.GetBlockDataFromWorldPosition(_pos);
        if (_blockData == null) return;
        _blockData.blockType = BlockType.Air;
        Debug.Log(_blockData.placementTriangles + " " + _blockData.placementVertices);
        blockDebug = _blockData;
        onDebug = true;
    }
    public void DestroyBlockProfondeur(Vector3Int _pos, float _radius)
    {
        List<ChunkUpgrade> _toUpdate = new List<ChunkUpgrade>();
        _toUpdate.Add(this);
        int radiusRound = Mathf.RoundToInt(_radius);
        for (int x = -radiusRound; x < radiusRound; x++)
        {
            for (int z = -radiusRound; z < radiusRound; z++)
            {
                for (int y = -radiusRound; y < radiusRound; y++)
                {
                    Vector3Int _posBlock = new Vector3Int(_pos.x + x, _pos.y + y, _pos.z + z);
                    if ((_pos - _posBlock).sqrMagnitude >= _radius * _radius) continue;
                    if (IsBlockInChunk(_posBlock))
                    {
                        BlockData _blockData = ChunkManagerUpgrade.Instance.GetBlockDataFromWorldPosition(_posBlock);
                        if (_blockData == null) continue;
                        _blockData.blockType = BlockType.Air;
                    }
                    else
                    {
                        BlockData _blockData = ChunkManagerUpgrade.Instance.GetBlockDataFromWorldPosition(_posBlock);
                        if (_blockData == null)continue;
                        _blockData.blockType = BlockType.Air;
                        if(!_toUpdate.Contains(_blockData.owner))
                            _toUpdate.Add(_blockData.owner);
                    }
                }
            }
        }
        foreach (var item in diagonalDirection)
        {
            ChunkUpgrade _neighbor = neighborChunk[item];
            if (_neighbor && !_toUpdate.Contains(_neighbor))
                _toUpdate.Add(_neighbor);
        }
        for (int i = 0; i < _toUpdate.Count; i++)
        {
            _toUpdate[i].UpdateMesh();
        }
    }
    public bool IsBlockInChunk(Vector3Int _pos)
    {
        return (_pos.x < blocks.GetLength(0) && _pos.x >= 0) &&
               (_pos.y < blocks.GetLength(1) && _pos.y >= 0) &&
               (_pos.z < blocks.GetLength(2) && _pos.z >= 0);
    }
    void RenderMesh()
    {
        chunkMesh = new Mesh();
        chunkMesh.vertices = chunksVertices.ToArray();
        chunkMesh.triangles = chunksTriangles.ToArray();
        chunkMesh.SetUVs(0, uvs);
        chunkMesh.RecalculateNormals();
        meshFilter.mesh = chunkMesh;
        meshCollider.sharedMesh = chunkMesh;
    }
    void MakeCube(Vector3Int _pos)
    {
        BlockData currentBlock = blocks[_pos.x, _pos.y, _pos.z];
        if (currentBlock.blockType == BlockType.Air) return;
        BlockData[] _neighbor = currentBlock.neighborBlockData;
        foreach (BlockData _blockData in _neighbor)
        {
            if (_blockData.blockType != BlockType.Air) continue;
            Vector3 _blockPos = _blockData.position;
            Vector3 _direction = (_blockPos - currentBlock.position).normalized * (_blockData.owner == this ? 1 : -1);
            Vector3 _directionFace = _direction * 0.5f;
            bool _upVector = Vector3.up == _direction || Vector3.down == _direction;
            Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3.right : Vector3.up) * _directionFace;
            Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * 0.5f;
            Vector3 _facePos = _directionFace + _pos;

            chunksVertices.Add(_facePos + _directionUp + _directionRight);
            chunksVertices.Add(_facePos + _directionUp - _directionRight);
            chunksVertices.Add(_facePos - _directionUp + _directionRight);
            chunksVertices.Add(_facePos - _directionUp - _directionRight);

            chunksTriangles.Add(chunksVertices.Count - 4);
            chunksTriangles.Add(chunksVertices.Count - 3);
            chunksTriangles.Add(chunksVertices.Count - 2);

            chunksTriangles.Add(chunksVertices.Count - 3);
            chunksTriangles.Add(chunksVertices.Count - 1);
            chunksTriangles.Add(chunksVertices.Count - 2);
            currentBlock.placementTriangles.Add(chunksTriangles.Count - 6);
            currentBlock.placementVertices.Add(chunksVertices.Count - 4);

        }
    }
    void GetAllNeighBorBlock(Vector3Int _blockPos, out BlockData[] _blockDatas)
    {
        List<BlockData> _tempBlockDatas = new List<BlockData>();
        foreach (Vector3Int dir in allDirection)
        {
            Vector3Int _blockSidePos = _blockPos + dir;
            if(IsBlockInChunk(_blockSidePos))
                _tempBlockDatas.Add(blocks[_blockSidePos.x, _blockSidePos.y, _blockSidePos.z]);
            else if(dir != Vector3Int.up && dir != Vector3Int.down)
            {
                Vector3Int _blockNeightBor = _blockPos - new Vector3Int((chunkSize - 1) * dir.x, 0, (chunkSize - 1) * dir.z);
                ChunkUpgrade _neighbor = neighborChunk[dir];
                if (!_neighbor) continue;
                _tempBlockDatas.Add(_neighbor.blocks[_blockNeightBor.x, _blockNeightBor.y, _blockNeightBor.z]);
            }
        }
        _blockDatas = _tempBlockDatas.ToArray();
    }

    public Vector3Int GetPositionBlockFromWorldPosition(Vector3 _pos, Vector3 _normal)
    {
        Vector3 _posNormal = _pos - _normal * 0.5f;
        return new Vector3Int(Mathf.RoundToInt(_posNormal.x), Mathf.RoundToInt(_posNormal.y), Mathf.RoundToInt(_posNormal.z));
    }
    public Vector3Int GetPositionBlockInChunkFromClampPosition(Vector3Int _pos)
    {
        return _pos - new Vector3Int(chunksIndex.x * (chunkSize), 0, chunksIndex.y * (chunkSize));
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireMesh(chunkMesh,0,transform.position);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (!onDebug) return;
        for (int i = 0; i < blockDebug.placementTriangles.Count; i++) 
        {
            for (int j = 0; j < 6; j++)
            {
                Gizmos.DrawCube(transform.position + chunksVertices[chunksTriangles[j + blockDebug.placementTriangles[i]]] ,Vector3.one * 0.05f);
            }
        }
    }
}
