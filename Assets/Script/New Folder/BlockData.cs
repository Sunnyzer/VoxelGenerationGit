using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlockData
{
    public Vector3Int positionBlock;
    public ChunkFinal owner;
    public BlockType blockType = BlockType.Dirt;
    public Dictionary<Vector3Int, Block> blocksNeighbor = new Dictionary<Vector3Int, Block>();
    public Dictionary<Vector3Int, Face> facePerDirection = new Dictionary<Vector3Int, Face>();

    public BlockData(Vector3Int _positionBlock, ChunkFinal _owner, BlockType _blockType)
    {
        positionBlock = _positionBlock;
        owner = _owner;
        blockType = _blockType;
    }

    public void AddNewFace(Vector3Int _direction, Face _face)
    {

    }
    public void SetNeighbor(ChunkFinal _chunk)
    {

    }
}
