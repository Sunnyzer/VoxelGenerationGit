using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[Serializable]
public struct ChunkParam
{
    public int chunkSize;
    public int chunkHeight;
    public ChunkParam(int _chunkSize, int _chunkHeight)
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
    MeshData meshData = new MeshData();
    Vector2Int indexChunk;

    Dictionary<Vector2Int, Chunk> neighborChunk = new Dictionary<Vector2Int, Chunk>()
    {
        { Vector2Int.right,null },
        { Vector2Int.left,null },
        { Vector2Int.down,null },
        { Vector2Int.up,null },
    };
    BlockType[] blocks;
    List<int> blocksRender = new List<int>();
    int count = 0;

    public Vector2Int IndexChunk => indexChunk;
    public BlockType[] Blocks => blocks;
    public int Count => count;
    public Vector3Int WorldPosition => new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));

    public IEnumerator Init(ChunkParam _chunkParamFinal, int _indexX, int _indexZ)
    {
        indexChunk = new Vector2Int(_indexX, _indexZ);
        count = _chunkParamFinal.chunkSize * _chunkParamFinal.chunkHeight * _chunkParamFinal.chunkSize;
        blocks = new BlockType[count];
        yield return GenerateBlocks(_chunkParamFinal);
    }
    public IEnumerator GenerateBlocks(ChunkParam _chunkParamFinal)
    {
        int _chunkSize = _chunkParamFinal.chunkSize;
        int _chunkHeight = _chunkParamFinal.chunkHeight;
        for (int x = 0; x < _chunkSize; x++)
        {
            for (int z = 0; z < _chunkSize; z++)
            {
                float _perlinNoise = ChunkManager.Instance.PerlinNoiseOctaves(indexChunk.x * _chunkSize + x, indexChunk.y * _chunkSize + z);
                float _groundPos = Mathf.RoundToInt(_perlinNoise * _chunkHeight);
                for (int y = 0; y < _chunkHeight; y++)
                {
                    int _index = y * _chunkHeight + x * _chunkSize + z;
                    BlockType _blockType = BlockType.Air;
                    if (y <= _groundPos)
                    {
                        _blockType = BlockType.Dirt;
                        if(y == _groundPos)
                            _blockType = BlockType.Grass_Dirt;
                        else if (y == 0)
                            _blockType = BlockType.Grass_Stone;
                    }
                    blocks[_index] = _blockType;
                }
            }
        }
        yield return null;
    }
    public IEnumerator Render()
    {
        for (int i = 0; i < count; ++i)
        {
            BlockType _blockType = blocks[i];
            if (_blockType == BlockType.Air) continue;

            Vector3 _blockPos = GetPositionWithIndex(i);

            for (int j = 0; j < Direction.allDirection.Count; j++)
            {
                SetFace(_blockPos, Direction.allDirection[j], _blockType);
            }
        }
        meshData.UpdateMesh(meshCollider, meshFilter);
        yield return null;
    }
    public bool IsBlockBorderDirection(Vector3 _blockPos, Vector3Int _direction)
    {
        ChunkParam _chunkParam = ChunkManager.Instance.ChunkParam;
        int _chunkSize = _chunkParam.chunkSize;
        if (Vector3Int.forward == _direction)
            return _blockPos.z == _chunkSize - 1;
        if (Vector3Int.back == _direction)
            return _blockPos.z == 0;
        if (Vector3Int.right == _direction)
            return _blockPos.x == _chunkSize - 1;
        if (Vector3Int.left == _direction)
            return _blockPos.x == 0;
        return false;
    }
    public bool IsBlockBorder(Vector3 _blockPos)
    {
        ChunkParam _chunkParam = ChunkManager.Instance.ChunkParam;
        int _chunkSize = _chunkParam.chunkSize;
        if (_blockPos.z == _chunkSize - 1)
            return true;
        if (_blockPos.z == 0)
            return true;
        if (_blockPos.x == _chunkSize - 1)
            return true;
        if (_blockPos.x == 0)
            return true;
        return false;
    }
    public void SetFace(Vector3 _blockPos, Vector3Int _direction, BlockType _blockType)
    {
        Vector3Int _blockPosInt = new Vector3Int((int)_blockPos.x, (int)_blockPos.y, (int)_blockPos.z);
        bool _border = IsBlockBorderDirection(_blockPos, _direction);
        if (IsBlockPosInChunk(_blockPosInt + _direction) && blocks[GetIndexWithPosition(_blockPosInt + _direction)] == BlockType.Air && !_border)
        {
            int _index = GetIndexWithPosition(_blockPosInt);
            if(!blocksRender.Contains(_index))
                blocksRender.Add(_index);
            meshData.AddFaceV2(_blockPos, _direction, _blockType);
        }
        else if (_border)
        {
            Vector2Int _direction2 = new Vector2Int(_direction.x, _direction.z);
            Chunk _neighbor = neighborChunk[_direction2];
            Vector3Int _neighborBlock = GetPositionNeighborBlock(_blockPos, _direction);
            int _indexNeighbor = GetIndexWithPosition(_neighborBlock);
            if ((_neighbor && _neighbor.IsBlockIndexInChunk(_indexNeighbor) && _neighbor.blocks[_indexNeighbor] == BlockType.Air) || !_neighbor)
            {
                int _index = GetIndexWithPosition(_blockPosInt);
                if (!blocksRender.Contains(_index))
                    blocksRender.Add(_index);
                meshData.AddFaceV2(_blockPos, _direction, _blockType);
            }
        }
    }
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
    public void DestroyBlock(Vector3 _point)
    {
        Vector3Int _blockPos = BlockManager.GetBlockPositionWithWorldPosition(_point) - WorldPosition;
        Debug.DrawRay(_blockPos, Vector3.up, Color.red, 2);
        int _index = GetIndexWithPosition(_blockPos);
        blocks[_index] = BlockType.Air;
        if (IsBlockBorderDirection(_blockPos, Vector3Int.forward))
        {
            GetChunkNeighbor(Vector2Int.up, out Chunk _neighborChunk);
            _neighborChunk?.Rerender();
        }
        if (IsBlockBorderDirection(_blockPos, Vector3Int.back))
        {
            GetChunkNeighbor(Vector2Int.down, out Chunk _neighborChunk);
            _neighborChunk?.Rerender();
        }
        if (IsBlockBorderDirection(_blockPos, Vector3Int.right))
        {
            GetChunkNeighbor(Vector2Int.right, out Chunk _neighborChunk);
            _neighborChunk?.Rerender();
        }
        if (IsBlockBorderDirection(_blockPos, Vector3Int.left))
        {
            GetChunkNeighbor(Vector2Int.left, out Chunk _neighborChunk);
            _neighborChunk?.Rerender();
        }
        Rerender();
    }
    public void Rerender()
    {
        meshData.ResetVerticesAndTriangles();
        StopCoroutine(Render());
        StartCoroutine(Render());
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
