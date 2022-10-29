using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(MeshRenderer), typeof(MeshFilter))]
public class ChunkFinalC : MonoBehaviour
{
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] MeshData meshData;
    [SerializeField] ChunkParamFinal chunkParam;
    [SerializeField] Vector2Int indexChunk;

    [SerializeField] List<BlockDataC> blockRender = new List<BlockDataC>();
    Dictionary<Vector2Int, ChunkFinalC> neighborChunk = new Dictionary<Vector2Int, ChunkFinalC>();
    BlockDataC[,,] blocks;

    public Vector2Int IndexChunk => indexChunk;
    public BlockDataC[,,] Blocks => blocks;
    public Vector3Int WorldPosition => new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
    public int ChunkSize => chunkParam.chunkSize;
    public int ChunkHeight => chunkParam.chunkHeight;

    public void SetChunk(int _indexChunkX, int _indexChunkZ)
    {
        meshData = new MeshData();
        chunkParam = ChunkManagerFinalC.Instance.ChunkParam;
        blocks = new BlockDataC[chunkParam.chunkSize, chunkParam.chunkHeight, chunkParam.chunkSize];
        SetIndexChunk(_indexChunkX, _indexChunkZ);
        //ThreadManager.Instance.AddThread(GenerateBlocks);
    }
    public void SetIndexChunk(int _x, int _z) => indexChunk = new Vector2Int(_x, _z);
    public void GenerateBlocks()
    {
        Direction.RunThroughAllDirection2D(AddNeighborChunkFromDirection);
        int _chunkSize = chunkParam.chunkSize;
        for (int x = 0; x < _chunkSize; x++)
        {
            for (int z = 0; z < _chunkSize; z++)
            {
                float _perlinNoise = ChunkManagerFinalC.Instance.PerlinNoiseOctaves(indexChunk.x * _chunkSize + x, indexChunk.y * _chunkSize + z);
                float _groundPos = Mathf.RoundToInt(_perlinNoise * ChunkManagerFinalC.Instance.WorldParam.chunkHeightMax);
                for (int y = 0; y < ChunkManagerFinalC.Instance.ChunkParam.chunkHeight; y++)
                {
                    BlockType _blockType = BlockType.Air;
                    if(y == _groundPos)
                        _blockType = BlockType.Grass_Dirt;
                    else if(y < _groundPos)
                        _blockType = BlockType.Dirt;

                    if(_blockType != BlockType.Air)
                        blocks[x, y, z] = new BlockDataC(new Vector3Int(x,y,z), this, _blockType);
                    if (y == _groundPos)
                    {
                        blockRender.Add(blocks[x, y, z]);                
                    }
                }
            }
        }
    }
    void AddNeighborChunkFromDirection(Vector2Int _direction)
    {
        Vector2Int _indexChunk = indexChunk + _direction;
        ChunkFinalC _chunkNeighbor = ChunkManagerFinalC.Instance.GetChunkFromIndexChunk(_indexChunk);
        if (_chunkNeighbor) neighborChunk.Add(_direction, _chunkNeighbor);
    }
    public void UpdateMesh()
    {
        RenderMesh();
    }
    public void RenderMesh()
    {
        int _count = blockRender.Count;
        for (int i = 0; i < _count; i++)
            BlockRenderFace(blockRender[i]);
        meshData.UpdateMesh(meshCollider, meshFilter);
    }
    void BlockRenderFace(BlockDataC _block)
    {
        Vector3Int _blockPos = _block.positionBlock + WorldPosition;
        Direction.RunThroughAllDirection((_direction) =>
        {
            BlockDataC _blockDataNeighbor = BlockManagerC.Instance.GetBlockFromBlockWorldPosition(_blockPos + _direction);
            if (!_blockDataNeighbor /*&& _blockDataNeighbor.blockType == BlockType.Air*/)
            {
                Vector3 _faceDirPos = _direction; 
                Vector3 _faceCenter = _block.positionBlock + _faceDirPos * 0.5f;
                meshData.AddFace(_faceCenter, _direction, _block.blockType);
            }
        });
    }

    public void DestroyWorldPositionRadius(Vector3 _blockPos, Vector3 _normal, int radius)
    {
        List<BlockDataC> _toRender = new List<BlockDataC>();
        BlockDataC _blockData = BlockManagerC.Instance.GetBlockFromWorldPosition(_blockPos, _normal);
        Vector3Int _blockWorldPos = BlockManagerC.Instance.GetBlockPositionWorldFromBlock(_blockData);
        List<ChunkFinalC> chunksToUpdate = new List<ChunkFinalC>();
        chunksToUpdate.Add(this);
        for (int x = -radius; x < radius; x++)
        {
            for (int z = -radius; z < radius; z++)
            {
                for (int y = -radius; y < radius; y++)
                {
                    Vector3Int _blockDataPos = _blockWorldPos - new Vector3Int(x,y,z);
                    if ((_blockDataPos - _blockWorldPos).sqrMagnitude > radius * radius) continue;
                    BlockDataC _blockDataNeighbor = BlockManagerC.Instance.GetBlockFromBlockWorldPosition(_blockDataPos);
                    if (!_blockDataNeighbor) continue;
                    _blockDataNeighbor.DestroyBlock();
                    chunksToUpdate.Add(_blockDataNeighbor.owner);
                    if (x == -radius)
                    {
                        blockRender.Add(BlockManagerC.Instance.GetBlockFromBlockWorldPosition(_blockDataPos - new Vector3Int(1,0,0)));

                    }
                    if (x == radius - 1)
                        blockRender.Add(BlockManagerC.Instance.GetBlockFromBlockWorldPosition(_blockDataPos + new Vector3Int(1,0,0)));
                }
            }
        }

        int _count = chunksToUpdate.Count;
        for (int i = 0; i < _count; i++)
        {
            chunksToUpdate[i].meshData.ResetVerticesAndTriangles();
            chunksToUpdate[i].RenderMesh();
        }
    }
    public void DestroyWorldPositionBlock(Vector3 _blockPos, Vector3 _normal)
    {
        BlockDataC _blockData = BlockManagerC.Instance.GetBlockFromWorldPosition(_blockPos, _normal);
        if (!_blockData) return;
        if (!blockRender.Remove(_blockData))
        {
            Debug.LogError("Failed Remove!!!");
        }
        List<ChunkFinalC> chunksToUpdate = new List<ChunkFinalC>();
        Vector3Int _worldPosition = BlockManagerC.Instance.GetBlockPositionWorldFromBlock(_blockData);
        Direction.RunThroughAllDirection((_direction) =>
        {
            BlockDataC _blockNeighbor = BlockManagerC.Instance.GetBlockFromBlockWorldPosition(_worldPosition + _direction);
            if (!_blockNeighbor) return;
            if (!chunksToUpdate.Contains(_blockNeighbor.owner))
                chunksToUpdate.Add(_blockNeighbor.owner);
            _blockNeighbor.owner.blockRender.Add(_blockNeighbor);
        });
        _blockData = null;
        meshData.ResetVerticesAndTriangles();
        chunksToUpdate.Add(this);
        int _count = chunksToUpdate.Count;
        for (int i = 0; i < _count; i++) 
        {
            chunksToUpdate[i].meshData.ResetVerticesAndTriangles();
            chunksToUpdate[i].RenderMesh();
        }
    }
    public void CreateWorldPositionBlock(Vector3 _blockPos, Vector3 _normal)
    {
        BlockDataC _blockData = BlockManagerC.Instance.GetBlockFromWorldPosition(_blockPos, _normal);
        if (!_blockData) return;
        if (!_blockData.owner.blockRender.Remove(_blockData))
        {
            Debug.Log("Failed Remove!!!");
        }
        List<ChunkFinalC> chunksToUpdate = new List<ChunkFinalC>();
        chunksToUpdate.Add(this);
        Vector3Int _worldPosition = BlockManagerC.Instance.GetBlockPositionWorldFromBlock(_blockData);
        Direction.RunThroughAllDirection((_direction) =>
        {
            BlockDataC _blockNeighbor = BlockManagerC.Instance.GetBlockFromBlockWorldPosition(_worldPosition + _direction);
            if (_blockNeighbor) return;

            ChunkFinalC _chunkOwner = ChunkManagerFinalC.Instance.GetChunkFromWorldPosition(_worldPosition + _direction);
            if(!chunksToUpdate.Contains(_chunkOwner))
                chunksToUpdate.Add(_chunkOwner);
            _blockNeighbor = new BlockDataC(_worldPosition + _direction, _chunkOwner, BlockType.Dirt);
            _chunkOwner.blockRender.Add(_blockNeighbor);
        });
        _blockData.DestroyBlock();

        int _count = chunksToUpdate.Count;
        for (int i = 0; i < _count; i++)
        {
            chunksToUpdate[i].meshData.ResetVerticesAndTriangles();
            chunksToUpdate[i].RenderMesh();
        }
    }

    public void RunThroughAllBlocks(Action<Vector3Int> _blockPos)
    {
        for (int x = 0; x < chunkParam.chunkSize; x++)
            for (int z = 0; z < chunkParam.chunkSize; z++)
                for (int y = 0; y < chunkParam.chunkHeight; y++)
                    _blockPos?.Invoke(new Vector3Int(x, y, z));
    }
    public bool IsBlockPosInChunk(Vector3Int _blockPos)
    {
        return (_blockPos.x >= 0 && _blockPos.x < chunkParam.chunkSize) &&
               (_blockPos.y >= 0 && _blockPos.y < chunkParam.chunkHeight) &&
               (_blockPos.z >= 0 && _blockPos.z < chunkParam.chunkSize);
    }
    public bool GetChunkNeighbor(Vector2Int _direction, out ChunkFinalC _neighbor)
    {
        if(neighborChunk.ContainsKey(_direction))
            _neighbor = neighborChunk[_direction];
        else
            _neighbor = null;
        return _neighbor != null;
    }
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.black;
        Gizmos.DrawWireMesh(meshData.Mesh, transform.position);
        Gizmos.color = Color.red;
        foreach (var item in neighborChunk)
            Gizmos.DrawWireMesh(item.Value.meshData.Mesh, item.Value.transform.position);
    }
}
