using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BlockDataC
{
    public Vector3Int positionBlock;
    public ChunkFinalC owner;
    public BlockType blockType = BlockType.Dirt;
    public Dictionary<Vector3Int, Face> facePerDirection = new Dictionary<Vector3Int, Face>();
    public BlockDataC(Vector3Int _positionBlock, ChunkFinalC _owner, BlockType _blockType)
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
        else
            facePerDirection[_direction] = _face;
        return _addSucceed;
    }
    public void DestroyBlock()
    {
        blockType = BlockType.Air;
        facePerDirection.Clear();
    }
    public static bool operator !(BlockDataC _blockData) => _blockData == null;
    public static implicit operator bool(BlockDataC _blockData) => _blockData != null;
}
