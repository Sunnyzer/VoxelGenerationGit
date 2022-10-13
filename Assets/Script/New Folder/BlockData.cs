using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BlockData
{
    public Vector3Int positionBlock;
    public ChunkFinal owner;
    public BlockType blockType = BlockType.Dirt;
    public Dictionary<Vector3Int, BlockData> blocksNeighbor = new Dictionary<Vector3Int, BlockData>();
    public Dictionary<Vector3Int, Face> facePerDirection = new Dictionary<Vector3Int, Face>();

    public BlockData(Vector3Int _positionBlock, ChunkFinal _owner, BlockType _blockType)
    {
        positionBlock = _positionBlock;
        owner = _owner;
        blockType = _blockType;
    }

    public bool AddNewFace(Vector3Int _direction, Face _face)
    {
        bool _addSucceed = !facePerDirection.Keys.Contains(_direction);
        if (_addSucceed)
            facePerDirection.Add(_direction, _face);
        return _addSucceed;
    }
    public void SetNeighbor(ChunkFinal _chunk)
    {
        foreach (Vector3Int _direction in Direction.allDirection)
        {
            Vector3Int _posNeighbor = positionBlock + _direction;
            if (_chunk.IsBlockPosInChunk(_posNeighbor))
                blocksNeighbor.Add(_direction, _chunk.Blocks[_posNeighbor.x, _posNeighbor.y, _posNeighbor.z]);
            else
            {
                if (_chunk.GetChunkNeighbor(new Vector2Int(_direction.x, _direction.z), out ChunkFinal _neighbor))
                {
                    Vector3Int _pos = positionBlock - new Vector3Int((_chunk.ChunkSize - 1) * _direction.x, 0, (_chunk.ChunkSize - 1) * _direction.z);
                    BlockData _blockNeighbor = _neighbor.Blocks[_pos.x, _pos.y, _pos.z];
                    blocksNeighbor.Add(_direction, _blockNeighbor);
                }
            }
        }
    }

    public static bool operator !(BlockData _blockData) => _blockData == null;
    public static implicit operator bool(BlockData _blockData) => _blockData != null;
}
