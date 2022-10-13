using UnityEngine;

public class BlockManager : Singleton<BlockManager>
{
    public BlockData GetBlockFromWorldPosition(Vector3 _blockWorldPos)
    {
        return null;
    }
    public Vector3Int GetBlockPositionWorldFromBlock(Block _block)
    {
        return new Vector3Int();
    }
    public Vector3Int GetBlockPositionWorldFromBlockPosInChunk(Block _block)
    {
        return new Vector3Int();
    }
    public Block[] GetBlocksFromBlockPositionToBlockPosition(Vector3Int _blockWorldPos, Vector3Int _blockWorldPos2)
    {
        return new Block[10];
    }
}
