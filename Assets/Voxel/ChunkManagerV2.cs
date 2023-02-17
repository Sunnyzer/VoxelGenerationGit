using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChunkManagerV2 : Singleton<ChunkManagerV2>
{
    public event Action OnFinishLoad = null;
    [SerializeField] WorldParam worldParam;
    [SerializeField] ChunkParamFinal chunkParam;
    [SerializeField] ChunkV2 chunksPrefab;
    [SerializeField] ChunkV2[,] chunks;
    [SerializeField] Texture2D texture;
    float time;

    public WorldParam WorldParam => worldParam;
    public ChunkParamFinal ChunkParam => chunkParam;
    public ChunkV2[,] Chunks => chunks;

    private IEnumerator Start()
    {
        texture = new Texture2D(256, 256);
        Color[] pix = new Color[256 * 256];
        for (int i = 0; i < 256; i++)
        {
            for (int j = 0; j < 256; j++)
            {
                float _perlin = Mathf.PerlinNoise((float)i/256, (float)j /256);
                pix[j * 256 + i] = new Color(_perlin, _perlin, _perlin);
            }
        }
        texture.SetPixels(pix);
        texture.Apply();
        yield return GenerateVoxels();
    }
    public IEnumerator GenerateVoxels()
    {
        time = Time.time;
        chunkParam = new ChunkParamFinal(worldParam.chunkSize, worldParam.chunkHeight, 1);
        yield return GenerateChunks();
    }
    IEnumerator GenerateChunks()
    {
        yield return CreateChunks();
        GiveNeighbor();
        yield return RenderChunks();
        Debug.Log("Generation finish in : " + (Time.time - time));
    }
    IEnumerator CreateChunks()
    {
        int _chunkAmount = worldParam.chunkAmount;
        int _chunkSize = worldParam.chunkSize;
        chunks = new ChunkV2[_chunkAmount, _chunkAmount];
        for (int x = 0; x < _chunkAmount; ++x)
        {
            for (int z = 0; z < _chunkAmount; ++z)
            {
                ChunkV2 _chunkFinal = Instantiate(chunksPrefab, new Vector3(x * _chunkSize, 0, z * _chunkSize), Quaternion.identity, transform);
                yield return _chunkFinal.Init(chunkParam, x, z);
                _chunkFinal.name = "Chunk" + (z + x * _chunkAmount);
                chunks[x, z] = _chunkFinal;
            }
        }
        yield return null;
    }
    void GiveNeighbor()
    {
        int _chunkAmount = worldParam.chunkAmount;
        for (int x = 0; x < _chunkAmount; ++x)
        {
            for (int z = 0; z < _chunkAmount; ++z)
            {
                for (int i = 0; i < Direction.direction2D.Count; i++)
                {
                    int _x = x + Direction.direction2D[i].x;
                    int _z = z + Direction.direction2D[i].y;
                    if(_x >= 0 && _x < chunks.GetLength(0) && _z >= 0 && _z < chunks.GetLength(1))
                        chunks[x, z].AddNeighbor(Direction.direction2D[i], chunks[_x, _z]);
                }
            }
        }
    }
    IEnumerator RenderChunks()
    {
        for (int x = 0; x < worldParam.chunkAmount; x++)
            for (int z = 0; z < worldParam.chunkAmount; z++)
                yield return chunks[x, z].RenderMesh();
    }
    public ChunkV2 GetChunkFromWorldPosition(float _worldPosX, float _worldPosY, float _worldPosZ)
    {
        int x = Mathf.RoundToInt(_worldPosX) / (worldParam.chunkSize);
        int z = Mathf.RoundToInt(_worldPosZ) / (worldParam.chunkSize);
        if (IsIndexChunkInChunkManager(x, z))
            return chunks[x, z];
        return null;
    }
    public ChunkV2 GetChunkFromWorldPosition(Vector3 _worldPos)
    {
        return GetChunkFromWorldPosition(_worldPos.x, _worldPos.y, _worldPos.z);
    }
    public ChunkV2 GetChunkFromIndexChunk(int _indexChunkX, int _indexChunkZ)
    {
        if (IsIndexChunkInChunkManager(_indexChunkX, _indexChunkZ))
            return chunks[_indexChunkX, _indexChunkZ];
        return null;
    }
    public bool IsIndexChunkInChunkManager(int _indexChunkX, int _indexChunkZ)
    {
        return (_indexChunkX < worldParam.chunkAmount && _indexChunkX >= 0) &&
               (_indexChunkZ < worldParam.chunkAmount && _indexChunkZ >= 0);
    }
    public ChunkV2 GetChunkFromIndexChunk(Vector2Int _indexChunk)
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
            _value += Mathf.PerlinNoise((x) * _frequence, (z) * _frequence) * _amplitude;
            _amplitude *= worldParam.persistence;
            _frequence *= worldParam.lacunarity;
        }
        return _value;
    }
    public static float PerlinNoiseOctaves(int x, int z, float amplitude, float frequence, int octaves, float persistence, float lacunarity)
    {
        float _value = 0;
        float _amplitude = amplitude;
        float _frequence = frequence;
        for (int i = 0; i < octaves; i++)
        {
            _value += Mathf.PerlinNoise((x) * _frequence, (z) * _frequence) * _amplitude;
            _amplitude *= persistence;
            _frequence *= lacunarity;
        }
        return _value;
    }
}
