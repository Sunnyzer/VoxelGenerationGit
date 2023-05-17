using System;
using System.Collections;
using UnityEngine;

public class ChunkManager : Singleton<ChunkManager>
{
    public event Action OnFinishLoad = null;

    [SerializeField] Chunk chunksPrefab;
    [SerializeField] int chunkAmount = 5;
    [SerializeField] WorldParam worldParam;
    [SerializeField] Chunk[,] chunks;
    [SerializeField] ChunkParam chunkParam;
    float timeRender;
    float timeData;

    public WorldParam WorldParam => worldParam;
    public ChunkParam ChunkParam => chunkParam;
    public Chunk[,] Chunks => chunks;

    private IEnumerator Start() => GenerateVoxels();
    public IEnumerator GenerateVoxels()
    {
        timeRender = Time.time;
        timeData = Time.time;
        yield return GenerateChunks();
    }
    IEnumerator GenerateChunks()
    {
        yield return CreateChunks();
        SetChunksNeighbor();
        Debug.Log("Generation Data finish in : " + (Time.time - timeData));
        yield return RenderChunks();
        Debug.Log("Generation Render finish in : " + (Time.time - timeRender));
    }
    IEnumerator CreateChunks()
    {
        int _chunkSize = chunkParam.chunkSize;
        chunks = new Chunk[chunkAmount, chunkAmount];
        for (int x = 0; x < chunkAmount; ++x)
        {
            for (int z = 0; z < chunkAmount; ++z)
            {
                Chunk _chunkFinal = Instantiate(chunksPrefab, new Vector3(x * _chunkSize, 0, z * _chunkSize), Quaternion.identity, transform);
                yield return _chunkFinal.Init(chunkParam, x, z);
                _chunkFinal.name = "Chunk" + (z + x * chunkAmount);
                chunks[x, z] = _chunkFinal;
            }
        }
        yield return null;
    }
    IEnumerator RenderChunks()
    {
        for (int x = 0; x < chunkAmount; x++)
            for (int z = 0; z < chunkAmount; z++)
                yield return chunks[x, z].Render();
    }

    void SetChunksNeighbor()
    {
        int _chunkLenght0 = chunks.GetLength(0);
        int _chunkLenght1 = chunks.GetLength(1);
        for (int x = 0; x < chunkAmount; ++x)
        {
            for (int z = 0; z < chunkAmount; ++z)
            {
                int _count = Direction.direction2D.Count;
                for (int i = 0; i < _count; i++)
                {
                    Vector2Int _direction = Direction.direction2D[i];
                    int _neighborX = x + _direction.x;
                    int __neighborZ = z + _direction.y;
                    if(_neighborX >= 0 && _neighborX < _chunkLenght0 && __neighborZ >= 0 && __neighborZ < _chunkLenght1)
                        chunks[x, z].AddNeighbor(_direction, chunks[_neighborX, __neighborZ]);
                }
            }
        }
    }

    public Chunk GetChunkFromWorldPosition(float _worldPosX, float _worldPosY, float _worldPosZ)
    {
        int x = Mathf.RoundToInt(_worldPosX) / chunkParam.chunkSize;
        int z = Mathf.RoundToInt(_worldPosZ) / chunkParam.chunkSize;
        if (IsIndexChunkInChunkManager(x, z))
            return chunks[x, z];
        return null;
    }
    public Chunk GetChunkFromWorldPosition(Vector3 _worldPos)
    {
        return GetChunkFromWorldPosition(_worldPos.x, _worldPos.y, _worldPos.z);
    }
    public Chunk GetChunkFromIndexChunk(int _indexChunkX, int _indexChunkZ)
    {
        if (IsIndexChunkInChunkManager(_indexChunkX, _indexChunkZ))
            return chunks[_indexChunkX, _indexChunkZ];
        return null;
    }
    public Chunk GetChunkFromIndexChunk(Vector2Int _indexChunk)
    {
        return GetChunkFromIndexChunk(_indexChunk.x, _indexChunk.y);
    }
    public bool IsIndexChunkInChunkManager(int _indexChunkX, int _indexChunkZ)
    {
        return (_indexChunkX < chunkAmount && _indexChunkX >= 0) &&
               (_indexChunkZ < chunkAmount && _indexChunkZ >= 0);
    }

    public float PerlinNoiseOctaves(int x, int z)
    {
        float _value = 0;
        float _amplitude = worldParam.amplitude;
        float _frequence = worldParam.frequence;
        for (int i = 0; i < worldParam.octaves; i++)
        {
            _value += Mathf.PerlinNoise(x * _frequence, z * _frequence) * _amplitude;
            _amplitude *= worldParam.persistence;
            _frequence *= worldParam.lacunarity;
        }
        return _value;
    }
}
