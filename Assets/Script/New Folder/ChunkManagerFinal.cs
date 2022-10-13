using System.Collections;
using UnityEngine;

public class ChunkManagerFinal : Singleton<ChunkManagerFinal>
{
    [SerializeField] WorldParam worldParam;
    [SerializeField] ChunkFinal chunksPrefab;
    [SerializeField] Vector2Int offset;
    ChunkFinal[,] chunks;

    public WorldParam WorldParam => worldParam;

    private IEnumerator Start()
    {
        yield return GenerateChunks();
    }

    IEnumerator GenerateChunks()
    {
        yield return CreateChunks();
        yield return InitChunks();
        yield return RenderChunks();
    }
    IEnumerator CreateChunks()
    {
        int _chunkAmount = worldParam.chunkAmount;
        int _chunkSize = worldParam.chunkSize;
        chunks = new ChunkFinal[_chunkAmount, _chunkAmount];
        for (int x = 0; x < _chunkAmount; x++)
        {
            for (int z = 0; z < _chunkAmount; z++)
            {
                ChunkFinal _chunkFinal = Instantiate<ChunkFinal>(chunksPrefab, new Vector3(x * _chunkSize, 0, z * _chunkSize), Quaternion.identity, transform);
                yield return _chunkFinal.GenerateBlocks(new ChunkParamFinal(worldParam.chunkSize, worldParam.chunkHeight, worldParam.sizeBlock,new Vector2Int(x,z)));
                _chunkFinal.name = "Chunk" + (x + z * _chunkAmount); 
                chunks[x, z] = _chunkFinal;
            }
        }
        yield return null;
    }
    IEnumerator InitChunks()
    {
        int _chunkAmount = worldParam.chunkAmount;
        for (int x = 0; x < _chunkAmount; x++)
            for (int z = 0; z < _chunkAmount; z++)
                yield return chunks[x, z].InitChunks();
    }
    IEnumerator RenderChunks()
    {
        int _chunkAmount = worldParam.chunkAmount;
        for (int x = 0; x < _chunkAmount; x++)
            for (int z = 0; z < _chunkAmount; z++)
                yield return chunks[x, z].UpdateMesh();
    }
    
    public ChunkFinal GetChunkFromWorldPosition(float _worldPosX, float _worldPosY, float _worldPosZ)
    {
        return null;
    }
    public ChunkFinal GetChunkFromWorldPosition(Vector3 _worldPos)
    {
        return GetChunkFromWorldPosition(_worldPos.x, _worldPos.y, _worldPos.z);
    }
    public ChunkFinal GetChunkFromIndexChunk(int _indexChunkX, int _indexChunkZ)
    {
        if(IsIndexChunkInChunkManager(_indexChunkX, _indexChunkZ))
            return chunks[_indexChunkX, _indexChunkZ];
        return null;
    }
    public bool IsIndexChunkInChunkManager(int _indexChunkX, int _indexChunkZ)
    {
        return (_indexChunkX < worldParam.chunkAmount && _indexChunkX >= 0) &&
               (_indexChunkZ < worldParam.chunkAmount && _indexChunkZ >= 0);
    }
    public ChunkFinal GetChunkFromIndexChunk(Vector2Int _indexChunk)
    {
        return GetChunkFromIndexChunk(_indexChunk.x, _indexChunk.y);
    }

    public float PerlinNoiseOctaves(int x, int z)
    {
        float _value = 0;
        float _amplitude = worldParam.amplitude;
        float _frequence = worldParam.frequence;
        for (int i = 0; i < worldParam.octaves; i++)
        {
            _value += Mathf.PerlinNoise((x + offset.x) * _frequence, (z + offset.y) * _frequence) * _amplitude;
            _amplitude *= worldParam.persistence;
            _frequence *= worldParam.lacunarity;
        }
        return _value;
    }
}
