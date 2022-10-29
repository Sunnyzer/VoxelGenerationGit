using System.Collections.Generic;
using UnityEngine;

public class BlockManagerC : Singleton<BlockManagerC>
{
    [SerializeField] float textureOffset = 0.001f;
    [SerializeField] float tileSizeX;
    [SerializeField] float tileSizeY;
    [SerializeField] Dictionary<BlockType, TextureData> blockTextureDataDictionary = new Dictionary<BlockType, TextureData>();
    public BlockDataSO textureData;

    protected virtual void Awake()
    {
        base.Awake();
        foreach (var item in textureData.textureDataList)
        {
            if(!blockTextureDataDictionary.ContainsKey(item.blockType))
            {
                blockTextureDataDictionary.Add(item.blockType, item);
            }
        }
        tileSizeX = textureData.textureSizeX;
        tileSizeY = textureData.textureSizeY;
    }

    public Vector2Int TexturePosition(Vector3Int _direction, BlockType _blockType)
    {
        return blockTextureDataDictionary[_blockType][GetEDirectionFromVector3Int(_direction)];
    }
    public Vector2[] FaceUVs(Vector3Int _direction, BlockType _blockType)
    {
        Vector2[] UVs = new Vector2[4];
        Vector2Int _tilePos = TexturePosition(_direction, _blockType);

        UVs[3] = new Vector2(tileSizeX * _tilePos.x + textureOffset,
                             tileSizeY * _tilePos.y + textureOffset);
        //0,0
        UVs[2] = new Vector2(tileSizeX * _tilePos.x + tileSizeX - textureOffset,
                             tileSizeY * _tilePos.y + textureOffset);
        //0.1,0
        UVs[1] = new Vector2(tileSizeX * _tilePos.x + tileSizeX - textureOffset,
                             tileSizeY * _tilePos.y + tileSizeY - textureOffset);
        //0.1,0.1
        UVs[0] = new Vector2(tileSizeX * _tilePos.x + textureOffset,
                             tileSizeY * _tilePos.y + tileSizeY - textureOffset);
        //0,0.1
        //2 1 0, 1 3 0
        return UVs;
    }
    
    public EDirection GetEDirectionFromVector3Int(Vector3Int _direction)
    {
        if (_direction == Vector3Int.up)
            return EDirection.Up;
        else if (_direction == Vector3Int.down)
            return EDirection.Down;
        else
            return EDirection.Side;
    }
    public BlockDataC GetBlockFromWorldPosition(Vector3 _blockWorldPos, Vector3 _normal)
    {
        ChunkFinalC _chunk = ChunkManagerFinalC.Instance.GetChunkFromWorldPosition(_blockWorldPos);
        Vector3 _blockPos = _blockWorldPos - _chunk.transform.position - _normal * 0.5f;
        Vector3Int _blockChunkPos = new Vector3Int(Mathf.RoundToInt(_blockPos.x), Mathf.RoundToInt(_blockPos.y), Mathf.RoundToInt(_blockPos.z));
        BlockDataC _blockData = null;
        if (_chunk.IsBlockPosInChunk(_blockChunkPos))
            _blockData = _chunk.Blocks[_blockChunkPos.x, _blockChunkPos.y, _blockChunkPos.z];
        return _blockData;
    }
    public BlockDataC GetBlockFromBlockWorldPosition(Vector3Int _blockWorldPos)
    {
        ChunkFinalC _chunk = ChunkManagerFinalC.Instance.GetChunkFromWorldPosition(_blockWorldPos);
        if (!_chunk) return null;
        Vector3Int _blockChunkPos = _blockWorldPos - _chunk.WorldPosition;
        if(!_chunk.IsBlockPosInChunk(_blockChunkPos)) return null;
        BlockDataC _blockData = _chunk.Blocks[_blockChunkPos.x, _blockChunkPos.y, _blockChunkPos.z];
        return _blockData;
    }
    public Vector3Int GetBlockPositionWorldFromWorldPosition(Vector3 _worldPos)
    {
        return new Vector3Int(Mathf.RoundToInt(_worldPos.x), Mathf.RoundToInt(_worldPos.y), Mathf.RoundToInt(_worldPos.z));
    }
    public Vector3Int GetBlockPositionWorldFromBlock(BlockDataC _block)
    {
        if (!_block) return new Vector3Int();
        Vector3Int _blockWorldPos = _block.owner.WorldPosition + _block.positionBlock;
        return _blockWorldPos;
    }
    public Vector3Int GetBlockPositionWorldFromBlockPosInChunk(BlockDataC _block)
    {
        return new Vector3Int();
    }
    public BlockDataC[] GetBlocksFromBlockPositionToBlockPosition(Vector3Int _blockWorldPos, Vector3Int _blockWorldPos2)
    {
        return new BlockDataC[10];
    }
}
