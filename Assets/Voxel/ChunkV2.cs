using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshCollider), typeof(MeshRenderer), typeof(MeshFilter))]
public class ChunkV2 : MonoBehaviour
{
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] MeshData meshData;
    [SerializeField] Vector3Int pos;
    [SerializeField] Vector2Int indexChunk;
    [SerializeField] Texture2D text;

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
        text = new Texture2D(16, 256/2);
        Color[] pix = new Color[16 * (256/2)];
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
                    
                    if (y <= _groundPos)
                    {
                        float xCoord = indexChunk.x + (float)x /_chunkParamFinal.chunkSize;
                        float yCoord = (float)y/ _groundPos;
                        float zCoord = indexChunk.y + (float)z/_chunkParamFinal.chunkSize;
                        float _perlinNoiseG = Mathf.PerlinNoise(xCoord, yCoord) + Mathf.PerlinNoise(yCoord, zCoord);
                        pix[x * _chunkSize + y] = new Color(_perlinNoiseG, _perlinNoiseG, _perlinNoiseG)/2;
                        if(y != 0 && _perlinNoiseG < 0.5f && _perlinNoiseG > 0f)
                        {
                            _blockType = BlockType.Air;
                        }
                        else if(y == _groundPos)
                        {
                            _blockType = BlockType.Grass_Dirt;
                        }
                        else if (y == 0)
                            _blockType = BlockType.Grass_Stone;
                        else
                        {
                            _blockType = BlockType.Dirt;
                        }
                    }
                    blocks[y * _chunkParamFinal.chunkHeight + x * _chunkParamFinal.chunkSize + z] = _blockType;
                }
            }
        }
        text.SetPixels(pix);
        text.Apply();
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
            if (IsBlockIndexInChunk(i + 1) && _blockPos.z != _chunkSize - 1 && blocks[i + 1] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.forward, blocks[i]);
            else if(_blockPos.z == _chunkSize - 1)
            {
                ChunkV2 _neighbor = neighborChunk[Vector2Int.up];
                Vector3Int _neighborBlock = new Vector3Int((int)_blockPos.x, (int)_blockPos.y, 0);
                int _indexNeighbor = GetIndexWithPosition(_neighborBlock);
                if (_neighbor && _neighbor.IsBlockIndexInChunk(_indexNeighbor) && _neighbor.blocks[_indexNeighbor] == BlockType.Air)
                {
                    meshData.AddFaceV2(_blockPos, Vector3Int.forward, blocks[i]);
                }
                else if (!_neighbor)
                {
                    meshData.AddFaceV2(_blockPos, Vector3Int.forward, blocks[i]);
                }
            }

            if (IsBlockIndexInChunk(i - 1) && _blockPos.z != 0 && blocks[i - 1] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.back, blocks[i]);
            else if (_blockPos.z == 0)
            {
                ChunkV2 _neighbor = neighborChunk[Vector2Int.down];
                Vector3Int _neighborBlock = new Vector3Int((int)_blockPos.x, (int)_blockPos.y, (int)(_chunkSize - 1 - _blockPos.z));
                int _indexNeighbor = GetIndexWithPosition(_neighborBlock);
                if (_neighbor && _neighbor.IsBlockIndexInChunk(_indexNeighbor) && _neighbor.blocks[_indexNeighbor] == BlockType.Air)
                {
                    meshData.AddFaceV2(_blockPos, Vector3Int.back, blocks[i]);
                }
                else if(!_neighbor)
                {
                    meshData.AddFaceV2(_blockPos, Vector3Int.back, blocks[i]);
                }
            }

            if (IsBlockIndexInChunk(i + _chunkSize) && _blockPos.x != _chunkSize - 1 && blocks[i + _chunkSize] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.right, blocks[i]);
            else if(_blockPos.x == _chunkSize - 1)
            {
                ChunkV2 _neighbor = neighborChunk[Vector2Int.right];
                Vector3Int _neighborBlock = new Vector3Int(0, (int)_blockPos.y, (int)_blockPos.z);
                int _indexNeighbor = GetIndexWithPosition(_neighborBlock);
                if (_neighbor && _neighbor.IsBlockIndexInChunk(_indexNeighbor) && _neighbor.blocks[_indexNeighbor] == BlockType.Air)
                {
                    meshData.AddFaceV2(_blockPos, Vector3Int.right, blocks[i]);
                }
                else if (!_neighbor)
                {
                    meshData.AddFaceV2(_blockPos, Vector3Int.right, blocks[i]);
                }
            }

            if (IsBlockIndexInChunk(i - _chunkSize) && _blockPos.x != 0 && blocks[i - _chunkSize] == BlockType.Air)
                meshData.AddFaceV2(_blockPos, Vector3Int.left, blocks[i]);
            else if(_blockPos.x == 0)
            {
                ChunkV2 _neighbor = neighborChunk[Vector2Int.left];
                Vector3Int _neighborBlock = new Vector3Int((int)(_chunkSize - 1 - _blockPos.x), (int)_blockPos.y, (int)_blockPos.z);
                int _indexNeighbor = GetIndexWithPosition(_neighborBlock);
                if (_neighbor && _neighbor.IsBlockIndexInChunk(_indexNeighbor) && _neighbor.blocks[_indexNeighbor] == BlockType.Air)
                {
                    meshData.AddFaceV2(_blockPos, Vector3Int.left, blocks[i]);
                }
                else if (!_neighbor)
                {
                    meshData.AddFaceV2(_blockPos, Vector3Int.left, blocks[i]);
                }
            }

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlockPosInChunk(Vector3Int _blockPos)
    {
        ChunkParamFinal _chunkParam = ChunkManagerV2.Instance.ChunkParam;
        return (_blockPos.x >= 0 && _blockPos.x < _chunkParam.chunkSize) &&
                (_blockPos.y >= 0 && _blockPos.y < _chunkParam.chunkHeight) &&
                (_blockPos.z >= 0 && _blockPos.z < _chunkParam.chunkSize);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    public Vector3Int GetPositionWithIndex(int _index)
    {
        ChunkParamFinal _chunkParam = ChunkManagerV2.Instance.ChunkParam;
        int y = Mathf.FloorToInt((float)_index / _chunkParam.chunkHeight);
        _index = _index - y * _chunkParam.chunkHeight;
        int x = _index / _chunkParam.chunkSize;
        _index = _index - x * _chunkParam.chunkSize;
        int z = _index;
        return new Vector3Int(x, y, z);
    }
    public int GetIndexWithPosition(Vector3Int _blockPos)
    {
        ChunkParamFinal _chunkParam = ChunkManagerV2.Instance.ChunkParam;
        return _blockPos.x * _chunkParam.chunkSize + _blockPos.y * _chunkParam.chunkHeight + _blockPos.z;
    }
    public void AddNeighbor(Vector2Int _direction, ChunkV2 _chunk)
    {
        if(neighborChunk.ContainsKey(_direction))
            neighborChunk[_direction] = _chunk;
    }

    private void OnDrawGizmosSelected()
    { 
        foreach (var item in neighborChunk)
        {
            if (!item.Value) continue;
            Gizmos.DrawWireMesh(item.Value.meshData.Mesh, 0, item.Value.transform.position);    
        }
        int _count = blockRenders.Count;
        for (int i = 0; i < _count; ++i)
        {
            switch (blocks[blockRenders[i]])
            {
                case BlockType.Nothing:
                    break;
                case BlockType.Air:
                    break;
                case BlockType.Grass_Dirt:
                    Gizmos.color = Color.green;
                    break;
                case BlockType.Dirt:
                    Gizmos.color = Color.yellow;
                    break;
                case BlockType.Grass_Stone:
                    break;
                case BlockType.Stone:
                    break;
                case BlockType.TreeTrunk:
                    break;
                case BlockType.TreeLeafesTransparent:
                    break;
                case BlockType.TreeLeafsSolid:
                    break;
                case BlockType.Water:
                    Gizmos.color = Color.blue;
                    break;
                case BlockType.Sand:
                    break;
                default:
                    break;
            }
            Gizmos.DrawCube(transform.position + GetPositionWithIndex(blockRenders[i]), Vector3.one);
        }
        meshData.GizmoDebug(transform);
    }
}
