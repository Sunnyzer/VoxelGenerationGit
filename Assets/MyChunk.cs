using System.Collections.Generic;
using UnityEngine;

public class MyChunk : MonoBehaviour
{
    int chunkHeight = 30;
    int chunkSize = 8;
    float noiseScale = 0.03f;
    [SerializeField] int waterThreshold = 20;
    [SerializeField] BlockType[] blocks;
    int countBlocks = 0;
    public void Init(float _noiseScale, int _chunkSize,int _chunckHeight)
    {
        noiseScale = _noiseScale;
        chunkHeight = _chunckHeight;
        chunkSize = _chunkSize;
        GenerateVoxels();
    }
    private void GenerateVoxels()
    {
        countBlocks = chunkSize * chunkSize * chunkHeight;
        blocks = new BlockType[countBlocks];
        for (int y = 0; y < chunkHeight; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float noiseValue = Mathf.PerlinNoise((transform.position.x + x) * noiseScale, (transform.position.z + z) * noiseScale);
                    int groundPosition = Mathf.RoundToInt(noiseValue * chunkHeight);
                    BlockType voxelType = BlockType.Dirt;
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
                    int index = GetIndexFromPosition(x, y, z);
                    blocks[index] = voxelType;
                }
            }
        }
    }
    public int GetIndexFromPosition(int x,int y,int z)
    {
        return x + y * chunkSize + chunkSize * chunkHeight * z;
    }
    public Vector3 GetPositionFromIndex(int _index) 
    {
        int chunkHeightPas = chunkSize * chunkHeight;
        int z = _index / chunkHeightPas;
        _index = _index - chunkHeightPas * z;
        int y = _index / chunkSize;
        _index = _index - chunkSize * y;
        int x = _index;
        return new Vector3(x,y,z);
    }    

    private void OnDrawGizmos()
    {
        for (int i = 0; i < countBlocks; i++)
        {
            switch (blocks[i])
            {
                case BlockType.Nothing:
                    continue;
                case BlockType.Air:
                    continue;
                case BlockType.Grass_Dirt:
                    Gizmos.color = Color.green;
                    break;
                case BlockType.Dirt:
                    Gizmos.color = Color.yellow;
                    break;
                case BlockType.Water:
                    Gizmos.color = Color.blue - new Color(0,0,0,0f);
                    break;
            }
            bool test = true;
            bool test1 = true;
            bool test2 = true;
            bool test3 = true;
            bool test4 = true;
            bool test5 = true;
            if (i + 1 < countBlocks && blocks[i + 1] != BlockType.Air)
                test = false;
            else
                test = i + 1 < countBlocks;
            if (i - 1 > 0 && blocks[i - 1] != BlockType.Air)
                test1 = false;
            else
                test1 = i - 1 > 0;
            if (i + chunkSize < countBlocks && blocks[i + chunkSize] != BlockType.Air)
                test2 = false;
            else
                test2 = i + chunkSize < countBlocks;
            if (i - chunkSize > 0 && blocks[i - chunkSize] != BlockType.Air)
                test3 = false;
            else
                test3 = i - chunkSize > 0;
            if (i + chunkHeight * chunkSize < countBlocks && blocks[i + chunkHeight * chunkSize] != BlockType.Air)
                test4 = false;
            else
                test4 = i + chunkHeight * chunkSize < countBlocks;
            if (i - chunkHeight * chunkSize > 0 && blocks[i - chunkHeight * chunkSize] != BlockType.Air)
                test5 = false;
            else
                test5 = i - chunkHeight * chunkSize > 0;
            if (test || test1 || test2 || test3 || test4 || test5)
                Gizmos.DrawCube(transform.position + GetPositionFromIndex(i), new Vector3(1,1,1));
        }
    }
}
