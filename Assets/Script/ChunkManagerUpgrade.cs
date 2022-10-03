using System;
using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ChunkManagerUpgrade : MonoBehaviour
{
    static private ChunkManagerUpgrade instance = null;
    static public ChunkManagerUpgrade Instance => instance;
    public event Action OnFinishLoad = null;
    public static int x = 0;
    public static int y = 0;
    [SerializeField] float noiseScale = 0.03f;
    [SerializeField] int chunkSize = 8;
    [SerializeField] int chunkHeight = 25;
    [SerializeField] int chunksAmountX = 10;
    [SerializeField] int chunksAmountZ = 10;
    [SerializeField] bool onDebug = false;
    [SerializeField] ChunkUpgrade chunkPrefab = null;
    [SerializeField] int radiusChunks = 2;
    ChunkUpgrade[,] chunks;
    public float NoiseScale => noiseScale;
    public int ChunkSize => chunkSize;
    public int ChunkHeight => chunkHeight;
    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }
    public BlockData GetBlockDataFromWorldPosition(Vector3Int _posBlock)
    {
        Vector2Int _r = new Vector2Int(_posBlock.x / chunkSize, _posBlock.z / chunkSize);
        ChunkUpgrade _chunk = GetChunk(_r.x, _r.y);
        if (!_chunk) return null;
        Vector3Int _posBlockInChunk = new Vector3Int(_posBlock.x - chunkSize * _r.x, _posBlock.y, _posBlock.z - chunkSize * _r.y);
        if (!_chunk.IsBlockInChunk(_posBlockInChunk)) return null;
        BlockData _blockData = _chunk.BlockDatas[_posBlockInChunk.x, _posBlockInChunk.y, _posBlockInChunk.z];
        return _blockData;
    }
    public ChunkUpgrade GetChunk(int x, int z)
    {
        if (x < chunks.GetLength(0) && z < chunks.GetLength(1) && x >= 0 && z >= 0)
            return chunks[x,z];
        return null;
    }

    public Vector2Int GetChunkIndexFromWorldPosition(Vector3 _pos)
    {
        return new Vector2Int((int)_pos.x / chunkSize, (int)_pos.z / chunkSize);
    }
    private IEnumerator Start()
    {
        yield return GenerateMap();
    }
    private IEnumerator GenerateMap()
    {
        x = UnityEngine.Random.Range(0, 10000);
        y = UnityEngine.Random.Range(0, 10000);
        yield return CreateChunks(chunksAmountX, chunksAmountZ);

        if (onDebug)
            Debug.Log("Create chunks : " + Time.time);
        
        float _timeFinishChunk = Time.time;
        yield return UpdateChunk();

        if (onDebug)
            Debug.Log("Finish load all chunks : " + (Time.time - _timeFinishChunk));
        OnFinishLoad?.Invoke();
    }
    public IEnumerator CreateChunks(int _sizeX,int _sizeY)
    {
        chunks = new ChunkUpgrade[_sizeX, _sizeY];
        for (int i = 0; i < _sizeX; i++)
        {
            for (int j = 0; j < _sizeY; j++)
            {
                ChunkUpgrade myChunk = Instantiate<ChunkUpgrade>(chunkPrefab, new Vector3(i * chunkSize, 0, j * chunkSize), Quaternion.identity, transform);
                yield return myChunk.Init(noiseScale, chunkSize, chunkHeight);
                myChunk.name = "myChunk " + (i * _sizeX + j);
                chunks[i, j] = myChunk;
            }
        }
    }
    public IEnumerator UpdateChunk()
    {
        for (int x = 0; x < chunksAmountX; x++)
            for (int z = 0; z < chunksAmountZ; z++)
                yield return chunks[x, z].SetMakeMesh();
    }
    public void UpdateChunkAtPos(Vector3 _pos)
    {
        Vector2Int _indexChunk = GetChunkIndexFromWorldPosition(_pos);
        for (int x = -radiusChunks; x < radiusChunks; x++)
        {
            for (int z = -radiusChunks; z < radiusChunks; z++)
            {
                ChunkUpgrade _chunkToRender = GetChunk(_indexChunk.x + x, _indexChunk.y + z);
                if (!_chunkToRender) continue;
                StartCoroutine(_chunkToRender.SetMakeMesh());
            }
        }
    }
    public void UpdateChunkFromChunk(ChunkUpgrade _currentChunk) => UpdateChunkAtPos(_currentChunk.transform.position);
}
