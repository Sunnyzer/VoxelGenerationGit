using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public struct ChunkParamFinal
{
    public int chunkSize;
    public int chunkHeight;
    public int sizeBlock;
    public ChunkParamFinal(int _chunkSize, int _chunkHeight, int _sizeBlock)
    {
        chunkSize = _chunkSize;
        chunkHeight = _chunkHeight;
        sizeBlock = _sizeBlock;
    }
}

[RequireComponent(typeof(MeshCollider), typeof(MeshRenderer), typeof(MeshFilter))]
public class ChunkFinal : MonoBehaviour
{
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Vector2Int indexChunk;
    [SerializeField] MeshData meshData;
    ChunkParamFinal chunkParam;

    List<BlockData> chunkRender = new List<BlockData>();
    BlockData[,,] blocks;
    Dictionary<Vector2Int,ChunkFinal> neighborChunk = new Dictionary<Vector2Int, ChunkFinal>();

    public BlockData[,,] Blocks => blocks;
    public Vector3Int WorldPosition => new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));

    public int ChunkSize => chunkParam.chunkSize;
    public int ChunkHeight => chunkParam.chunkHeight;

    public IEnumerator GenerateBlocks()
    {
        blocks = new BlockData[chunkParam.chunkSize, chunkParam.chunkHeight, chunkParam.chunkSize];
        for (int x = 0; x < chunkParam.chunkSize; x++)
        {
            for (int z = 0; z < chunkParam.chunkSize; z++)
            {
                float _perlinNoise = ChunkManagerFinal.Instance.PerlinNoiseOctaves(x, z);
                float _groundPos = Mathf.RoundToInt(_perlinNoise * chunkParam.chunkHeight);
                BlockType _blockType = BlockType.Air;
                for (int y = 0; y < chunkParam.chunkHeight; y++)
                {
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
        foreach (var item in Direction.direction2D)
        {
            ChunkFinal _chunkNeighbor = ChunkManagerFinal.Instance.GetChunkFromIndexChunk(indexChunk.x + item.x, indexChunk.y + item.y);
            if(_chunkNeighbor)
                neighborChunk.Add(item, _chunkNeighbor);
        }
        RunThroughAllBlocks((_blockPos) => { blocks[_blockPos.x, _blockPos.y, _blockPos.z].SetNeighbor(this);  });
        yield return null;
    }
    public IEnumerator UpdateMesh()
    {
        meshData.UpdateMesh(meshCollider, meshFilter);
        yield return null;
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
        
}
