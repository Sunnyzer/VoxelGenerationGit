using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Block
{
    public static List<Vector3Int> allDirection = new List<Vector3Int>()
    {
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down
    };
    public Vector3Int positionBlock;
    public Chunk owner;
    public BlockType blockType = BlockType.Dirt;
    public Dictionary<Vector3Int, Block> blocksNeighbor = new Dictionary<Vector3Int, Block>();
    public Dictionary<Vector3Int, Face> facePerDirection = new Dictionary<Vector3Int, Face>();
    public Block(Vector3Int _position,BlockType _blockType, Chunk _owner)
    {
        blockType = _blockType;
        positionBlock = _position;
        owner = _owner;
    }
    public void SetNeighbor(Chunk _chunk)
    { 
        foreach (Vector3Int _direction in allDirection)
        {
            Vector3Int _posNeighbor = positionBlock + _direction;
            if (_chunk.IsPosBlockInChunk(_posNeighbor))
                blocksNeighbor.Add(_direction, _chunk.Blocks[_posNeighbor.x, _posNeighbor.y, _posNeighbor.z]);
            else
            {
                if(_chunk.GetChunkNeighbor(new Vector2Int(_direction.x, _direction.z),out Chunk _neighbor))
                {
                    Vector3Int _pos = positionBlock - new Vector3Int((_chunk.ChunkSize - 1) * _direction.x, 0, (_chunk.ChunkSize - 1) * _direction.z); 
                    Block _blockNeighbor = _neighbor.Blocks[_pos.x, _pos.y, _pos.z];
                    blocksNeighbor.Add(_direction, _blockNeighbor);
                }
            }
        }
    }
    public bool AddNewFace(Vector3Int _direction, Face _face)
    {
        if (!facePerDirection.Keys.Contains(_direction))
        {
            facePerDirection.Add(_direction, _face);
            return false;
        }
        else
        {
            facePerDirection[_direction] = _face;
            return true;
        }
    }
    public static bool operator !(Block _block) => _block == null;
    public static implicit operator bool(Block _block) => _block != null;
}
public class Face
{
    public int[] triangles = new int[6];
    public Vector3[] vertices = new Vector3[4];
    public Face(Vector3[] _vertices,int[] _triangles)
    {
        triangles = _triangles;
        vertices = _vertices;
    }
}

[Serializable]
public struct ChunkParam
{
    public int chunkSize; 
    public int chunkHeight;
    public int octaves;
    public float sizeBlock; 
    public float noiseScale;

    public ChunkParam(int _chunkSize,int _chunkHeight,float _noiseScale,float _sizeBlock)
    {
        chunkSize = _chunkSize;
        noiseScale = _noiseScale; 
        chunkHeight = _chunkHeight;
        sizeBlock = _sizeBlock;
        octaves = 2;
    }
}


[RequireComponent(typeof(MeshCollider),typeof(MeshFilter),typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    Block[,,] blocks;
    public static List<Vector2Int> direction2D = new List<Vector2Int>()
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.right,
        Vector2Int.left,
    };

    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] List<Vector2> uvs = new List<Vector2>();
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<int> triangles = new List<int>();
    [SerializeField] bool debugBlock = false;
    [SerializeField] float sizeBlock = 1;
    [SerializeField] ChunkParam chunkParam;
    [SerializeField] Dictionary<Vector2Int, Chunk> chunkNeighbor = new Dictionary<Vector2Int, Chunk>();
    [SerializeField] Vector2Int indexChunk;
    [SerializeField] float frequency = 1;
    [SerializeField] float amplitude = 1;
    [SerializeField] float lacunarity = 2;
    [SerializeField] float persitence = 0.5f;
    List<Block> blockRender = new List<Block>();
    Mesh blockMesh;
    public int ChunkSize => chunkParam.chunkSize;
    public int ChunkHeight  => chunkParam.chunkHeight;
    public float SizeBlock => sizeBlock;
    public Block[,,] Blocks => blocks;
    public Vector3Int PositionClamp => new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
    public void Init(Vector2Int _indexChunk,ChunkParam _chunkParam)
    {
        indexChunk = _indexChunk;
        chunkParam = _chunkParam;
        blocks = new Block[chunkParam.chunkSize, chunkParam.chunkHeight, chunkParam.chunkSize];
        GenerateBlocks();
    }
    float CalcPerlin(int x,int z)
    {
        amplitude = 0.5f;
        frequency = chunkParam.noiseScale;
        float _value = 0;
        for (int i = 0; i < chunkParam.octaves; i++)
        {
            _value += Mathf.PerlinNoise((ChunkManager.noisePosX + transform.position.x + x) * frequency, (ChunkManager.noisePosY + transform.position.z + z) * frequency) * amplitude;
            amplitude *= persitence;
            frequency *= lacunarity;
        }
        return _value;
    }
    void GenerateBlocks()
    {
        for (int x = 0; x < chunkParam.chunkSize; x++)
        {
            for (int z = 0; z < chunkParam.chunkSize; z++)
            {
                //float noiseValue = Mathf.PerlinNoise((OldChunkManager.noisePosX + transform.position.x + x) * chunkParam.noiseScale, (OldChunkManager.noisePosY + transform.position.z + z) * chunkParam.noiseScale);
                float noiseValue = CalcPerlin(x,z);
                Debug.Log(noiseValue);
                int groundPosition = Mathf.RoundToInt(noiseValue * chunkParam.chunkHeight);
                BlockType voxelType = BlockType.Dirt;
                for (int y = 0; y < chunkParam.chunkHeight; y++)
                {
                    if (y > groundPosition)
                    {
                        voxelType = BlockType.Air;
                    }
                    else if (y == groundPosition)
                    {
                        voxelType = BlockType.Grass_Dirt;
                    }
                    Block _newBlock = new Block(new Vector3Int(x, y, z), voxelType, this);
                    blocks[x, y, z] = _newBlock;
                    if (voxelType == BlockType.Grass_Dirt)
                        blockRender.Add(_newBlock);
                }
            }
        }
    }
    public void FinishInitChunk()
    {
        SetNeighborChunk();
        RunThroughAllBlocks(SetNeighborBlock);
        int _count = blockRender.Count;
        for (int i = 0; i < _count; i++)
            SetAllFace(blockRender[i]);
        RecalculateMesh();
    }
    void SetNeighborBlock(Vector3Int _posBlock)
    {
        blocks[_posBlock.x, _posBlock.y, _posBlock.z].SetNeighbor(this);
    }
    public bool GetChunkNeighbor(Vector2Int _direction, out Chunk _chunkNeighbor)
    {
        if(chunkNeighbor.Keys.Contains(_direction))
        {
            _chunkNeighbor = chunkNeighbor[_direction];
            return true;
        }
        _chunkNeighbor = null;
        return false;
    }
    void SetNeighborChunk()
    {
        foreach (Vector2Int _direction in direction2D)
        {
            Vector2Int _posNeighbor = indexChunk + _direction;
            Chunk _chunk =  ChunkManager.Instance.GetChunk(_posNeighbor);
            if (!_chunk) continue;
            chunkNeighbor.Add(_direction, _chunk);
        }
    }
    void UpdateBlockToDestroy(Block _toDestroy)
    {
        _toDestroy.blockType = BlockType.Air;
        blockRender.Remove(_toDestroy);
        _toDestroy.facePerDirection.Clear();

        List<Chunk> _chunkToUpdate = new List<Chunk>();
        _chunkToUpdate.Add(this);
        foreach (var item in _toDestroy.blocksNeighbor)
        {
            AddFace(item.Value, _toDestroy, -item.Key);
            if (!_chunkToUpdate.Contains(item.Value.owner))
                _chunkToUpdate.Add(item.Value.owner);
        }
        for (int i = 0; i < _chunkToUpdate.Count; i++)
        {
            Debug.Log(_chunkToUpdate[i].name);
            _chunkToUpdate[i].UpdateVerticesAndTriangles();
            _chunkToUpdate[i].RecalculateMesh();
        }
    }
    public void DestroyMultiBlock(Vector3 _pos, Vector3 _normal, int _radius)
    {
        List<Block> _blockUpdate = new List<Block>();
        Vector3Int _posBlock = GetBlockInChunkFromWorldLocationAndNormal(_pos, _normal);
        for (int x = -_radius; x < _radius; ++x)
            for (int z = -_radius; z < _radius; ++z)
                for (int y = -_radius; y < _radius; ++y)
                {
                    Block _toDestroy = ChunkManager.Instance.GetBlockDataFromWorldPosition(PositionClamp + _posBlock + new Vector3Int(x, y, z));
                    if (!_toDestroy) continue;
                    _toDestroy.blockType = BlockType.Air;
                    blockRender.Remove(_toDestroy);
                    _toDestroy.facePerDirection.Clear();
                    _blockUpdate.Add(_toDestroy);
                }
        List<Chunk> _chunkToUpdate = new List<Chunk>();
        _chunkToUpdate.Add(this);
        int _count = _blockUpdate.Count;
        for (int i = 0; i < _count; i++)
        {
            Block _destroy = _blockUpdate[i]; 
            Dictionary<Vector3Int, Block> _neighbor = _destroy.blocksNeighbor;
            foreach (var item in _neighbor)
            {
                if (item.Value.blockType != BlockType.Air)
                {
                    AddFace(item.Value, _destroy, -item.Key);
                    if (!_chunkToUpdate.Contains(item.Value.owner))
                        _chunkToUpdate.Add(item.Value.owner);
                }
            }
        }
        for (int i = 0; i < _chunkToUpdate.Count; i++)
        {
            _chunkToUpdate[i].UpdateVerticesAndTriangles();
            _chunkToUpdate[i].RecalculateMesh();
        }
    }
    public void DestroyBlock(Vector3Int _posInChunk)
    {
        Block _block = blocks[_posInChunk.x, _posInChunk.y, _posInChunk.z];
        UpdateBlockToDestroy(_block);
        UpdateVerticesAndTriangles();
        RecalculateMesh();
    }
    public void DestroyBlock(Vector3 _pos, Vector3 _normal)
    {
        Vector3Int _posBlock = GetBlockInChunkFromWorldLocationAndNormal(_pos, _normal);
        if (!IsPosBlockInChunk(_posBlock)) return;
        DestroyBlock(_posBlock);
    }
    public void BuildFace(Chunk _chunk,Vector3 _facePos,Vector3 _directionUp, Vector3 _directionRight)
    {
        Vector3 _verticePosUpRight = _facePos + _directionUp + _directionRight;
        _chunk.vertices.Add(_verticePosUpRight);
        Vector3 _verticePosUpLeft = _facePos + _directionUp - _directionRight;
        _chunk.vertices.Add(_verticePosUpLeft);
        Vector3 _verticePosDownRight = _facePos - _directionUp + _directionRight;
        _chunk.vertices.Add(_verticePosDownRight);
        Vector3 _verticePosDownLeft = _facePos - _directionUp - _directionRight;
        _chunk.vertices.Add(_verticePosDownLeft);

        _chunk.triangles.Add(_chunk.vertices.Count - 4);
        _chunk.triangles.Add(_chunk.vertices.Count - 3);
        _chunk.triangles.Add(_chunk.vertices.Count - 2);
        _chunk.triangles.Add(_chunk.vertices.Count - 3);
        _chunk.triangles.Add(_chunk.vertices.Count - 1);
        _chunk.triangles.Add(_chunk.vertices.Count - 2);
    }
    void SetAllFace(Block _block)
    {
        if (_block.blockType == BlockType.Air) return;
        Dictionary<Vector3Int, Block> _blocksNeighbor = _block.blocksNeighbor;
        foreach (var item in _blocksNeighbor)
        {
            if (item.Value.blockType != BlockType.Air) continue;
            Vector3 _directionNeighbor = item.Key;
            Vector3 _directionFace = _directionNeighbor * (chunkParam.sizeBlock / 2);
            bool _upVector = Vector3Int.up == item.Key || Vector3Int.down == item.Key;
            Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3.right : Vector3.up) * _directionFace;
            Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * (chunkParam.sizeBlock / 2);
            Vector3 _posBlock = _block.positionBlock;
            Vector3 _facePos = _posBlock * chunkParam.sizeBlock + _directionFace;

            BuildFace(item.Value.owner, _facePos, _directionUp, _directionRight);

            Vector3[] _vertices = item.Value.owner.vertices.GetRange(item.Value.owner.vertices.Count - 4, 4).ToArray();
            int[] _triangles = item.Value.owner.triangles.GetRange(item.Value.owner.triangles.Count - 6, 6).ToArray();
            Face _face = new Face(_vertices, _triangles);
            _block.AddNewFace(item.Key,_face);

            if(!blockRender.Contains(_block))
                blockRender.Add(_block);
        }
    }
    void AddFace(Block _block, Block _blockDestroy, Vector3Int _direction)
    {
        if (_block.blockType == BlockType.Air || _blockDestroy.blockType != BlockType.Air) return;
        Vector3 _directionFace = new Vector3(_direction.x, _direction.y, _direction.z) * (chunkParam.sizeBlock / 2);
        bool _upVector = Vector3Int.up == _direction || Vector3Int.down == _direction;
        Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3.right : Vector3.up) * _directionFace;
        Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * (chunkParam.sizeBlock / 2);
        Vector3 _posBlock = _block.positionBlock;
        Vector3 _facePos = _posBlock * chunkParam.sizeBlock + _directionFace;

        BuildFace(_block.owner, _facePos ,_directionUp, _directionRight);

        Vector3[] _vertices = _block.owner.vertices.GetRange(_block.owner.vertices.Count - 4, 4).ToArray();
        int[] _triangles = _block.owner.triangles.GetRange(_block.owner.triangles.Count - 6, 6).ToArray();
        Face _face = new Face(_vertices, _triangles);
        _block.AddNewFace(_direction, _face);
            
        if (!_block.owner.blockRender.Contains(_block))
            _block.owner.blockRender.Add(_block);
    }

    void UpdateVerticesAndTriangles()
    {
        triangles.Clear();
        vertices.Clear();
        int _count = blockRender.Count;
        for (int i = 0; i < _count; i++)
            SetAllFace(blockRender[i]);
    }
    public bool IsPosBlockInChunk(Vector3Int _posBlock)
    {
        return (_posBlock.x < chunkParam.chunkSize && _posBlock.x >= 0) &&
               (_posBlock.y < chunkParam.chunkHeight && _posBlock.y >= 0) &&
               (_posBlock.z < chunkParam.chunkSize && _posBlock.z >= 0);
    }
    private void RunThroughAllBlocks(Action<Vector3Int> actionCallBlock)
    {
        for (int x = 0; x < chunkParam.chunkSize; x++)
            for (int z = 0; z < chunkParam.chunkSize; z++)
                for (int y = 0; y < chunkParam.chunkHeight; y++)
                    actionCallBlock?.Invoke(new Vector3Int(x, y, z));
    }
    void RecalculateMesh()
    {
        if(vertices.Count == 0 || triangles.Count == 0)
        {
            meshFilter.mesh = null;
            meshCollider.sharedMesh = null;
            return;
        } 
        blockMesh = new Mesh();
        blockMesh.vertices = vertices.ToArray();
        blockMesh.triangles = triangles.ToArray();
        blockMesh.SetUVs(0, uvs);
        blockMesh.RecalculateNormals();
        meshFilter.mesh = blockMesh;
        meshCollider.sharedMesh = blockMesh;
    }
    public Vector3Int GetBlockInChunkFromWorldLocationAndNormal(Vector3 _pos, Vector3 _normal)
    {
        float _sizeHalfBlock = chunkParam.sizeBlock / 2;
        Vector3 _posNormal = _pos - _normal * _sizeHalfBlock;
        Vector3 _posBlock = _posNormal - transform.position;
        Vector3Int _place = new Vector3Int(Mathf.FloorToInt((_posBlock.x + _sizeHalfBlock) / chunkParam.sizeBlock), Mathf.FloorToInt((_posBlock.y + _sizeHalfBlock) / chunkParam.sizeBlock), Mathf.FloorToInt((_posBlock.z + _sizeHalfBlock) / chunkParam.sizeBlock));
        return _place;
    }
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        if(debugBlock)
        {
            RunThroughAllBlocks((_blockCoord) => {
                Gizmos.color = BlockType.Dirt == blocks[_blockCoord.x, _blockCoord.y, _blockCoord.z].blockType ? Color.yellow : Color.white;
                Vector3 _posBlock = blocks[_blockCoord.x, _blockCoord.y, _blockCoord.z].positionBlock;
                Gizmos.DrawCube(transform.position + _posBlock * sizeBlock, Vector3.one * sizeBlock);
            });
        }
        Gizmos.color = Color.black;
        Gizmos.DrawWireMesh(blockMesh, transform.position);
    }
}
