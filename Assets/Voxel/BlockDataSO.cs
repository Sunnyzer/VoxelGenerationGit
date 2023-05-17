using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockData",menuName = "BlockData",order = 0)]
public class BlockDataSO : ScriptableObject
{
    public float textureSizeX, textureSizeY;
    public List<TextureData> textureDataList;

}
[Serializable]
public enum EDirection
{
    Side,
    Up,
    Down,
}

[Serializable]
public class TextureData
{
    public BlockType blockType;
    public Vector2Int up, down, side;
    public bool isSolid = true;
    public bool generatesCollide = true;
    public Vector2Int this[EDirection _direction]
    {
        get
        {
            switch (_direction)
            {
                case EDirection.Side:
                    return side;
                case EDirection.Up:
                    return up;
                case EDirection.Down:
                    return down;
            }
            return side;
        }
    }
}