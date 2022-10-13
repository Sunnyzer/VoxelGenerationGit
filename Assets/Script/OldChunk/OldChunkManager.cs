using System;
using System.Collections;
using UnityEngine;

public class OldChunkManager : MonoBehaviour
{
    static private OldChunkManager instance = null;
    static public OldChunkManager Instance => instance;
    public event Action OnFinishLoad = null;
    public static int noisePosX = 0;
    public static int noisePosY = 0;
    [SerializeField] float noiseScale = 0.03f;
    [SerializeField] int chunkSize = 8;
    [SerializeField] int chunkHeight = 25;
    [SerializeField] int chunksAmountX = 10;
    [SerializeField] int chunksAmountZ = 10;
    [SerializeField] OldChunk chunkPrefab = null;
    [SerializeField] bool onDebug = false;
    OldChunk[,] chunks;
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
    private IEnumerator Start()
    {
        yield return GenerateMap();
    }
    public OldBlockData GetBlockDataFromWorldPosition(Vector3Int _posBlock)
    {
        Vector2Int _chunkPosBlock = GetChunkIndexFromWorldPosition(_posBlock);
        OldChunk _chunkBlock = GetChunk(_chunkPosBlock.x, _chunkPosBlock.y);
        if (!_chunkBlock) return null;
        Vector3Int _posBlockInChunk = new Vector3Int(_posBlock.x - chunkSize * _chunkPosBlock.x, _posBlock.y, _posBlock.z - chunkSize * _chunkPosBlock.y);
        if (!_chunkBlock.IsBlockInChunk(_posBlockInChunk)) return null;
        return _chunkBlock.BlockDatas[_posBlockInChunk.x, _posBlockInChunk.y, _posBlockInChunk.z];
    }
    public OldChunk GetChunk(int _x, int _z)
    {
        if (IsCoordInChunk(_x,_z))
            return chunks[_x,_z];
        return null;
    }
    public OldChunk GetChunk(Vector2Int _chunkIndex) => GetChunk(_chunkIndex.x, _chunkIndex.y);
    public bool IsCoordInChunk(int _x,int _z) => _x < chunks.GetLength(0) && _z < chunks.GetLength(1) && _x >= 0 && _z >= 0;
    public Vector2Int GetChunkIndexFromWorldPosition(Vector3 _pos) => new Vector2Int((int)_pos.x / chunkSize, (int)_pos.z / chunkSize);
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
    public IEnumerator CreateChunks(int _sizeX,int _sizeY)
    {
        chunks = new OldChunk[_sizeX, _sizeY];
        for (int i = 0; i < _sizeX; ++i)
        {
            for (int j = 0; j < _sizeY; ++j)
            {
                OldChunk myChunk = Instantiate<OldChunk>(chunkPrefab,transform.position + new Vector3(i * chunkSize, 0, j * chunkSize), Quaternion.identity, transform);
                yield return myChunk.Init(noiseScale, chunkSize, chunkHeight);
                myChunk.name = "myChunk " + (i * _sizeX + j);
                chunks[i, j] = myChunk;
            }
        }
    }
    public IEnumerator UpdateChunk()
    {
        for (int x = 0; x < chunksAmountX; ++x)
            for (int z = 0; z < chunksAmountZ; ++z)
                yield return chunks[x, z].SetMakeMesh();
    }
}
