using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlockData
{
    public Vector3Int position;
    public BlockType blockType = BlockType.Nothing;
    [HideInInspector] public BlockData[] neighborBlockData;
    public List<int> placementTriangles = new List<int>();
    public List<int> placementVertices = new List<int>(); 
    public OldChunk owner;
    public BlockData(BlockType _blockType, Vector3Int _pos,OldChunk _owner)
    {
        blockType = _blockType;
        position = _pos;
        owner = _owner;
    }
    public static bool operator! (BlockData _a) => _a == null;
}

class FaceVertices
{
    Vector3[] vertices = new Vector3[4];
    int[] triangles = new int[6];
}
public class OldChunk : MonoBehaviour
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
    Dictionary<Vector3Int, OldChunk> neighborChunk = new Dictionary<Vector3Int, OldChunk>()
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
    List<BlockData> blockRenderer = new List<BlockData>();
    public BlockData[,,] BlockDatas => blocks;
    Mesh chunkMesh;
    float noiseScale = 0.03f;
    int chunkHeight = 30;
    int chunkSize = 8;
    int waterThreshold = 20;

    List<BlockData> _blockDatas = new List<BlockData>();

    [SerializeField] private bool onDebug;
    private BlockData blockDebug;
    Dictionary<Vector3Int, FaceVertices> dir;
    public IEnumerator Init(float _noiseScale, int _chunkSize, int _chunckHeight)
    {
        noiseScale = _noiseScale;
        chunkHeight = _chunckHeight;
        chunkSize = _chunkSize;
        chunksIndex = OldChunkManager.Instance.GetChunkIndexFromWorldPosition(transform.position);
        blocks = new BlockData[chunkSize, chunkHeight, chunkSize];
        yield return GenerateVoxelsBlocks();
    }
    private IEnumerator GenerateVoxelsBlocks()
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                float noiseValue = Mathf.PerlinNoise((OldChunkManager.noisePosX + transform.position.x + x) * noiseScale, (OldChunkManager.noisePosY + transform.position.z + z) * noiseScale);
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
                    if (voxelType == BlockType.Grass_Dirt)
                        _blockDatas.Add(blocks[x, y, z]);
                }
            }
            yield return null;
        }
    }
    private void RunThroughAllBlocks(Action<Vector3Int> actionCallBlock)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                for (int y = 0; y < chunkHeight; y++)
                {
                    actionCallBlock?.Invoke(new Vector3Int(x, y, z));
                }
            }
        }
    }
    private void SetNeighBorChunk()
    {
        foreach (Vector3Int dir in diagonalDirection)
        {
            Vector2Int _chunkPos = new Vector2Int(chunksIndex.x + dir.x, chunksIndex.y + dir.z);
            OldChunk _chunkNeighbor = OldChunkManager.Instance.GetChunk(_chunkPos);
            if (!_chunkNeighbor) continue;
            neighborChunk[dir] = _chunkNeighbor;
        }
    }
    private void SetBlockData(Vector3Int _blockPos)
    {
        GetAllNeighBorBlock(_blockPos, out blocks[_blockPos.x, _blockPos.y, _blockPos.z].neighborBlockData);
        MakeCube(_blockPos);
    }
    public IEnumerator SetMakeMesh()
    {
        SetNeighBorChunk();
        chunksVertices.Clear();
        chunksTriangles.Clear();
        RunThroughAllBlocks(SetBlockData);
        RenderMesh();
        yield break;
    }
    public void UpdateMesh()
    {
        chunksVertices.Clear();
        chunksTriangles.Clear();
        RunThroughAllBlocks(MakeCube);
        RenderMesh();
    }

    public void DestroyBlock(Vector3Int _pos)
    {
        BlockData _blockData = OldChunkManager.Instance.GetBlockDataFromWorldPosition(_pos);
        if (_blockData == null) return;
        _blockData.blockType = BlockType.Air;
        onDebug = true;
        ChangeMesh(_blockData);
    }
    public void ChangeMesh(BlockData _blockData)
    {
        if (!_blockData) return;
        BlockData[] _neighbor = _blockData.neighborBlockData;
        List<OldChunk> _toUpdate = new List<OldChunk>();
        _toUpdate.Add(this);
        foreach (BlockData _blockDataNeighbor in _neighbor)
        {
            if (!_blockDataNeighbor || _blockDataNeighbor.blockType == BlockType.Air) continue;
            Vector3 _blockNeighborPos = _blockDataNeighbor.position;
            Vector3 _directionToPutVertices = (_blockNeighborPos - _blockData.position).normalized * (_blockDataNeighbor.owner == this ? 1 : -1);
            Vector3 _directionFace = _directionToPutVertices * 0.5f;
            bool _upVector = Vector3.up == _directionToPutVertices || Vector3.down == _directionToPutVertices;
            Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3.right : Vector3.up) * _directionFace;
            Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * 0.5f;
            Vector3 _facePos = _directionFace + _blockData.position;
            OldChunk _owner = _blockDataNeighbor.owner;
            if(!_toUpdate.Contains(_owner))
            {
                blockDebug = _blockDataNeighbor;
                _toUpdate.Add(_owner);
            }
            _directionToPutVertices = (_owner == this ? new Vector3(0, 0, 0) : -chunkSize * _directionToPutVertices);
            _owner.chunksVertices.Add(_directionToPutVertices + _facePos + _directionUp + _directionRight);
            _owner.chunksVertices.Add(_directionToPutVertices + _facePos + _directionUp - _directionRight);
            _owner.chunksVertices.Add(_directionToPutVertices + _facePos - _directionUp + _directionRight);
            _owner.chunksVertices.Add(_directionToPutVertices + _facePos - _directionUp - _directionRight);
            int _countVertices = _owner.chunksVertices.Count;
            _owner.chunksTriangles.Add(_countVertices - 2);
            _owner.chunksTriangles.Add(_countVertices - 3);
            _owner.chunksTriangles.Add(_countVertices - 4);

            _owner.chunksTriangles.Add(_countVertices - 2);
            _owner.chunksTriangles.Add(_countVertices - 1);
            _owner.chunksTriangles.Add(_countVertices - 3);

            _blockDataNeighbor.placementTriangles.Add(_owner.chunksTriangles.Count - 6);
            _blockDataNeighbor.placementVertices.Add(_countVertices - 4);
        }
        ResetVerticesAndTriangleInBlockData(_blockData);
        for (int i = 0; i < _toUpdate.Count; ++i)
            _toUpdate[i].UpdateVertices();
    }
    public void ResetVerticesAndTriangleInBlockData(BlockData _blockData)
    {
        int _count = _blockData.placementTriangles.Count;
        for (int i = 0; i < _count; ++i)
        {
            int _triangle = _blockData.placementTriangles[i];
            for (int j = 0; j < 6; ++j)
            {
                chunksVertices[chunksTriangles[j + _triangle]] = new Vector3(0, 0, 0);
                chunksTriangles[j + _triangle] = 0;
            }
        }
    }
    void UpdateVertices()
    {
        chunkMesh.vertices = chunksVertices.ToArray();
        chunkMesh.triangles = chunksTriangles.ToArray();
        chunkMesh.SetUVs(0, uvs);
        chunkMesh.RecalculateNormals();
        meshCollider.sharedMesh = chunkMesh;
    }
    public void DestroyBlockProfondeur(Vector3Int _pos, float _radius)
    {
        List<OldChunk> _toUpdate = new List<OldChunk>();
        _toUpdate.Add(this);
        int radiusRound = Mathf.RoundToInt(_radius);
        for (int x = -radiusRound; x < radiusRound; ++x)
        {
            for (int z = -radiusRound; z < radiusRound; ++z)
            {
                for (int y = -radiusRound; y < radiusRound; ++y)
                {
                    Vector3Int _posBlock = new Vector3Int(_pos.x + x, _pos.y + y, _pos.z + z);
                    if ((_pos - _posBlock).sqrMagnitude >= _radius * _radius) continue;
                    if (IsBlockInChunk(_posBlock))
                    {
                        BlockData _blockData = OldChunkManager.Instance.GetBlockDataFromWorldPosition(_posBlock);
                        if (!_blockData) continue;
                        _blockData.blockType = BlockType.Air;
                    }
                    else
                    {
                        BlockData _blockData = OldChunkManager.Instance.GetBlockDataFromWorldPosition(_posBlock);
                        if (!_blockData) continue;
                        _blockData.blockType = BlockType.Air;
                        if(!_toUpdate.Contains(_blockData.owner))
                            _toUpdate.Add(_blockData.owner);
                    }
                }
            }
        }
        foreach (var item in diagonalDirection)
        {
            OldChunk _neighbor = neighborChunk[item];
            if (_neighbor && !_toUpdate.Contains(_neighbor))
                _toUpdate.Add(_neighbor);
        }
        for (int i = 0; i < _toUpdate.Count; ++i)
            _toUpdate[i].UpdateMesh();
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
            if(!blockRenderer.Contains(currentBlock))
                blockRenderer.Add(currentBlock);
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
                OldChunk _neighbor = neighborChunk[dir];
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
        return _pos - new Vector3Int(chunksIndex.x * chunkSize, 0, chunksIndex.y * chunkSize);
    }
}
