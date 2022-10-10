using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldOldChunk : MonoBehaviour
{
    int chunkHeight = 30;
    int chunkSize = 8;
    float noiseScale = 0.03f;
    Mesh chunkMesh;
    [SerializeField] int waterThreshold = 20;
    [SerializeField] BlockType[,,] blocks;
    [SerializeField] List<OldOldChunk> chunkNeighbor = new List<OldOldChunk>();
    [SerializeField] Vector3 posBlockDebug;
    [SerializeField] bool drawVerticesGizmo = false;
    [SerializeField] bool drawNeighBorGizmo = true;
    [SerializeField] Dictionary<Vector3, int> verticesId = new Dictionary<Vector3, int>();
    static public Vector3Int GetBlockPositionFromWorldPosition(Vector3 _position, Vector3 _direction)
    {
        Vector3 _posNoTruncate = _position - _direction * .4f;
        return new Vector3Int(Mathf.RoundToInt(_posNoTruncate.x), Mathf.RoundToInt(_posNoTruncate.y), Mathf.RoundToInt(_posNoTruncate.z)); ; 
    }
    public static List<Vector3Int> direction = new List<Vector3Int>()
    { 
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down,
    };
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    List<int> triangle = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    public Vector3Int GetPositionBlockFromWorldPosition(Vector3 _pos, Vector3 _normal)
    {
        Vector3 _posBlock = _pos - _normal * 0.5f;
        return new Vector3Int(Mathf.RoundToInt(_posBlock.x), Mathf.RoundToInt(_posBlock.y), Mathf.RoundToInt(_posBlock.z));
    }
    public Vector3Int GetBlockPositionInChunkFromWorldPostion(Vector3Int _pos)
    {
        Vector3Int _clampPos = new Vector3Int(_pos.x - Mathf.RoundToInt(transform.position.x), 0, _pos.z - Mathf.RoundToInt(transform.position.z));
        return new Vector3Int(_clampPos.x, _pos.y, _clampPos.z);
    }
    public void DestroyBlock(Vector3Int _pos)
    {
        blocks[_pos.x, _pos.y, _pos.z] = BlockType.Air;
        StartCoroutine(MakeMesh());
        for (int i = 0; i < chunkNeighbor.Count; i++)
        {
            StartCoroutine(chunkNeighbor[i].MakeMesh());
        }
    }
    public bool IsBlockInChunk(Vector3Int _pos)
    {
        return (_pos.x < blocks.GetLength(0) && _pos.x >= 0) && (_pos.y < blocks.GetLength(1) && _pos.y >= 0) && (_pos.z < blocks.GetLength(2) && _pos.z >= 0);
    }
    public IEnumerator Init(float _noiseScale, int _chunkSize,int _chunckHeight)
    {
        noiseScale = _noiseScale;
        chunkHeight = _chunckHeight;
        chunkSize = _chunkSize;
        GenerateVoxels();
        yield break;
    }
    private void GenerateVoxels()
    {
        blocks = new BlockType[chunkSize,chunkHeight,chunkSize];
        GenerateBlocksChunk();
        MakeMesh();
    }
    void GenerateBlocksChunk()
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                float noiseValue = Mathf.PerlinNoise((OldOldChunkManager.x + transform.position.x + x) * noiseScale, (OldOldChunkManager.y + transform.position.z + z) * noiseScale);
                int groundPosition = Mathf.RoundToInt(noiseValue * chunkHeight);
                BlockType voxelType = BlockType.Dirt;
                for (int y = 0; y < chunkHeight; y++)
                {
                    if (y > groundPosition)
                    {
                        if (y < waterThreshold)
                            voxelType = BlockType.Water;
                        else
                            voxelType = BlockType.Air;
                    }
                    else if (y == groundPosition)
                    {
                        voxelType = BlockType.Grass_Dirt;
                    }
                    blocks[x,y,z] = voxelType;
                }
            }
        }
    }
    public IEnumerator MakeMesh()
    {
        vertices.Clear();
        triangle.Clear();
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                for (int y = 0; y < chunkHeight; y++)
                {
                    Vector3Int _blockPos = new Vector3Int(x, y, z);
                    MakeCube(_blockPos);
                }
            }
        }
        RenderMesh();
        yield break;
    }
    void RenderMesh()
    {
        chunkMesh = new Mesh();
        chunkMesh.vertices = vertices.ToArray();
        chunkMesh.triangles = triangle.ToArray();
        chunkMesh.SetUVs(0, uvs);
        chunkMesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = chunkMesh;
        GetComponent<MeshCollider>().sharedMesh = chunkMesh;
    }
    void MakeCube(Vector3Int _blockPos)
    {
        Vector3Int[] directionToFill = IsBlockFill(_blockPos);
        if (directionToFill.Length != 0 && blocks[_blockPos.x, _blockPos.y, _blockPos.z] != BlockType.Air)
        {
            foreach (Vector3 _dir in directionToFill)
            {
                bool _upVector = Vector3.up == _dir || Vector3.down == _dir;
                Vector3 _directionFace = _dir * 0.5f;
                Vector3 _directionRight = Quaternion.AngleAxis(90, _upVector ? Vector3.right : Vector3.up) * _directionFace;
                Vector3 _directionUp = (_upVector ? Vector3.right : Vector3.up) * 0.5f;
                Vector3 _facePos = _blockPos + _directionFace;

                vertices.Add(_facePos + _directionUp + _directionRight);
                vertices.Add(_facePos + _directionUp - _directionRight);
                vertices.Add(_facePos - _directionUp + _directionRight);
                vertices.Add(_facePos - _directionUp - _directionRight);

                triangle.Add(vertices.Count - 4);
                triangle.Add(vertices.Count - 3);
                triangle.Add(vertices.Count - 2);

                triangle.Add(vertices.Count - 3);
                triangle.Add(vertices.Count - 1);
                triangle.Add(vertices.Count - 2);
            }
        }
    }
    bool IsPositionInChunk(Vector3Int _position)
    {
        if(_position.z < 0 || _position.z >= chunkSize)
            return false;
        if(_position.x < 0 || _position.x >= chunkSize)
            return false;
        if(_position.y < 0 || _position.y >= chunkHeight)
            return false;
        return true;
    }
    Vector3Int[] IsBlockFill(Vector3Int _pos)
    {
        List<Vector3Int> _directionFill = new List<Vector3Int>();
        foreach (Vector3Int _dir in direction)
        {
            Vector3Int _posDir = _pos + _dir;
            if (IsPositionInChunk(_posDir))
            {
                if (blocks[_posDir.x, _posDir.y, _posDir.z] == BlockType.Air || blocks[_posDir.x, _posDir.y, _posDir.z] == BlockType.Water)
                    _directionFill.Add(_dir);
            }
            else if (_dir != Vector3.up && _dir != Vector3.down)
            {
                Vector3 _posCube = transform.position / 16 + _dir;
                OldOldChunk _chunkNeighbor = OldOldChunkManager.Instance.GetChunk((int)_posCube.x, (int)_posCube.z);
                if (!_chunkNeighbor) continue;
                if(!chunkNeighbor.Contains(_chunkNeighbor))
                    chunkNeighbor.Add(_chunkNeighbor);
                Vector3Int _cal = _pos - new Vector3Int((chunkSize - 1) * _dir.x, 0, (chunkSize - 1) * _dir.z);
                if (_cal.x >= 0 && _cal.x < _chunkNeighbor.blocks.GetLength(0) && _cal.z >= 0 && _cal.z < _chunkNeighbor.blocks.GetLength(2))
                    if (_chunkNeighbor.blocks[_cal.x, _cal.y, _cal.z] == BlockType.Air)
                    {
                        _directionFill.Add(_dir);
                    }
            }
        }
        return _directionFill.ToArray();
    }
    private void OnDrawGizmosSelected()
    {
        if (!drawNeighBorGizmo) return;
        int _count = chunkNeighbor.Count;
        Gizmos.color = Color.red;
        Vector3 _offset = Vector3.up * 2;
        for (int i = 0; i < _count; i++)
        {
            OldOldChunk _chunk = chunkNeighbor[i];
            Gizmos.DrawMesh(_chunk.chunkMesh, 0, _chunk.transform.position + _offset);
        }
    }
    private void OnDrawGizmos()
    {
        if (!drawVerticesGizmo) return;
        int _count = vertices.Count;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < _count; i++)
            Gizmos.DrawCube(vertices[i], Vector3.one);
    }
}
