using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoaderC : MonoBehaviour
{
    public event Action<ChunkFinalC> OnChangeChunk = null;
    [SerializeField] ChunkFinalC currentChunk;
    [SerializeField] int renderDistance = 4;
    [SerializeField] List<ChunkFinalC> chunkLoad = new List<ChunkFinalC>();

    private void Start()
    {
        OnChangeChunk += (_chunk) =>
        {
            if (!_chunk) return;
            LoadNextChunk(_chunk.IndexChunk - currentChunk.IndexChunk);
            currentChunk = _chunk;
        };
        ChunkManagerFinalC.Instance.OnFinishLoad += () =>
        {
            ChunkFinalC _currentChunk = ChunkManagerFinalC.Instance.GetChunkFromWorldPosition(transform.position);
            currentChunk = _currentChunk;
            StartCoroutine(LoadChunkAround());
        };
    }
    public void LoadNextChunk(Vector2Int _direction)
    {
        int renderAmount = renderDistance + renderDistance + 1;
        if (_direction.y == 1)
        {
            for (int i = 0; i < renderAmount; i++)
            {
                int _indexToRemove = i * renderAmount;
                int _indexToAdd = renderAmount - 2 + _indexToRemove;
                chunkLoad[_indexToRemove]?.gameObject.SetActive(false);
                chunkLoad.RemoveAt(_indexToRemove);
                ChunkFinalC _chunkDirection = null;
                if (chunkLoad[_indexToAdd])
                    _chunkDirection = ChunkManagerFinalC.Instance.GetChunkFromIndexChunk(chunkLoad[_indexToAdd].IndexChunk + _direction);
                chunkLoad.Insert(_indexToAdd + 1, _chunkDirection);
                ActiveChunk(_chunkDirection);
            }
        }
        else if (_direction.y == -1)
        {
            for (int i = 0; i < renderAmount; i++)
            {
                int _indexToAdd = i * renderAmount;
                int _indexToRemove = renderAmount - 1 + _indexToAdd;
                chunkLoad[_indexToRemove]?.gameObject.SetActive(false);
                chunkLoad.RemoveAt(_indexToRemove);//4
                ChunkFinalC _chunkDirection = null;
                if (chunkLoad[_indexToAdd])
                    _chunkDirection = ChunkManagerFinalC.Instance.GetChunkFromIndexChunk(chunkLoad[_indexToAdd].IndexChunk + _direction);
                chunkLoad.Insert(_indexToAdd, _chunkDirection); //0
                ActiveChunk(_chunkDirection);
            }
        }
        if (_direction.x == 1)
        {
            DesactivateAndRemoveChunk(0, renderAmount);
            int _blockRightLast = renderAmount * renderAmount - renderAmount;
            for (int i = 0; i < renderAmount; i++)
            {
                ChunkFinalC _chunkDirection = null;
                if(chunkLoad[_blockRightLast - renderAmount + i])
                    _chunkDirection = ChunkManagerFinalC.Instance.GetChunkFromIndexChunk(chunkLoad[_blockRightLast - renderAmount + i].IndexChunk + _direction);
                chunkLoad.Add(_chunkDirection);
                ActiveChunk(_chunkDirection);
            }
        }
        else if(_direction.x == -1)
        {
            int _blockRightFirst = renderAmount * renderAmount - renderAmount;
            DesactivateAndRemoveChunk(_blockRightFirst, renderAmount);
            ChunkFinalC[] _chunkToAdd = new ChunkFinalC[renderAmount];
            for (int i = 0; i < renderAmount; i++)
            {
                ChunkFinalC _chunkDirection = null;
                if(chunkLoad[i])
                    _chunkDirection = ChunkManagerFinalC.Instance.GetChunkFromIndexChunk(chunkLoad[i].IndexChunk + _direction);
                _chunkToAdd[i] = _chunkDirection;
                ActiveChunk(_chunkDirection);
            }
            chunkLoad.InsertRange(0, _chunkToAdd);
        }   
    }
    void DesactivateAndRemoveChunk(int _indexStart, int _count)
    {
        for (int i = _indexStart; i < _count + _indexStart; i++)
            chunkLoad[i]?.gameObject.SetActive(false);
        chunkLoad.RemoveRange(_indexStart, _count);
    }
    public void ActiveChunk(ChunkFinalC _chunk)
    {
        if (!_chunk) return;
        _chunk.UpdateMesh();
        _chunk.gameObject.SetActive(true);
    }
    public IEnumerator LoadChunkAround()
    {
        for (int x = -renderDistance; x < renderDistance + 1; x++)
        {
            for (int z = -renderDistance; z < renderDistance + 1; z++)
            {
                Vector2Int _indexChunk = currentChunk.IndexChunk + new Vector2Int(x, z);
                ChunkFinalC _chunkNeighbor = ChunkManagerFinalC.Instance.GetChunkFromIndexChunk(_indexChunk);
                ActiveChunk(_chunkNeighbor);
                chunkLoad.Add(_chunkNeighbor);
            }
            yield return null;
        }
    }
    void Update()
    {
        ChunkFinalC _currentChunk = ChunkManagerFinalC.Instance.GetChunkFromWorldPosition(transform.position);
        if(currentChunk != _currentChunk)
        {
            if(currentChunk)
            {
                OnChangeChunk?.Invoke(_currentChunk);
                currentChunk.gameObject.SetActive(true);
            }
        }
    }
}
