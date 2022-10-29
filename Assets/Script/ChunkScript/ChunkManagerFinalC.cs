using System;
using System.Collections;
using UnityEngine;

public class ChunkManagerFinalC : Singleton<ChunkManagerFinalC>
{
    public event Action OnFinishLoad = null;
    [SerializeField] WorldParam worldParam;
    [SerializeField] ChunkParamFinal chunkParam;
    [SerializeField] ChunkFinalC chunksPrefab;
    [SerializeField] Vector2Int offset;
    ChunkFinalC[,] chunks;
    bool pass = true;

    public WorldParam WorldParam => worldParam;
    public ChunkParamFinal ChunkParam => chunkParam;
    public ChunkFinalC[,] Chunks => chunks;

    private IEnumerator Start()
    {
        chunkParam = new ChunkParamFinal(worldParam.chunkSize, worldParam.chunkHeight, 1);
        GenerateChunks();
        yield return null;
    }
    private void Update()
    {
        if (pass && ThreadManager.Instance.IsEmptyThreads)
        {
            OnFinishLoad?.Invoke();
            Debug.Log("time : " + Time.time);
            pass = false;
        }
    }
    void GenerateChunks()
    {
        CreateChunks();
        for (int x = 0; x < worldParam.chunkAmount; x++)
        {
            for (int z = 0; z < worldParam.chunkAmount; z++)
            {
                ThreadManager.instance.AddThread(chunks[x,z].GenerateBlocks);
            }
        }
    }
    void CreateChunks()
    {
        int _chunkAmount = worldParam.chunkAmount;
        int _chunkSize = worldParam.chunkSize;
        chunks = new ChunkFinalC[_chunkAmount, _chunkAmount];
        for (int x = 0; x < _chunkAmount; x++)
        {
            for (int z = 0; z < _chunkAmount; z++)
            {
                ChunkFinalC _chunkFinal = Instantiate<ChunkFinalC>(chunksPrefab, new Vector3(x * _chunkSize, 0, z * _chunkSize), Quaternion.identity, transform);
                _chunkFinal.SetChunk(x, z);
                _chunkFinal.name = "Chunk" + (z + x * _chunkAmount);
                chunks[x, z] = _chunkFinal;
                _chunkFinal.gameObject.SetActive(false);
            }
        }
    }
    IEnumerator RenderChunks()
    {
        int _chunkAmount = worldParam.chunkAmount;
        for (int x = 0; x < _chunkAmount; x++)
            for (int z = 0; z < _chunkAmount; z++)
                chunks[x, z].UpdateMesh();

        yield return null;
    }
    
    public ChunkFinalC GetChunkFromWorldPosition(float _worldPosX, float _worldPosY, float _worldPosZ)
    {
        int x = Mathf.RoundToInt(_worldPosX) / (worldParam.chunkSize);
        int z = Mathf.RoundToInt(_worldPosZ) / (worldParam.chunkSize);
        if(IsIndexChunkInChunkManager(x, z))
            return chunks[x, z];
        return null;
    }
    public ChunkFinalC GetChunkFromWorldPosition(Vector3 _worldPos)
    {
        return GetChunkFromWorldPosition(_worldPos.x, _worldPos.y, _worldPos.z);
    }
    public ChunkFinalC GetChunkFromIndexChunk(int _indexChunkX, int _indexChunkZ)
    {
        if(IsIndexChunkInChunkManager(_indexChunkX, _indexChunkZ))
            return chunks[_indexChunkX, _indexChunkZ];
        return null;
    }
    public ChunkFinalC GetChunkFromIndexChunk(Vector2Int _indexChunk)
    {
        return GetChunkFromIndexChunk(_indexChunk.x, _indexChunk.y);
    }
    public bool IsIndexChunkInChunkManager(int _indexChunkX, int _indexChunkZ)
    {
        return (_indexChunkX < worldParam.chunkAmount && _indexChunkX >= 0) &&
               (_indexChunkZ < worldParam.chunkAmount && _indexChunkZ >= 0);
    }
    public bool IsIndexChunkInChunkManager(Vector2Int _indexChunk)
    {
        return IsIndexChunkInChunkManager(_indexChunk.x, _indexChunk.y);
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
