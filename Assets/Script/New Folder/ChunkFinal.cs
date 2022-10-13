using System;
using System.Collections;
using System.Collections.Generic;
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
    Dictionary<Vector3, int> verticesIndex = new Dictionary<Vector3, int>();
    BlockData[,,] blocks;

    public Vector3Int WorldPosition => new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
    
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
        yield return null;
    }
    public IEnumerator UpdateMesh()
    {
        yield return null;
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
}
