using System;

[Serializable]
public struct WorldParam
{
    public int chunkSize;
    public int chunkHeight;
    public int chunkAmount;  
    public int octaves;
    public float sizeBlock;
    public float amplitude;
    public float frequence;
    public float lacunarity;
    public float persistence;
}
