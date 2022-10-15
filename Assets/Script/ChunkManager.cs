using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    static private ChunkManager instance = null;
    static public ChunkManager Instance => instance;
    public event Action OnFinishLoad = null;
    public static int noisePosX = 0;
    public static int noisePosY = 0;
    public static float sizeBlock;
    [SerializeField] int chunksAmountX = 10;
    [SerializeField] int chunksAmountZ = 10;
    [SerializeField] Chunk chunkPrefab = null;
    [SerializeField] ChunkParam chunkParam;
    [SerializeField] bool onDebug = false;
    Chunk[,] chunks;
    public float NoiseScale => chunkParam.noiseScale;
    public int ChunkSize => chunkParam.chunkSize;
    public int ChunkHeight => chunkParam.chunkHeight;
    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }
    private IEnumerator Start()
    {
        sizeBlock = chunkParam.sizeBlock;
        yield return GenerateMap();
    }

    public Block GetBlockDataFromWorldPosition(Vector3Int _posBlock)
    {
        Vector2Int _chunkPosBlock = GetChunkIndexFromWorldPosition(_posBlock);
        Chunk _chunkBlock = GetChunk(_chunkPosBlock.x, _chunkPosBlock.y);
        if (!_chunkBlock) return null;
        Vector3Int _posBlockInChunk = new Vector3Int(_posBlock.x - chunkParam.chunkSize * _chunkPosBlock.x, _posBlock.y, _posBlock.z - chunkParam.chunkSize * _chunkPosBlock.y);
        if (!_chunkBlock.IsPosBlockInChunk(_posBlockInChunk)) return null;
        return _chunkBlock.Blocks[_posBlockInChunk.x, _posBlockInChunk.y, _posBlockInChunk.z];
    }
    public Chunk GetChunk(int _x, int _z)
    {
        if (IsCoordInChunk(_x, _z))
            return chunks[_x, _z];
        return null;
    }
    public Chunk GetChunk(Vector2Int _chunkIndex) => GetChunk(_chunkIndex.x, _chunkIndex.y);
    public bool IsCoordInChunk(int _x, int _z) => _x < chunks.GetLength(0) && _z < chunks.GetLength(1) && _x >= 0 && _z >= 0;
    public Vector2Int GetChunkIndexFromWorldPosition(Vector3 _pos) => new Vector2Int((int)_pos.x / chunkParam.chunkSize, (int)_pos.z / chunkParam.chunkSize);
    private IEnumerator GenerateMap()
    {
        noisePosX = UnityEngine.Random.Range(0, 10000);
        noisePosY = UnityEngine.Random.Range(0, 10000);
        yield return CreateChunks(chunksAmountX, chunksAmountZ);

        if (onDebug)
            Debug.Log("Create chunks : " + Time.time);

        float _timeFinishChunk = Time.time;
        yield return UpdateChunk();

        if (onDebug)
            Debug.Log("Finish load all chunks : " + (Time.time - _timeFinishChunk));
        OnFinishLoad?.Invoke();
    }
    public IEnumerator CreateChunks(int _sizeX, int _sizeY)
    {
        chunks = new Chunk[_sizeX, _sizeY];
        for (int i = 0; i < _sizeX; ++i)
        {
            for (int j = 0; j < _sizeY; ++j)
            {
                Chunk myChunk = Instantiate<Chunk>(chunkPrefab, transform.position + new Vector3(i * chunkParam.chunkSize, 0, j * chunkParam.chunkSize), Quaternion.identity, transform);
                myChunk.Init(new Vector2Int(i, j),chunkParam);
                myChunk.name = "myChunk " + (i * _sizeX + j);
                chunks[i, j] = myChunk;
            }
        }
        yield break;
    }
    public IEnumerator UpdateChunk()
    {
        for (int x = 0; x < chunksAmountX; ++x)
            for (int z = 0; z < chunksAmountZ; ++z)
                chunks[x, z].FinishInitChunk();
        yield break;
    }
}
