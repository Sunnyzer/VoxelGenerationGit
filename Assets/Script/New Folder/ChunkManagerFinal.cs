using System;
using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ChunkManagerFinal : Singleton<ChunkManagerFinal>
{
    public event Action OnFinishLoad = null;
    [SerializeField] WorldParam worldParam;
    [SerializeField] ChunkParamFinal chunkParam;
    [SerializeField] ChunkFinal chunksPrefab;
    [SerializeField] Vector2Int offset;
    [SerializeField] float viewFrustrum = -0.1f;
    [SerializeField] Player player;
    [SerializeField] ChunkFinal currentChunk;
    [SerializeField] int chunkAmount;
    ChunkFinal[,] chunks;
    bool pass = true;
    public event Action<ChunkFinal> OnChangeChunk;

    public WorldParam WorldParam => worldParam;
    public ChunkParamFinal ChunkParam => chunkParam;
    public ChunkFinal[,] Chunks => chunks;

    private IEnumerator Start()
    {
        player = FindObjectOfType<Player>();
        chunkParam = new ChunkParamFinal(worldParam.chunkSize, worldParam.chunkHeight, 1);
        chunkAmount = worldParam.chunkAmount;
        OnChangeChunk += (test) => Debug.Log(test.name);
        yield return GenerateChunks();
    }

    private void Update()
    {
        if (pass && ThreadManager.Instance.IsEmptyThreads)
        {
            //StartCoroutine(RenderChunks());
            OnFinishLoad?.Invoke();
            Debug.Log("time : " + Time.time);
            pass = false;
        }
        int _chunkSizeHalf = worldParam.chunkSize/2;
    }

    IEnumerator GenerateChunks()
    {
        yield return CreateChunks();
        yield return InitChunks();
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
                ChunkFinal _chunkFinal = Instantiate(chunksPrefab, new Vector3(x * _chunkSize, 0, z * _chunkSize), Quaternion.identity, transform);
                _chunkFinal.SetChunk(x, z);
                _chunkFinal.name = "Chunk" + (z + x * _chunkAmount);
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
                ThreadManager.Instance.AddThread(chunks[x, z].InitChunks);
        yield return null;
    }
    IEnumerator RenderChunks()
    {
        int _chunkAmount = worldParam.chunkAmount;
        for (int x = 0; x < _chunkAmount; x++)
            for (int z = 0; z < _chunkAmount; z++)
                chunks[x, z].UpdateMesh();

        yield return null;
    }
    
    public ChunkFinal GetChunkFromWorldPosition(float _worldPosX, float _worldPosY, float _worldPosZ)
    {
        int x = Mathf.RoundToInt(_worldPosX) / (worldParam.chunkSize);
        int z = Mathf.RoundToInt(_worldPosZ) / (worldParam.chunkSize);
        if(IsIndexChunkInChunkManager(x, z))
            return chunks[x, z];
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
    public bool IsIndexChunkInChunkManager(Vector2Int _indexChunk)
    {
        return IsIndexChunkInChunkManager(_indexChunk.x, _indexChunk.y);
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
