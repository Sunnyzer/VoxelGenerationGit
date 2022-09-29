using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    static private ChunkManager instance = null;
    static public ChunkManager Instance => instance;
	[SerializeField] MyChunk[] chunks;
	[SerializeField] float noiseScale = 0.03f;
	[SerializeField] int chunkSize = 8;
	[SerializeField] int chunkHeight = 25;
    [SerializeField] int chunksAmountX = 10;
    [SerializeField] int chunksAmountY = 10;
	[SerializeField] MyChunk chunkPrefab = null;
    static public int x = 0;
    static public int y = 0;
    private void Awake()
    {
        if(instance)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }
    public MyChunk GetChunk(int x,int z)
    {
        int _posX = x / 15;
        int _posZ = z / 15;
        if (_posX < 0 || _posZ < 0) return null; 
        int _index = _posX + _posZ * chunksAmountX;
        if(_index < chunks.Length && _index >= 0)
            return chunks[_index];
        return null;
    }
    private void Start()
    {
        x = Random.Range(0,0);
        y = Random.Range(0,0);
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
