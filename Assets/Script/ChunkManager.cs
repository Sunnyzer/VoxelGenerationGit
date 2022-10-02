using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    static private ChunkManager instance = null;
    static public ChunkManager Instance => instance;
	[SerializeField] float noiseScale = 0.03f;
	[SerializeField] int chunkSize = 8;
	[SerializeField] int chunkHeight = 25;
    [SerializeField] int chunksAmountX = 10;
    [SerializeField] int chunksAmountY = 10;
	[SerializeField] Chunk chunkPrefab = null;
	List<Chunk> chunks = new List<Chunk>();
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
    public Chunk GetChunk(int x,int z)
    {
        int _index = z + x * chunksAmountX;
        if (_index < chunks.Count && _index >= 0)
            return chunks[_index]; 
        return null;
    }
    private IEnumerator Start()
    {
        x = Random.Range(0,0);
        y = Random.Range(0,0);
        for (int i = 0; i < chunksAmountX; i++)
        {
            for (int j = 0; j < chunksAmountY; j++)
            {
                Chunk myChunk = Instantiate<Chunk>(chunkPrefab, new Vector3(i * chunkSize, 0, j * chunkSize), Quaternion.identity, transform);
                yield return myChunk.Init(noiseScale, chunkSize, chunkHeight);
                myChunk.name = "myChunk " + (i* chunksAmountX + j);
                chunks.Add(myChunk);
            }
        }
        Debug.Log("Create chunks : " + Time.time);
        float _timeFinishChunk = Time.time;
        for (int i = 0; i < chunks.Count; i++)
        {
            yield return chunks[i].MakeMesh();
        }
        Debug.Log("Finish load all chunks : " + (Time.time - _timeFinishChunk));
    }
}
