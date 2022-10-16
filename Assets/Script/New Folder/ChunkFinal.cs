using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Mesh;

[Serializable]
public struct ChunkParamFinal
{
    public int chunkSize;
    public int chunkHeight;
    public float sizeBlock;
    public Vector2Int indexChunk;
    public ChunkParamFinal(int _chunkSize, int _chunkHeight, float _sizeBlock, Vector2Int _indexChunk)
    {
        chunkSize = _chunkSize;
        chunkHeight = _chunkHeight;
        sizeBlock = _sizeBlock;
        indexChunk = _indexChunk;
    }
}

[RequireComponent(typeof(MeshCollider), typeof(MeshRenderer), typeof(MeshFilter))]
public class ChunkFinal : MonoBehaviour
{
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] MeshData meshData;
    [SerializeField] BlockData blockData;
    [SerializeField] Vector3Int pos;
    [SerializeField] ChunkParamFinal chunkParam;

    [SerializeField] List<BlockData> blockRender = new List<BlockData>();
    Dictionary<Vector2Int, ChunkFinal> neighborChunk = new Dictionary<Vector2Int, ChunkFinal>();
    BlockData[,,] blocks;

    public BlockData[,,] Blocks => blocks;
    public Vector3Int WorldPosition => new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
    public int ChunkSize => chunkParam.chunkSize;
    public int ChunkHeight => chunkParam.chunkHeight;

    public IEnumerator GenerateBlocks(ChunkParamFinal _chunkParam)
    {
        meshData = new MeshData(this);
        chunkParam = _chunkParam;
        int _chunkSize = chunkParam.chunkSize;
        blocks = new BlockData[_chunkSize, chunkParam.chunkHeight, _chunkSize];
        for (int x = 0; x < _chunkSize; x++)
        {
            for (int z = 0; z < _chunkSize; z++)
            {
                float _perlinNoise = ChunkManagerFinal.Instance.PerlinNoiseOctaves(WorldPosition.x + x, WorldPosition.z + z);
                float _groundPos = Mathf.RoundToInt(_perlinNoise * chunkParam.chunkHeight);
                for (int y = 0; y < chunkParam.chunkHeight; y++)
                {
                    BlockType _blockType = BlockType.Air;
                    if(y == _groundPos)
                        _blockType = BlockType.Grass_Dirt;
                    else if(y < _groundPos)
                        _blockType = BlockType.Dirt;
                    
                    blocks[x, y, z] = new BlockData(new Vector3Int(x,y,z), this, _blockType);
                }
            }
            yield return null;
        }
    }
    public IEnumerator InitChunks()
    {
        Direction.RunThroughAllDirection2D(AddNeighborChunk);
        RunThroughAllBlocks((_blockPos) =>
        {
            BlockData _blockData = blocks[_blockPos.x, _blockPos.y, _blockPos.z];
            _blockData.SetNeighbor(this);
            if (_blockData.blockType == BlockType.Air) return;
            foreach (var item in _blockData.blocksNeighbor)
            {
                if (item.Value.blockType != BlockType.Air) continue;
                blockRender.Add(_blockData);
                break;
            }
        });
        yield return null;
    }
    void AddNeighborChunk(Vector2Int _direction)
    {
        Vector2Int _indexChunk = chunkParam.indexChunk + _direction;
        ChunkFinal _chunkNeighbor = ChunkManagerFinal.Instance.GetChunkFromIndexChunk(_indexChunk);
        if (_chunkNeighbor) neighborChunk.Add(_direction, _chunkNeighbor);
    }
    public IEnumerator UpdateMesh()
    {
        RenderMesh();
        yield return null;
    }
    public void RenderMesh()
    {
        int _count = blockRender.Count;
        for (int i = 0; i < _count; i++)
            BlockRenderFace(blockRender[i]);
        meshData.UpdateMesh(meshCollider, meshFilter);
    }
    void BlockRenderFace(BlockData _block)
    {
        Dictionary<Vector3Int, BlockData> _blockNeighbor = _block.blocksNeighbor;
        foreach (var item in _blockNeighbor)
        {
            if (item.Value.blockType != BlockType.Air) continue;
            Vector3 _direction = item.Key;
            Vector3 _faceCenter = _block.positionBlock + _direction * 0.5f;
            Face _faceAddToMesh = meshData.AddFace(_faceCenter, item.Key, _block.blockType);
            _block.AddNewFace(item.Key, _faceAddToMesh);
        }
    }

    public void DestroyWorldPositionRadius(Vector3 _blockPos, Vector3 _normal, int radius)
    {
        List<BlockData> _toRender = new List<BlockData>();
        BlockData _blockData = BlockManager.Instance.GetBlockFromWorldPosition(_blockPos, _normal);
        Vector3Int _blockWorldPos = BlockManager.Instance.GetBlockPositionWorldFromBlock(_blockData);
        List<ChunkFinal> chunksToUpdate = new List<ChunkFinal>();
        chunksToUpdate.Add(this);
        for (int x = -radius; x < radius; x++)
        {
            for (int z = -radius; z < radius; z++)
            {
                for (int y = -radius; y < radius; y++)
                {
                    Vector3Int _blockDataPos = _blockWorldPos - new Vector3Int(x,y,z);
                    if ((_blockDataPos - _blockWorldPos).sqrMagnitude > radius * radius) continue;
                    BlockData _blockDataNeighbor = BlockManager.Instance.GetBlockFromBlockWorldPosition(_blockDataPos);
                    if (!_blockDataNeighbor || _blockDataNeighbor.blockType == BlockType.Air) continue;
                    _blockDataNeighbor.DestroyBlock();
                    if (!_blockDataNeighbor.owner.blockRender.Remove(_blockDataNeighbor))
                    {
                        Debug.LogWarning("Failed Remove!!!");
                    }
                    _toRender.AddRange(_blockDataNeighbor.blocksNeighbor.Values);
                }
            }
        }
        int _renderCount = _toRender.Count;
        for (int i = 0; i < _renderCount; i++)
        {
            BlockData _blockRender = _toRender[i];
            ChunkFinal _chunkFinal = _toRender[i].owner;
            if (_blockRender.blockType != BlockType.Air && !_chunkFinal.blockRender.Contains(_blockRender))
            {
                _chunkFinal.blockRender.Add(_blockRender);
                if (!chunksToUpdate.Contains(_chunkFinal))
                    chunksToUpdate.Add(_chunkFinal);
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
        BlockData _blockData = BlockManager.Instance.GetBlockFromWorldPosition(_blockPos, _normal);
        if (!_blockData) return;
        _blockData.DestroyBlock();
        if (!blockRender.Remove(_blockData))
        {
            Debug.LogError("Failed Remove!!!");
        }
        meshData.ResetVerticesAndTriangles();
        List<ChunkFinal> chunksToUpdate = new List<ChunkFinal>();
        chunksToUpdate.Add(this);
        foreach (var item in _blockData.blocksNeighbor)
        {
            if (item.Value.blockType != BlockType.Air && !item.Value.owner.blockRender.Contains(item.Value))
                item.Value.owner.blockRender.Add(item.Value);
            if(!chunksToUpdate.Contains(item.Value.owner))
                chunksToUpdate.Add(item.Value.owner);
        }
        int _count = chunksToUpdate.Count;
        for (int i = 0; i < _count; i++)
            chunksToUpdate[i].RenderMesh();
    }
    public void CreateWorldPositionBlock(Vector3 _blockPos, Vector3 _normal)
    {
        BlockData _blockData = BlockManager.Instance.GetBlockFromWorldPosition(_blockPos, _normal);
        if (!_blockData) return;
        if (!blockRender.Remove(_blockData))
        {
            Debug.LogError("Failed Remove!!!");
        }
        _blockData.facePerDirection.Clear();
        meshData.ResetVerticesAndTriangles();
        List<ChunkFinal> chunksToUpdate = new List<ChunkFinal>();
        chunksToUpdate.Add(this);
        foreach (var item in _blockData.blocksNeighbor)
        {
            BlockData _blockDataNeighbor = item.Value;
            ChunkFinal _chunkFinal = _blockDataNeighbor.owner;
            if (_blockDataNeighbor.blockType == BlockType.Air)
            {
                _chunkFinal.blockRender.Add(_blockDataNeighbor);
                BlockType _blockType = _blockData.blocksNeighbor[Vector3Int.down].blockType;
                if (_blockType == BlockType.Air)
                    _blockType = BlockType.Dirt;
                _blockDataNeighbor.blockType = _blockType;
            }
            if (!chunksToUpdate.Contains(_chunkFinal))
                chunksToUpdate.Add(_chunkFinal);
        }
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
    public bool GetChunkNeighbor(Vector2Int _direction, out ChunkFinal _neighbor)
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
        blockData = blocks[pos.x, pos.y, pos.z];
        if (!blockData) return;
        Gizmos.DrawCube(BlockManager.Instance.GetBlockPositionWorldFromBlock(blockData), Vector3.one);
        foreach (var item in blockData.blocksNeighbor)
            Gizmos.DrawCube(BlockManager.Instance.GetBlockPositionWorldFromBlock(item.Value), Vector3.one);
        Gizmos.color = Color.red;
        foreach (var item in neighborChunk)
            Gizmos.DrawWireMesh(item.Value.meshData.Mesh, item.Value.transform.position);
    }
}
