using System;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public event Action<ChunkFinal> OnChangeChunk = null;
    [SerializeField] ChunkFinal currentChunk;
    [SerializeField] int renderDistance = 4;
    ChunkFinal[,] chunkLoad;

    private void Start()
    {
        chunkLoad = new ChunkFinal[renderDistance * 2, renderDistance * 2];
        OnChangeChunk += (_chunk) =>
        {
            UnrenderChunk();
            LoadChunkAround();
        };
        ChunkManagerFinal.Instance.OnFinishLoad += () =>
        {
            LoadChunkAround();
        };
    }
    public void LoadChunkAround()
    {
        for (int x = -renderDistance; x < renderDistance; x++)
        {
            for (int z = -renderDistance; z < renderDistance; z++)
            {
                Vector2Int _indexChunk = currentChunk.IndexChunk + new Vector2Int(x, z);
                ChunkFinal _chunkNeighbor = ChunkManagerFinal.Instance.GetChunkFromIndexChunk(_indexChunk);
                if (_chunkNeighbor)
                {
                    //Debug.Log(_indexChunk + " " + new Vector2Int(x + renderDistance, z + renderDistance));
                    _chunkNeighbor.gameObject.SetActive(true);
                    _chunkNeighbor.UpdateMesh();
                    chunkLoad[x + renderDistance, z + renderDistance] = _chunkNeighbor;
                }
            }
        }
    }
    public void UnrenderChunk()
    {
        for (int x = 0; x < renderDistance * 2; x++)
        {
            for (int z = 0; z < renderDistance * 2; z++)
            {
                if(chunkLoad[x, z])
                    chunkLoad[x, z].gameObject.SetActive(false);
            }
        }
    }
    void Update()
    {
        ChunkFinal _currentChunk = ChunkManagerFinal.Instance.GetChunkFromWorldPosition(transform.position);
        
        if(_currentChunk && currentChunk != _currentChunk)
        {
            Vector3 _direction;
            if (currentChunk)
                _direction = _currentChunk.transform.position - currentChunk.transform.position;
            
            currentChunk = _currentChunk;
            OnChangeChunk?.Invoke(currentChunk);
            currentChunk.gameObject.SetActive(true);
        }
    }
}
