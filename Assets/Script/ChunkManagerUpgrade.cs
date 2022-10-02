using System.Collections;
using UnityEngine;

public class ChunkManagerUpgrade : MonoBehaviour
{
    static private ChunkManagerUpgrade instance = null;
    static public ChunkManagerUpgrade Instance => instance;
    [SerializeField] float noiseScale = 0.03f;
    [SerializeField] int chunkSize = 8;
    [SerializeField] int chunkHeight = 25;
    [SerializeField] int chunksAmountX = 10;
    [SerializeField] int chunksAmountZ = 10;
    [SerializeField] bool onDebug = false;
    [SerializeField] ChunkUpgrade chunkPrefab = null;
    [SerializeField] ChunkUpgrade[,] chunks;
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
    public ChunkUpgrade GetChunk(int x, int z)
    {
        if (x < chunks.GetLength(0) && z < chunks.GetLength(1) && x >= 0 && z >= 0)
            return chunks[x,z];
        return null;
    }
    private IEnumerator Start()
    {
        chunks = new ChunkUpgrade[chunksAmountX, chunksAmountZ];
        for (int i = 0; i < chunksAmountX; i++)
        {
            for (int j = 0; j < chunksAmountZ; j++)
            {
                ChunkUpgrade myChunk = Instantiate<ChunkUpgrade>(chunkPrefab, new Vector3(i * chunkSize, 0, j * chunkSize), Quaternion.identity, transform);
                yield return myChunk.Init(noiseScale, chunkSize, chunkHeight);
                myChunk.name = "myChunk " + (i * chunksAmountX + j);
                chunks[i,j] = myChunk;
            }
        }
        if(onDebug)
            Debug.Log("Create chunks : " + Time.time);
        float _timeFinishChunk = Time.time;
        for (int x = 0; x < chunksAmountX; x++)
            for (int z = 0; z < chunksAmountZ; z++)
                yield return chunks[x,z].MakeMesh();
        if(onDebug)
            Debug.Log("Finish load all chunks : " + (Time.time - _timeFinishChunk));
    }
}
