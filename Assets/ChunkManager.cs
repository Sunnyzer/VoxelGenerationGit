using UnityEngine;

public class ChunkManager : MonoBehaviour
{
	[SerializeField] MyChunk[] chunks;
	[SerializeField] float noiseScale = 0.03f;
	[SerializeField] int chunkSize = 8;
	[SerializeField] int chunkHeight = 25;
	[SerializeField] int chunksAmountX = 3;
	[SerializeField] int chunksAmountY = 3;
	[SerializeField] MyChunk chunkPrefab = null;
    private void Start()
    {
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
