using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MeshBlock
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
    public Dictionary<Vector3Int, MeshBlock> blocksNeighbor = new Dictionary<Vector3Int, MeshBlock>();
    public Dictionary<Vector3Int, FaceBlock> facePerDirection = new Dictionary<Vector3Int, FaceBlock>();
    public MeshBlock(Vector3Int _position, BlockType _blockType)
    {
        blockType = _blockType;
        positionBlock = _position;
    }
    public void SetNeighbor(MeshChunk _renderCube)
    {
        foreach (Vector3Int _direction in allDirection)
        {
            Vector3Int _posNeighbor = positionBlock + _direction;
            if (_renderCube.IsBlockInBlocks(_posNeighbor))
                blocksNeighbor.Add(_direction, _renderCube.blocks[_posNeighbor.x, _posNeighbor.y, _posNeighbor.z]);
        }
    }
}
public class FaceBlock
{
    public int[] triangles = new int[6];
    public Vector3[] vertices = new Vector3[4];
    public FaceBlock(Vector3[] _vertices, int[] _triangles)
    {
        triangles = _triangles;
        vertices = _vertices;
    }
}

[RequireComponent(typeof(MeshCollider), typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshChunk : MonoBehaviour
{
    Mesh blockMesh;
    public MeshBlock[,,] blocks;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    [SerializeField] bool debugBlock = false;
    List<MeshBlock> blockRender = new List<MeshBlock>();
    private void Start() => Init();
    public void Init()
    {
        blocks = new MeshBlock[16, 100, 16];
        GenerateBlocks();
        RunThroughAllBlocks((_posBlock) => { blocks[_posBlock.x, _posBlock.y, _posBlock.z].SetNeighbor(this); });
        for (int i = 0; i < blockRender.Count; i++)
            SetFace(blockRender[i]);
        RenderMesh();
    }
    void GenerateBlocks()
    {
        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                float noiseValue = Mathf.PerlinNoise((ChunkManager.noisePosX + transform.position.x + x) * 0.0005f, (ChunkManager.noisePosY + transform.position.z + z) * 0.0005f);
                int groundPosition = Mathf.RoundToInt(noiseValue * 100);
                BlockType voxelType = BlockType.Dirt;
                for (int y = 0; y < 100; y++)
                {
                    if (y > groundPosition)
                    {
                        voxelType = BlockType.Air;
                    }
                    else if (y == groundPosition)
                    {
                        voxelType = BlockType.Grass_Dirt;
                    }
                    blocks[x, y, z] = new MeshBlock(new Vector3Int(x, y, z), voxelType);
                    if (voxelType == BlockType.Grass_Dirt)
                        blockRender.Add(blocks[x, y, z]);
                }
            }
        }
    }
    void UpdateBlockToDestroy(MeshBlock _toDestroy)
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
                    if (IsBlockInBlocks(_blockPosToDestroy))
                        UpdateBlockToDestroy(blocks[_blockPosToDestroy.x, _blockPosToDestroy.y, _blockPosToDestroy.z]);
                }
        ResetVerticesAndTriangles();
        RenderMesh();
    }
    public void DestroyBlock(Vector3Int _posInChunk)
    {
        MeshBlock _block = blocks[_posInChunk.x, _posInChunk.y, _posInChunk.z];
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
        Vector3 _posNormal = _pos - _normal * 0.5f;
        Vector3Int _posBlockInBlock = new Vector3Int(Mathf.RoundToInt(_posNormal.x), Mathf.RoundToInt(_posNormal.y), Mathf.RoundToInt(_posNormal.z));
        Vector3 _posBlock = _posBlockInBlock - transform.position;
        return new Vector3Int(Mathf.RoundToInt(_posBlock.x), Mathf.RoundToInt(_posBlock.y), Mathf.RoundToInt(_posBlock.z));
    }
    void SetFace(MeshBlock _block)
    {
        if (_block.blockType == BlockType.Air) return;
        foreach (var item in _block.blocksNeighbor)
        {
            if (item.Value.blockType == BlockType.Air)
            {
                List<int> _trianglesTemp = new List<int>();
                List<Vector3> _verticesTemp = new List<Vector3>();
                Vector3 _direction = item.Key;
                Vector3 _directionFace = _direction * 0.5f;
                bool _upVector = Vector3Int.up == item.Key || Vector3Int.down == item.Key;
                Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3.right : Vector3.up) * _directionFace;
                Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * 0.5f;
                Vector3 _facePos = _block.positionBlock + _directionFace;

                Vector3 _verticePosUpRight = _facePos + _directionUp + _directionRight;
                vertices.Add(_verticePosUpRight);
                _verticesTemp.Add(_verticePosUpRight);

                Vector3 _verticePosUpLeft = _facePos + _directionUp - _directionRight;
                vertices.Add(_verticePosUpLeft);
                _verticesTemp.Add(_verticePosUpLeft);

                Vector3 _verticePosDownRight = _facePos - _directionUp + _directionRight;
                vertices.Add(_verticePosDownRight);
                _verticesTemp.Add(_verticePosDownRight);

                Vector3 _verticePosDownLeft = _facePos - _directionUp - _directionRight;
                vertices.Add(_verticePosDownLeft);
                _verticesTemp.Add(_verticePosDownLeft);

                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);

                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 1);
                triangles.Add(vertices.Count - 2);

                _trianglesTemp.Add(vertices.Count - 4);
                _trianglesTemp.Add(vertices.Count - 3);
                _trianglesTemp.Add(vertices.Count - 2);

                _trianglesTemp.Add(vertices.Count - 3);
                _trianglesTemp.Add(vertices.Count - 1);
                _trianglesTemp.Add(vertices.Count - 2);

                _block.facePerDirection.Add(item.Key, new FaceBlock(_verticesTemp.ToArray(), _trianglesTemp.ToArray()));
                if (!blockRender.Contains(_block))
                    blockRender.Add(_block);
            }
        }
    }
    void AddFace(MeshBlock _block, MeshBlock _blockDestroy, Vector3Int _direction)
    {
        if (_block.blockType == BlockType.Air || _blockDestroy.blockType != BlockType.Air) return;
        List<int> _trianglesTemp = new List<int>();
        List<Vector3> _verticesTemp = new List<Vector3>();
        Vector3 _directionFace = new Vector3(_direction.x, _direction.y, _direction.z) * 0.5f;
        bool _upVector = Vector3Int.up == _direction || Vector3Int.down == _direction;
        Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3.right : Vector3.up) * _directionFace;
        Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * 0.5f;
        Vector3 _facePos = _block.positionBlock + _directionFace;

        Vector3 _verticePosUpRight = _facePos + _directionUp + _directionRight;
        vertices.Add(_verticePosUpRight);
        _verticesTemp.Add(_verticePosUpRight);

        Vector3 _verticePosUpLeft = _facePos + _directionUp - _directionRight;
        vertices.Add(_verticePosUpLeft);
        _verticesTemp.Add(_verticePosUpLeft);

        Vector3 _verticePosDownRight = _facePos - _directionUp + _directionRight;
        vertices.Add(_verticePosDownRight);
        _verticesTemp.Add(_verticePosDownRight);

        Vector3 _verticePosDownLeft = _facePos - _directionUp - _directionRight;
        vertices.Add(_verticePosDownLeft);
        _verticesTemp.Add(_verticePosDownLeft);

        _trianglesTemp.Add(vertices.Count - 4);
        _trianglesTemp.Add(vertices.Count - 3);
        _trianglesTemp.Add(vertices.Count - 2);

        _trianglesTemp.Add(vertices.Count - 3);
        _trianglesTemp.Add(vertices.Count - 1);
        _trianglesTemp.Add(vertices.Count - 2);

        if (!_block.facePerDirection.Keys.Contains(_direction))
            _block.facePerDirection.Add(_direction, new FaceBlock(_verticesTemp.ToArray(), _trianglesTemp.ToArray()));
        else
            _block.facePerDirection[_direction] = new FaceBlock(_verticesTemp.ToArray(), _trianglesTemp.ToArray());

        if (!blockRender.Contains(_block))
            blockRender.Add(_block);
    }
    void ResetVerticesAndTriangles()
    {
        triangles.Clear();
        //vertices.Clear();
        int _countBlockRender = blockRender.Count;
        for (int i = 0; i < _countBlockRender; i++)
        {
            Dictionary<Vector3Int, FaceBlock>.ValueCollection _faces = blockRender[i].facePerDirection.Values;
            foreach (var item in _faces)
            {
                triangles.AddRange(item.triangles);
                //vertices.AddRange(item.vertices);
            }
        }
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
        if (vertices.Count == 0 || triangles.Count == 0)
        {
            meshFilter.mesh = null;
            meshCollider.sharedMesh = null;
            return;
        }
        //if (!blockMesh)
            blockMesh = new Mesh();
        blockMesh.vertices = vertices.ToArray();
        blockMesh.triangles = triangles.ToArray();
        blockMesh.SetUVs(0, uvs);
        blockMesh.RecalculateNormals();
        meshFilter.mesh = blockMesh;
        meshCollider.sharedMesh = blockMesh;
    }
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        if (debugBlock)
        {
            RunThroughAllBlocks((_posBlock) => {
                Gizmos.color = BlockType.Dirt == blocks[_posBlock.x, _posBlock.y, _posBlock.z].blockType ? Color.yellow : Color.white;
                Gizmos.DrawCube(transform.position + blocks[_posBlock.x, _posBlock.y, _posBlock.z].positionBlock, Vector3Int.one);
            });
        }
        Gizmos.color = Color.red;
        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    Gizmos.DrawCube(transform.position + vertices[i], Vector3.one * 0.05f);
        //}
        Gizmos.color = Color.black;
        Gizmos.DrawWireMesh(blockMesh, transform.position);
    }
}
