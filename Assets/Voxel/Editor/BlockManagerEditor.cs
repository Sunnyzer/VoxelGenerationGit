using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BlockManager))]
public class BlockManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Dico"))
        {
            ((BlockManager)target).Init();
        }
    }
}
