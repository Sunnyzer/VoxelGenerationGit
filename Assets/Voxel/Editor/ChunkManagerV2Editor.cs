using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(ChunkManagerV2))]
public class ChunkManagerV2Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ChunkManagerV2 eTarget = (ChunkManagerV2)target;
        if (GUILayout.Button("Generate World"))
        {
            Clear(eTarget);
            eTarget.StartCoroutine(eTarget.GenerateVoxels());
        }
        if (GUILayout.Button("Clear Voxels"))
        {
            Clear(eTarget);
        }
    }
    public void Clear(ChunkManagerV2 eTarget)
    {
        eTarget.StopAllCoroutines();
        if (eTarget.Chunks != null)
        {
            for (int x = 0; x < eTarget.Chunks.GetLength(0); x++)
            {
                for (int z = 0; z < eTarget.Chunks.GetLength(1); z++)
                {
                    if(eTarget.Chunks[x, z])
                    DestroyImmediate(eTarget.Chunks[x, z].gameObject);
                }
            }
            Array.Clear(eTarget.Chunks, 0, eTarget.Chunks.Length);
        }
    }
}
