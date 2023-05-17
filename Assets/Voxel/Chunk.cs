using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ChunkParam
{
    public int chunkSize;
    public int chunkHeight;
    public ChunkParam(int _chunkSize, int _chunkHeight, float _sizeBlock)
    {
        chunkSize = _chunkSize;
        chunkHeight = _chunkHeight;
    }
}

[RequireComponent(typeof(MeshCollider), typeof(MeshRenderer), typeof(MeshFilter))]
public class Chunk : MonoBehaviour
{
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] MeshData meshData;
    [SerializeField] Vector3Int pos;
    [SerializeField] Vector2Int indexChunk;

    Dictionary<Vector2Int, Chunk> neighborChunk = new Dictionary<Vector2Int, Chunk>();
    BlockType[] blocks;
    List<int> blockRenders = new List<int>();
    int count = 0;

    public Vector2Int IndexChunk => indexChunk;
    public BlockType[] Blocks => blocks;
    public int Count => count;
    public Vector3Int WorldPosition => new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));

    public IEnumerator Init(ChunkParam _chunkParamFinal, int _indexX, int _indexZ)
    {
        meshData = new MeshData();
        indexChunk = new Vector2Int(_indexX, _indexZ);
        count = _chunkParamFinal.chunkSize * _chunkParamFinal.chunkHeight * _chunkParamFinal.chunkSize;
        blocks = new BlockType[count];
        for (int i = 0; i < Direction.direction2D.Count; ++i)
            neighborChunk.Add(Direction.direction2D[i], null);
        yield return GenerateBlocks(_chunkParamFinal);
    }
    public IEnumerator GenerateBlocks(ChunkParam _chunkParamFinal)
    {
        int _chunkSize = _chunkParamFinal.chunkSize;
        int _chunkHeight = _chunkParamFinal.chunkHeight;
        int x = 0;
        int y = 0;
        int z = 0;
        for (x = 0; x < _chunkSize; x++)
        {
            for (z = 0; z < _chunkSize; z++)
            {
                float _perlinNoise = ChunkManager.Instance.PerlinNoiseOctaves(indexChunk.x * _chunkSize + x, indexChunk.y * _chunkSize + z);
                float _groundPos = Mathf.RoundToInt(_perlinNoise * _chunkHeight);
                for (y = 0; y < _chunkHeight; y++)
                {
                    int _index = y * _chunkHeight + x * _chunkSize + z;
                    BlockType _blockType = BlockType.Air;
                    if (y <= _groundPos)
                    {
                        _blockType = BlockType.Dirt;
                        if(y == _groundPos)
                        {
                            _blockType = BlockType.Grass_Dirt;
                        }
                        else if (y == 0)
                        {
                            _blockType = BlockType.Grass_Stone;
                        }
                    }
                    blocks[_index] = _blockType;
                }
            }
        }
        yield return null;
    }
    public IEnumerator AddAllFace()
    {
        ChunkParam _chunkParam = ChunkManager.Instance.ChunkParam;
        int _chunkSize = _chunkParam.chunkSize;
        int _chunkHeight = _chunkParam.chunkHeight;
        for (int i = 0; i < count; ++i)
        {
            BlockType _blockType = blocks[i];
            if (_blockType == BlockType.Air) continue;
            Vector3 _blockPos = GetPositionWithIndex(i);
            if (IsBlockIndexInChunk(i + 1) && _blockPos.z != _chunkSize - 1 && blocks[i + 1] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.forward, _blockType);
            else if(_blockPos.z == _chunkSize - 1)
            {
                Chunk _neighbor = neighborChunk[Vector2Int.up];
                Vector3Int _neighborBlock = GetPositionNeighborBlock(_blockPos, Vector3Int.forward);
                int _indexNeighbor = GetIndexWithPosition(_neighborBlock);
                if ((_neighbor && _neighbor.IsBlockIndexInChunk(_indexNeighbor) && _neighbor.blocks[_indexNeighbor] == BlockType.Air) || !_neighbor)
                {
                    meshData.AddFaceV2(_blockPos, Vector3Int.forward, _blockType);
                }
            }

            if (IsBlockIndexInChunk(i - 1) && _blockPos.z != 0 && blocks[i - 1] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.back, _blockType);
            else if (_blockPos.z == 0)
            {
                Chunk _neighbor = neighborChunk[Vector2Int.down];
                Vector3Int _neighborBlock = GetPositionNeighborBlock(_blockPos, Vector3Int.back);
                int _indexNeighbor = GetIndexWithPosition(_neighborBlock);
                if ((_neighbor && _neighbor.IsBlockIndexInChunk(_indexNeighbor) && _neighbor.blocks[_indexNeighbor] == BlockType.Air) || !_neighbor)
                {
                    meshData.AddFaceV2(_blockPos, Vector3Int.back, _blockType);
                }
            }

            if (IsBlockIndexInChunk(i + _chunkSize) && _blockPos.x != _chunkSize - 1 && blocks[i + _chunkSize] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.right, _blockType);
            else if(_blockPos.x == _chunkSize - 1)
            {
                Chunk _neighbor = neighborChunk[Vector2Int.right];
                Vector3Int _neighborBlock = GetPositionNeighborBlock(_blockPos, Vector3Int.right);
                int _indexNeighbor = GetIndexWithPosition(_neighborBlock);
                if ((_neighbor && _neighbor.IsBlockIndexInChunk(_indexNeighbor) && _neighbor.blocks[_indexNeighbor] == BlockType.Air) || !_neighbor)
                    meshData.AddFaceV2(_blockPos, Vector3Int.right, _blockType);
            }

            if (IsBlockIndexInChunk(i - _chunkSize) && _blockPos.x != 0 && blocks[i - _chunkSize] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.left, _blockType);
            else if(_blockPos.x == 0)
            {
                Chunk _neighbor = neighborChunk[Vector2Int.left];
                Vector3Int _neighborBlock = GetPositionNeighborBlock(_blockPos, Vector3Int.left);
                int _indexNeighbor = GetIndexWithPosition(_neighborBlock);
                if ((_neighbor && _neighbor.IsBlockIndexInChunk(_indexNeighbor) && _neighbor.blocks[_indexNeighbor] == BlockType.Air) || !_neighbor)
                    meshData.AddFaceV2(_blockPos, Vector3Int.left, _blockType);
            }

            if (IsBlockIndexInChunk(i + _chunkHeight) && blocks[i + _chunkHeight] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.up, _blockType);
            if (IsBlockIndexInChunk(i - _chunkHeight) && blocks[i - _chunkHeight] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.down, _blockType);
        }
        meshData.UpdateMesh(meshCollider, meshFilter);
        yield return null;
    }
    /*public void SetFace(int _index, Vector3 _blockPos, Vector2Int _direction, BlockType _blockType)
    {
        ChunkParam _chunkParam = ChunkManager.Instance.ChunkParam;
        int _chunkSize = _chunkParam.chunkSize;
        int _chunkHeight = _chunkParam.chunkHeight;
        Vector3Int _direction3 = new Vector3Int(_direction.x, 0, _direction.y);
        //bool _border = _direction.x == -1 ?  : _direction.x == -1 ? _blockPos.x != 0 : _blockPos.x != _chunkSize - 1;
        if (IsBlockIndexInChunk(_index) && _border && blocks[_index] == BlockType.Air)
            meshData.AddFaceV2(_blockPos, _direction3, _blockType);
        else if (_blockPos.x == 0)
        {
            Chunk _neighbor = neighborChunk[_direction];
            Vector3Int _neighborBlock = GetPositionNeighborBlock(_blockPos, _direction3);
            int _indexNeighbor = GetIndexWithPosition(_neighborBlock);
            if ((_neighbor && _neighbor.IsBlockIndexInChunk(_indexNeighbor) && _neighbor.blocks[_indexNeighbor] == BlockType.Air) || !_neighbor)
            {
                meshData.AddFaceV2(_blockPos, _direction3, _blockType);
            }
        }
    }*/
    public Vector3Int GetPositionNeighborBlock(Vector3 _blockPos, Vector3Int _direction)
    {
        ChunkParam _chunkParam = ChunkManager.Instance.ChunkParam;
        int _chunkSize = _chunkParam.chunkSize;
        Vector3Int _blockPosInt = new Vector3Int((int)_blockPos.x, (int)_blockPos.y, (int)_blockPos.z);
        Vector3Int _neighborBlock = _blockPosInt + _direction;
        if (IsBlockPosInChunk(_neighborBlock)) return _neighborBlock;
        int _x = (_direction.x == 0 ? _blockPosInt.x : (_direction.x == 1) ? 0 : (_chunkSize - 1 - _blockPosInt.x)); 
        int _y = _blockPosInt.y;
        int _z = (_direction.z == 0 ? _blockPosInt.z : (_direction.z == 1) ? 0 : (_chunkSize - 1 - _blockPosInt.z)); 
        return new Vector3Int(_x, _y, _z);
    }
    public bool IsBlockPosInChunk(Vector3Int _blockPos)
    {
        ChunkParam _chunkParam = ChunkManager.Instance.ChunkParam;
        return (_blockPos.x >= 0 && _blockPos.x < _chunkParam.chunkSize) &&
                (_blockPos.y >= 0 && _blockPos.y < _chunkParam.chunkHeight) &&
                (_blockPos.z >= 0 && _blockPos.z < _chunkParam.chunkSize);
    }
    public bool IsBlockIndexInChunk(int _indexBlock)
    {
        return _indexBlock >= 0 && _indexBlock < count;
    }

    public bool GetChunkNeighbor(Vector2Int _direction, out Chunk _neighbor)
    {
        if (neighborChunk.ContainsKey(_direction))
            _neighbor = neighborChunk[_direction];
        else
            _neighbor = null;
        return _neighbor != null;
    }

    public Vector3Int GetPositionWithIndex(int _index)
    {
        ChunkParam _chunkParam = ChunkManager.Instance.ChunkParam;
        int y = Mathf.FloorToInt((float)_index / _chunkParam.chunkHeight);
        _index = _index - y * _chunkParam.chunkHeight;
        int x = _index / _chunkParam.chunkSize;
        _index = _index - x * _chunkParam.chunkSize;
        int z = _index;
        return new Vector3Int(x, y, z);
    }
    public int GetIndexWithPosition(Vector3Int _blockPos)
    {
        ChunkParam _chunkParam = ChunkManager.Instance.ChunkParam;
        return _blockPos.x * _chunkParam.chunkSize + _blockPos.y * _chunkParam.chunkHeight + _blockPos.z;
    }
    //Add Neighbor chunk with a initial direction
    public void AddNeighbor(Vector2Int _direction, Chunk _chunk)
    {
        if(neighborChunk.ContainsKey(_direction))
            neighborChunk[_direction] = _chunk;
    }
}
