using UnityEngine;

public class BlockManager : Singleton<BlockManager>
{
    public BlockData GetBlockFromWorldPosition(Vector3 _blockWorldPos, Vector3 _normal)
    {
        ChunkFinal _chunk = ChunkManagerFinal.Instance.GetChunkFromWorldPosition(_blockWorldPos);
        Vector3 _blockPos = _blockWorldPos - _chunk.transform.position - _normal * 0.5f;
        Vector3Int _blockChunkPos = new Vector3Int(Mathf.RoundToInt(_blockPos.x), Mathf.RoundToInt(_blockPos.y), Mathf.RoundToInt(_blockPos.z));
        Debug.Log(_blockChunkPos);
        BlockData _blockData = _chunk.Blocks[_blockChunkPos.x, _blockChunkPos.y, _blockChunkPos.z];
        return _blockData;
    }
    public Vector3Int GetBlockPositionWorldFromWorldPosition(Vector3 _worldPos)
    {
        return new Vector3Int();
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
