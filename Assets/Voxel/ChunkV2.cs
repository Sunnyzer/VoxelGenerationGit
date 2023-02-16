using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(MeshRenderer), typeof(MeshFilter))]
public class ChunkV2 : MonoBehaviour
{
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] MeshData meshData;
    [SerializeField] Vector3Int pos;
    [SerializeField] Vector2Int indexChunk;
    [SerializeField] bool test = true;

    Dictionary<Vector2Int, ChunkV2> neighborChunk = new Dictionary<Vector2Int, ChunkV2>();
    BlockType[] blocks;
    List<int> blockRenders = new List<int>();
    int count = 0;

    public Vector2Int IndexChunk => indexChunk;
    public BlockType[] Blocks => blocks;
    public int Count => count;
    public Vector3Int WorldPosition => new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));

    public IEnumerator Init(ChunkParamFinal _chunkParamFinal, int _indexX, int _indexZ)
    {
        meshData = new MeshData();
        indexChunk = new Vector2Int(_indexX, _indexZ);
        count = _chunkParamFinal.chunkSize * _chunkParamFinal.chunkHeight * _chunkParamFinal.chunkSize;
        blocks = new BlockType[count];
        for (int i = 0; i < Direction.direction2D.Count; ++i)
            neighborChunk.Add(Direction.direction2D[i], null);
        yield return GenerateBlocks(_chunkParamFinal);
    }
    public IEnumerator GenerateBlocks(ChunkParamFinal _chunkParamFinal)
    {
        int _chunkSize = _chunkParamFinal.chunkSize;
        for (int x = 0; x < _chunkSize; x++)
        {
            for (int z = 0; z < _chunkSize; z++)
            {
                float _perlinNoise = ChunkManagerV2.Instance.PerlinNoiseOctaves(indexChunk.x * _chunkSize + x, indexChunk.y * _chunkSize + z);
                float _groundPos = Mathf.RoundToInt(_perlinNoise * _chunkParamFinal.chunkHeight);
                for (int y = 0; y < _chunkParamFinal.chunkHeight; y++)
                {
                    BlockType _blockType = BlockType.Air;
                    if (y == _groundPos)
                    {
                        _blockType = BlockType.Grass_Dirt;
                        blockRenders.Add(y * _chunkParamFinal.chunkHeight + x * _chunkParamFinal.chunkSize + z);
                    }
                    else if (y < _groundPos)
                        _blockType = BlockType.Dirt;

                    blocks[y * _chunkParamFinal.chunkHeight + x * _chunkParamFinal.chunkSize + z] = _blockType;
                }
            }
        }
        yield return null;
    }
    public IEnumerator RenderMesh()
    {
        ChunkParamFinal _chunkParam = ChunkManagerV2.Instance.ChunkParam;
        int _chunkSize = _chunkParam.chunkSize;
        int _chunkHeight = _chunkParam.chunkHeight;
        for (int i = 0; i < count; ++i)
        {
            if (blocks[i] == BlockType.Air) continue;
            Vector3 _blockPos = GetPositionWithIndex(i);
            if (IsBlockIndexInChunk(i + 1) && blocks[i + 1] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.forward, blocks[i]);
            if (IsBlockIndexInChunk(i - 1) && blocks[i - 1] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.back, blocks[i]);
            if (IsBlockIndexInChunk(i + _chunkSize) && blocks[i + _chunkSize] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.right, blocks[i]);
            if (IsBlockIndexInChunk(i - _chunkSize) && blocks[i - _chunkSize] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.left, blocks[i]);
            if (IsBlockIndexInChunk(i + _chunkHeight) && blocks[i + _chunkHeight] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.up, blocks[i]);
            if (IsBlockIndexInChunk(i - _chunkHeight) && blocks[i - _chunkHeight] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.down, blocks[i]);
        }
        meshData.UpdateMesh(meshCollider, meshFilter);
        yield return null;
    }
    public void UpdateMesh()
    {
        
    }
    public bool IsBlockPosInChunk(Vector3Int _blockPos)
    {
        ChunkParamFinal _chunkParam = ChunkManagerV2.Instance.ChunkParam;
        return (_blockPos.x >= 0 && _blockPos.x < _chunkParam.chunkSize) &&
                (_blockPos.y >= 0 && _blockPos.y < _chunkParam.chunkHeight) &&
                (_blockPos.z >= 0 && _blockPos.z < _chunkParam.chunkSize);
    }
    public bool IsBlockIndexInChunk(int _indexBlock)
    {
        return _indexBlock >= 0 && _indexBlock < count;
    }
    public bool GetChunkNeighbor(Vector2Int _direction, out ChunkV2 _neighbor)
    {
        if (neighborChunk.ContainsKey(_direction))
            _neighbor = neighborChunk[_direction];
        else
            _neighbor = null;
        return _neighbor != null;
    }
    public Vector3 GetPositionWithIndex(int _index)
    {
        int y = Mathf.FloorToInt((float)_index / ChunkManagerV2.Instance.ChunkParam.chunkHeight);
        _index = _index - y * ChunkManagerV2.Instance.ChunkParam.chunkHeight;
        int x = _index / ChunkManagerV2.Instance.ChunkParam.chunkSize;
        _index = _index - x * ChunkManagerV2.Instance.ChunkParam.chunkSize;
        int z = _index;
        return new Vector3(x, y, z);
    }
    private void OnDrawGizmosSelected()
    { 
        foreach (var item in neighborChunk)
        {
            if (!item.Value) continue;
            Gizmos.DrawWireMesh(item.Value.meshData.Mesh, 0, item.Value.transform.position);    
        }
    }
    private void OnDrawGizmos()
    {
        //int _count = blockRenders.Count;
        //for (int i = 0; i < _count; ++i)
        //{
        //    switch (blocks[blockRenders[i]])
        //    {
        //        case BlockType.Nothing:
        //            break;
        //        case BlockType.Air:
        //            break;
        //        case BlockType.Grass_Dirt:
        //            Gizmos.color = Color.green;
        //            break;
        //        case BlockType.Dirt:
        //            Gizmos.color = Color.yellow;
        //            break;
        //        case BlockType.Grass_Stone:
        //            break;
        //        case BlockType.Stone:
        //            break;
        //        case BlockType.TreeTrunk:
        //            break;
        //        case BlockType.TreeLeafesTransparent:
        //            break;
        //        case BlockType.TreeLeafsSolid:
        //            break;
        //        case BlockType.Water:
        //            Gizmos.color = Color.blue;
        //            break;
        //        case BlockType.Sand:
        //            break;
        //        default:
        //            break;
        //    }
        //    Gizmos.DrawCube(transform.position + GetPositionWithIndex(blockRenders[i]), Vector3.one);
        //}
    }

    public void AddNeighbor(Vector2Int _direction, ChunkV2 _chunk)
    {
        if(neighborChunk.ContainsKey(_direction))
            neighborChunk[_direction] = _chunk;
    }
}
