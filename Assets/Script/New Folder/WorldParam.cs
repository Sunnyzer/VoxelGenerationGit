using System;

[Serializable]
public struct WorldParam
{
    public int chunkSize;
    public int chunkHeight;
    public float sizeBlock;
    public float noiseScale;
    public int chunkAmount;  
    public int octaves;
    public int amplitude;
    public int frequence;
    public int lacunarity;
    public int persistence;
}
