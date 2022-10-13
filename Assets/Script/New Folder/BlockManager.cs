using UnityEngine;

public class BlockManager : Singleton<BlockManager>
{
    public BlockData GetBlockFromWorldPosition(Vector3 _blockWorldPos)
    {
        return null;
    }
    public Vector3Int GetBlockPositionWorldFromBlock(BlockData _block)
    {
        Vector3Int _blockWorldPos = _block.owner.WorldPosition + _block.positionBlock;
        return _blockWorldPos;
    }
    public Vector3Int GetBlockPositionWorldFromBlockPosInChunk(BlockData _block)
    {
        return new Vector3Int();
    }
    public BlockData[] GetBlocksFromBlockPositionToBlockPosition(Vector3Int _blockWorldPos, Vector3Int _blockWorldPos2)
    {
        return new BlockData[10];
    }
}
