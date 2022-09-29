using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    static private ChunkManager instance = null;
    static public ChunkManager Instance => instance;
	[SerializeField] MyChunk[] chunks;
	[SerializeField] float noiseScale = 0.03f;
	[SerializeField] int chunkSize = 8;
	[SerializeField] int chunkHeight = 25;
	int chunksAmountX = 10;
	int chunksAmountY = 10;
	[SerializeField] MyChunk chunkPrefab = null;
    static public int x = 0;
    static public int y = 0;
    [SerializeField] public int blockSize = 1;
    private void Awake()
    {
        if(instance)
        {
            return;
        }
        instance = this;
    }
    private void Start()
    {
        x = Random.Range(0,1);
        y = Random.Range(0,1);
        chunks = new MyChunk[chunksAmountX * chunksAmountY];
        for (int i = 0; i < chunksAmountX; i++)
        {
            for (int j = 0; j < chunksAmountY; j++)
            {
                MyChunk myChunk = Instantiate<MyChunk>(chunkPrefab, new Vector3(i * chunkSize, 0, j * chunkSize), Quaternion.identity, transform);
                myChunk.Init(noiseScale, chunkSize, chunkHeight);
                chunks[j + i * chunksAmountX] = myChunk;
            }
        }
    }
}
