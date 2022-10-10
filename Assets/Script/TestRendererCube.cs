using System;
using System.Collections.Concurrent;
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
    public BlockType blockType = BlockType.Dirt;
    public Dictionary<Vector3Int, Block> blocksNeighbor = new Dictionary<Vector3Int, Block>();
    public Dictionary<Vector3Int, Face> facePerDirection = new Dictionary<Vector3Int, Face>();
    public Block(Vector3Int _position,BlockType _blockType)
    {
        blockType = _blockType;
        positionBlock = _position;
    }
    public void SetNeighbor(TestRendererCube _renderCube)
    { 
        foreach (Vector3Int _direction in allDirection)
        {
            Vector3Int _posNeighbor = positionBlock + _direction;
            if (_renderCube.IsBlockInBlocks(_posNeighbor))
                blocksNeighbor.Add(_direction, _renderCube.blocks[_posNeighbor.x, _posNeighbor.y, _posNeighbor.z]);
        }
    }
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

[RequireComponent(typeof(MeshCollider),typeof(MeshFilter),typeof(MeshRenderer))]
public class TestRendererCube : MonoBehaviour
{
    Mesh blockMesh;
    public Block[,,] blocks;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    [SerializeField] bool debugBlock = false;
    Dictionary<Vector3, int> verticesIndex = new Dictionary<Vector3, int>(); 
    List<Block> blockRender = new List<Block>();
    [SerializeField] float sizeBlock = 1;
   public float SizeBlock => sizeBlock;
    private void Start() => Init();
    public void Init()
    {
        blocks = new Block[16, 70, 16];
        GenerateBlocks();
        RunThroughAllBlocks((_posBlock) => { blocks[_posBlock.x, _posBlock.y, _posBlock.z].SetNeighbor(this); });
        int _count = blockRender.Count;
        for (int i = 0; i < _count; i++)
            SetFace(blockRender[i]);
        RenderMesh();
    }
    void GenerateBlocks()
    {
        for (int x = 0; x < blocks.GetLength(0); x++)
        {
            for (int z = 0; z < blocks.GetLength(2); z++)
            {
                float noiseValue = Mathf.PerlinNoise((ChunkManager.noisePosX + transform.position.x + x) * 0.0005f, (ChunkManager.noisePosY + transform.position.z + z) * 0.0005f);
                int groundPosition = Mathf.RoundToInt(noiseValue * 100);
                BlockType voxelType = BlockType.Dirt;
                for (int y = 0; y < blocks.GetLength(1); y++)
                {
                    if (y > groundPosition)
                    {
                        voxelType = BlockType.Air;
                    }
                    else if (y == groundPosition)
                    {
                        voxelType = BlockType.Grass_Dirt;
                    }
                    blocks[x, y, z] = new Block(new Vector3Int(x, y, z), voxelType);
                    if (voxelType == BlockType.Grass_Dirt)
                        blockRender.Add(blocks[x, y, z]);
                }
            }
        }
    }
    void UpdateBlockToDestroy(Block _toDestroy)
    {
        _toDestroy.blockType = BlockType.Air;
        blockRender.Remove(_toDestroy);
        _toDestroy.facePerDirection.Clear();

        foreach (var item in _toDestroy.blocksNeighbor)
            AddFace(item.Value, _toDestroy, -item.Key);
    }
    public void DestroyMultiBlock(Vector3 _pos, Vector3 _normal, int _radius)
    {
        Vector3Int _posBlock = GetBlockInChunkFromWorldLocationAndNormal(_pos, _normal);
        for (int x = -_radius; x < _radius; ++x)
            for (int z = -_radius; z < _radius; ++z)
                for (int y = -_radius; y < _radius; ++y)
                {
                    Vector3Int _blockPosToDestroy = _posBlock + new Vector3Int(x, y, z);
                    if(IsBlockInBlocks(_blockPosToDestroy))
                        UpdateBlockToDestroy(blocks[_blockPosToDestroy.x, _blockPosToDestroy.y, _blockPosToDestroy.z]);
                }
        ResetVerticesAndTriangles();
        RenderMesh();
    }
    public void DestroyBlock(Vector3Int _posInChunk)
    {
        Block _block = blocks[_posInChunk.x, _posInChunk.y, _posInChunk.z];
        UpdateBlockToDestroy(_block);
        ResetVerticesAndTriangles();
        RenderMesh();
    }
    public void DestroyBlock(Vector3 _pos, Vector3 _normal)
    {
        Vector3Int _posBlock = GetBlockInChunkFromWorldLocationAndNormal(_pos, _normal);
        if (!IsBlockInBlocks(_posBlock)) return;
        DestroyBlock(_posBlock);
    }
    public Vector3Int GetBlockInChunkFromWorldLocationAndNormal(Vector3 _pos, Vector3 _normal)
    {
        Vector3 _posNormal = _pos - _normal * (sizeBlock/2);
        Vector3 _posBlock = _posNormal - transform.position;
        Vector3Int _place = new Vector3Int(Mathf.RoundToInt(_posBlock.x), Mathf.RoundToInt(_posBlock.y), Mathf.RoundToInt(_posBlock.z));
        return _place;
    }
    void SetFace(Block _block)
    {
        if (_block.blockType == BlockType.Air) return;
        foreach (var item in _block.blocksNeighbor)
        {
            if (item.Value.blockType == BlockType.Air)
            { 
                List<int> _trianglesTemp = new List<int>();
                List<Vector3> _verticesTemp = new List<Vector3>();
                Vector3 _direction = item.Key;
                Vector3 _directionFace = _direction * (sizeBlock / 2);
                bool _upVector = Vector3Int.up == item.Key || Vector3Int.down == item.Key;
                Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3.right : Vector3.up) * _directionFace;
                Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * (sizeBlock / 2);
                Vector3 _posBlock = _block.positionBlock;
                Vector3 _facePos = _posBlock * sizeBlock + _directionFace;

                Vector3 _verticePosUpRight = _facePos + _directionUp + _directionRight;
                bool _succeedToAddVertice = AddVerticeToBlock(_verticePosUpRight);
                _verticesTemp.Add(_verticePosUpRight);

                Vector3 _verticePosUpLeft = _facePos + _directionUp - _directionRight;
                _succeedToAddVertice = AddVerticeToBlock(_verticePosUpLeft);
                _verticesTemp.Add(_verticePosUpLeft);

                Vector3 _verticePosDownRight = _facePos - _directionUp + _directionRight;
                _succeedToAddVertice = AddVerticeToBlock(_verticePosDownRight);
                _verticesTemp.Add(_verticePosDownRight);

                Vector3 _verticePosDownLeft = _facePos - _directionUp - _directionRight;
                _succeedToAddVertice = AddVerticeToBlock(_verticePosDownLeft);
                _verticesTemp.Add(_verticePosDownLeft);

                triangles.Add(verticesIndex[_verticePosUpRight]);
                triangles.Add(verticesIndex[_verticePosUpLeft]);
                triangles.Add(verticesIndex[_verticePosDownRight]);

                triangles.Add(verticesIndex[_verticePosUpLeft]);
                triangles.Add(verticesIndex[_verticePosDownLeft]);
                triangles.Add(verticesIndex[_verticePosDownRight]);

                _trianglesTemp.Add(verticesIndex[_verticePosUpRight]);
                _trianglesTemp.Add(verticesIndex[_verticePosUpLeft]);
                _trianglesTemp.Add(verticesIndex[_verticePosDownRight]);

                _trianglesTemp.Add(verticesIndex[_verticePosUpLeft]);
                _trianglesTemp.Add(verticesIndex[_verticePosDownLeft]);
                _trianglesTemp.Add(verticesIndex[_verticePosDownRight]);
                _block.facePerDirection.Add(item.Key, new Face(_verticesTemp.ToArray(), _trianglesTemp.ToArray()));
                if(!blockRender.Contains(_block))
                    blockRender.Add(_block);
            }
        }
    }
    void AddFace(Block _block, Block _blockDestroy, Vector3Int _direction)
    {
        if (_block.blockType == BlockType.Air || _blockDestroy.blockType != BlockType.Air) return;
        List<int> _trianglesTemp = new List<int>();
        List<Vector3> _verticesTemp = new List<Vector3>();
        Vector3 _directionFace = new Vector3(_direction.x, _direction.y, _direction.z) * (sizeBlock / 2);
        bool _upVector = Vector3Int.up == _direction || Vector3Int.down == _direction;
        Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3.right : Vector3.up) * _directionFace;
        Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * (sizeBlock / 2);
        Vector3 _posBlock = _block.positionBlock;
        Vector3 _facePos = _posBlock * sizeBlock + _directionFace;

        Vector3 _verticePosUpRight = _facePos + _directionUp + _directionRight;
        bool _succeedToAddVertice = AddVerticeToBlock(_verticePosUpRight);
        _verticesTemp.Add(_verticePosUpRight);

        Vector3 _verticePosUpLeft = _facePos + _directionUp - _directionRight;
        _succeedToAddVertice = AddVerticeToBlock(_verticePosUpLeft);
        _verticesTemp.Add(_verticePosUpLeft);

        Vector3 _verticePosDownRight = _facePos - _directionUp + _directionRight;
        _succeedToAddVertice = AddVerticeToBlock(_verticePosDownRight);
        _verticesTemp.Add(_verticePosDownRight);

        Vector3 _verticePosDownLeft = _facePos - _directionUp - _directionRight;
        _succeedToAddVertice = AddVerticeToBlock(_verticePosDownLeft);
        _verticesTemp.Add(_verticePosDownLeft);

        _trianglesTemp.Add(verticesIndex[_verticePosUpRight]);
        _trianglesTemp.Add(verticesIndex[_verticePosUpLeft]);
        _trianglesTemp.Add(verticesIndex[_verticePosDownRight]);

        _trianglesTemp.Add(verticesIndex[_verticePosUpLeft]);
        _trianglesTemp.Add(verticesIndex[_verticePosDownLeft]);
        _trianglesTemp.Add(verticesIndex[_verticePosDownRight]);

        if(!_block.facePerDirection.Keys.Contains(_direction))
            _block.facePerDirection.Add(_direction, new Face(_verticesTemp.ToArray(), _trianglesTemp.ToArray()));
        else
            _block.facePerDirection[_direction] = new Face(_verticesTemp.ToArray(), _trianglesTemp.ToArray());
            
        if (!blockRender.Contains(_block))
            blockRender.Add(_block);
    }
    void ResetVerticesAndTriangles()
    {
        triangles.Clear();
        int _countBlockRender = blockRender.Count;
        for (int i = 0; i < _countBlockRender; i++)
        {
            Dictionary<Vector3Int, Face>.ValueCollection _faces = blockRender[i].facePerDirection.Values; 
            foreach (var item in _faces)
                triangles.AddRange(item.triangles);
        }
    }
    public bool AddVerticeToBlock(Vector3 _verticePos)
    {
        bool _succeedToAddVertice = verticesIndex.TryAdd(_verticePos, vertices.Count);
        if (_succeedToAddVertice)
            vertices.Add(_verticePos);
        return _succeedToAddVertice;
    }
    public bool IsBlockInBlocks(Vector3Int _pos)
    {
        return (_pos.x < blocks.GetLength(0) && _pos.x >= 0) &&
               (_pos.y < blocks.GetLength(1) && _pos.y >= 0) &&
               (_pos.z < blocks.GetLength(2) && _pos.z >= 0);
    }
    private void RunThroughAllBlocks(Action<Vector3Int> actionCallBlock)
    {
        for (int x = 0; x < blocks.GetLength(0); x++)
        {
            for (int z = 0; z < blocks.GetLength(2); z++)
            {
                for (int y = 0; y < blocks.GetLength(1); y++)
                {
                    actionCallBlock?.Invoke(new Vector3Int(x, y, z));
                }
            }
        }
    }
    void RenderMesh()
    {
        if(vertices.Count == 0 || triangles.Count == 0)
        {
            meshFilter.mesh = null;
            meshCollider.sharedMesh = null;
            return;
        }
        if(!blockMesh)
            blockMesh = new Mesh();
        blockMesh.vertices= vertices.ToArray();
        blockMesh.triangles = triangles.ToArray();
        blockMesh.SetUVs(0, uvs);
        blockMesh.RecalculateNormals();
        meshFilter.mesh = blockMesh;
        meshCollider.sharedMesh = blockMesh;
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
        Gizmos.color = Color.red;
        for (int i = 0; i < vertices.Count; i++)
        {
            Gizmos.DrawCube(transform.position + vertices[i],Vector3.one * 0.03f);
        }
        Gizmos.color = Color.black;
        Gizmos.DrawWireMesh(blockMesh, transform.position);
    }
}
