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
    ChunkFinal[,] chunks;
    bool pass = true;

    public WorldParam WorldParam => worldParam;
    public ChunkParamFinal ChunkParam => chunkParam;

    private IEnumerator Start()
    {
        chunkParam = new ChunkParamFinal(worldParam.chunkSize, worldParam.chunkHeight, 1);
        yield return GenerateChunks();
    }
    private void Update()
    {
        if (pass && ThreadManager.Instance.IsEmptyThreads)
        {
            StartCoroutine(RenderChunks());
            OnFinishLoad?.Invoke();
            Debug.Log("time : " + Time.time);
            pass = false;
        }
        for (int x = 0; x < worldParam.chunkAmount; x++)
        {
            for (int z = 0; z < worldParam.chunkAmount; z++)
            {
                Transform _chunkT = chunks[x, z].transform;
                Vector3 _posChunk = new Vector3(_chunkT.position.x + chunks[x, z].IndexChunk.x * 8 + 8, Camera.main.transform.position.y, _chunkT.position.z + chunks[x, z].IndexChunk.y * 8 + 8);;
                Vector3 _direction = _posChunk - Camera.main.transform.position;
                if (_direction.sqrMagnitude < 200)
                {
                    
                    continue;
                }
                float _dot = Vector3.Dot(Camera.main.transform.forward, _direction);
                if(_dot < viewFrustrum)
                    _chunkT.gameObject.SetActive(false);
                else
                    _chunkT.gameObject.SetActive(true);
            }
        }
    }
    IEnumerator GenerateChunks()
    {
        yield return CreateChunks();
        //yield return new WaitForSeconds(1f);
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
                ChunkFinal _chunkFinal = Instantiate<ChunkFinal>(chunksPrefab, new Vector3(x * _chunkSize, 0, z * _chunkSize), Quaternion.identity, transform);
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
