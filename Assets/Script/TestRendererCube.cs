using System;
using System.Collections.Generic;
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
    public List<Block> blocks = new List<Block>();
    public Dictionary<Vector3Int, Block> blocksNeighbor = new Dictionary<Vector3Int, Block>();
    public Dictionary<Vector3Int, Face> allFace = new Dictionary<Vector3Int, Face>();
    public Dictionary<Vector3, int> allVertices = new Dictionary<Vector3, int>();
    public Block(Vector3Int _position,BlockType _blockType)
    {
        blockType = _blockType;
        positionBlock = _position;
    }
    public void SetNeighbor(TestRendererCube _renderCube)
    {
        int index = 0;
        foreach (Vector3Int _direction in allDirection)
        {
            Vector3Int _posNeighbor = positionBlock + _direction;
            if (_renderCube.IsBlockInBlocks(_posNeighbor))
            {
                blocksNeighbor.Add(_direction, _renderCube.blocks[_posNeighbor.x, _posNeighbor.y, _posNeighbor.z]);
                blocks.Add(blocksNeighbor[_direction]);
            }
            index++;
        }
    }
    public bool AddVertices(Vector3 _verticePos, int verticeIndex)
    {
        if (!allVertices.ContainsKey(_verticePos))
        {
            allVertices.Add(_verticePos, verticeIndex);
            return true;
        }
        return false;
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
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] MeshFilter meshFilter;
    Mesh blockMesh;
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    public Block[,,] blocks;
    [SerializeField] bool debugBlock = false;
    [SerializeField] Block block;
    private void Start() => Init();
    public void Init()
    {
        blocks = new Block[1, 6, 1];
        RunThroughAllBlocks(GenerateBlocks);
        RunThroughAllBlocks((_posBlock) => { blocks[_posBlock.x, _posBlock.y, _posBlock.z].SetNeighbor(this); });
        RunThroughAllBlocks((_posBlock) => { SetFace(blocks[_posBlock.x, _posBlock.y, _posBlock.z]); });
        block = blocks[0, 2, 0];
        RenderMesh();
    }
    void GenerateBlocks(Vector3Int _posBlock)
    {
        if (_posBlock.y > 2)
            blocks[_posBlock.x, _posBlock.y, _posBlock.z] = new Block(_posBlock, BlockType.Air);
        else
            blocks[_posBlock.x, _posBlock.y, _posBlock.z] = new Block(_posBlock, BlockType.Dirt);
    }
    void SetFace(Block _block)
    {
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
                bool _succeedToAddVertice = AddVerticeToBlock(_verticePosUpRight, _block);
                _verticesTemp.Add(_verticePosUpRight);

                Vector3 _verticePosUpLeft = _facePos + _directionUp - _directionRight;
                _succeedToAddVertice = AddVerticeToBlock(_verticePosUpLeft, _block);
                _verticesTemp.Add(_verticePosUpLeft);

                Vector3 _verticePosDownRight = _facePos - _directionUp + _directionRight;
                _succeedToAddVertice = AddVerticeToBlock(_verticePosDownRight, _block);
                _verticesTemp.Add(_verticePosDownRight);

                Vector3 _verticePosDownLeft = _facePos - _directionUp - _directionRight;
                _succeedToAddVertice = AddVerticeToBlock(_verticePosDownLeft, _block);
                _verticesTemp.Add(_verticePosDownLeft);

                triangles.Add(_block.allVertices[_verticePosUpRight]);
                triangles.Add(_block.allVertices[_verticePosUpLeft]);
                triangles.Add(_block.allVertices[_verticePosDownRight]);

                triangles.Add(_block.allVertices[_verticePosUpLeft]);
                triangles.Add(_block.allVertices[_verticePosDownLeft]);
                triangles.Add(_block.allVertices[_verticePosDownRight]);

                _trianglesTemp.Add(_block.allVertices[_verticePosUpRight]);
                _trianglesTemp.Add(_block.allVertices[_verticePosUpLeft]);
                _trianglesTemp.Add(_block.allVertices[_verticePosDownRight]);

                _trianglesTemp.Add(_block.allVertices[_verticePosUpLeft]);
                _trianglesTemp.Add(_block.allVertices[_verticePosDownLeft]);
                _trianglesTemp.Add(_block.allVertices[_verticePosDownRight]);
                _block.allFace.Add(item.Key, new Face(_verticesTemp.ToArray(), _trianglesTemp.ToArray()));
            }
        }
    }
    public bool AddVerticeToBlock(Vector3 _verticePos,Block _block)
    {
        bool _succeedToAddVertice = _block.AddVertices(_verticePos, vertices.Count);
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
        blockMesh = new Mesh();
        blockMesh.vertices= vertices.ToArray();
        blockMesh.triangles = triangles.ToArray();
        blockMesh.RecalculateNormals();
        blockMesh.SetUVs(0, uvs);
        meshFilter.mesh = blockMesh;
        meshCollider.sharedMesh = blockMesh;
    }
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        if(debugBlock)
        {
            RunThroughAllBlocks((_posBlock) => {
                Gizmos.color = BlockType.Dirt == blocks[_posBlock.x, _posBlock.y, _posBlock.z].blockType ? Color.yellow : Color.white;
                Gizmos.DrawCube(transform.position + blocks[_posBlock.x, _posBlock.y, _posBlock.z].positionBlock, Vector3Int.one);
            });
        }
        Gizmos.color = Color.red;
        for (int i = 0; i < vertices.Count; i++)
        {
            Gizmos.DrawCube(transform.position + vertices[i],Vector3.one * 0.05f);
        }
    }
}
