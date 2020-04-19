using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Building))]
public class BuildingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUI.enabled = Application.isPlaying;

        EditorGUILayout.Space();

        Building b = target as Building;
        if (GUILayout.Button("Build New Module"))
            b.BuildNewModule();

        if (GUILayout.Button("Clear Building"))
            b.ClearBuildingModules();
    }
}