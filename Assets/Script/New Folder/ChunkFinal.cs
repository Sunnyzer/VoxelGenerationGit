using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        blocks = new BlockData[chunkParam.chunkSize, chunkParam.chunkHeight, chunkParam.chunkSize];
        for (int x = 0; x < chunkParam.chunkSize; x++)
        {
            for (int z = 0; z < chunkParam.chunkSize; z++)
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
                    
                    if (_blockType == BlockType.Grass_Dirt)
                        blockRender.Add(blocks[x, y, z]);
                }
            }
            yield return null;
        }
    }
    public IEnumerator InitChunks()
    {
        foreach (var item in Direction.direction2D)
        {
            ChunkFinal _chunkNeighbor = ChunkManagerFinal.Instance.GetChunkFromIndexChunk(chunkParam.indexChunk.x + item.x, chunkParam.indexChunk.y + item.y);
            if(_chunkNeighbor)
                neighborChunk.Add(item, _chunkNeighbor);
        }
        RunThroughAllBlocks((_blockPos) => { blocks[_blockPos.x, _blockPos.y, _blockPos.z].SetNeighbor(this);  });
        yield return null;
    }
    public IEnumerator UpdateMesh()
    {
        int _count = blockRender.Count;
        for (int i = 0; i < _count; i++)
            BlockRenderFace(blockRender[i]);
        meshData.UpdateMesh(meshCollider, meshFilter);
        yield return null;
    }
    void BlockRenderFace(BlockData _block)
    {
        Dictionary<Vector3Int, BlockData> _blockNeighbor = _block.blocksNeighbor;
        foreach (var item in _blockNeighbor)
        {
            if (item.Value.blockType != BlockType.Air) continue;
            Vector3 _direction = item.Key;
            Vector3Int _faceCenter = _block.positionBlock + item.Key;
            Face _faceAddToMesh = meshData.AddFace(_faceCenter, item.Key);
            _block.AddNewFace(item.Key, _faceAddToMesh);
        }
    }

    void DestroyWorldPositionRadius(Vector3 _blockPos, int radius)
    {

    }
    void DestroyWorldPositionBlock(Vector3 _blockPos)
    {

    }
    void RecalculateMesh()
    {

    }
    void UpdateVerticesAndTriangles()
    {

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
        meshData.DebugMesh();
        if (!blockData) return;
        Gizmos.DrawCube(BlockManager.Instance.GetBlockPositionWorldFromBlock(blockData), Vector3.one);
        Gizmos.color = Color.red;
        foreach (var item in blockData.blocksNeighbor)
        {
            Gizmos.DrawWireCube(BlockManager.Instance.GetBlockPositionWorldFromBlock(item.Value), Vector3.one);
        }
        foreach (var item in neighborChunk)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireMesh(item.Value.meshData.Mesh, item.Value.transform.position);
        }
    }
}
