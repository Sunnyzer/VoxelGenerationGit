using System;
using System.Collections.Generic;
using UnityEngine;

public static class Direction
{
    public static List<Vector2Int> direction2D = new List<Vector2Int>()
    {
        Vector2Int.right,
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.down,
    };
    public static List<Vector3Int> allDirection = new List<Vector3Int>()
    {
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down
    };
    public static void RunThroughAllDirection2D(Action<Vector2Int> _action)
    {
        int _count = direction2D.Count;
        for (int i = 0; i < _count; i++)
            _action?.Invoke(direction2D[i]);
    }
}
