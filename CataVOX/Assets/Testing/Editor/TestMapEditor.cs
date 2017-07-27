using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestMap))]
public class TestMapEditor : Editor
{
    SerializedProperty tilesFolder;
    SerializedProperty enableGrid;
    SerializedProperty scale;
    SerializedProperty terrainMaterial;

    void OnEnable()
    {
        tilesFolder = serializedObject.FindProperty("tilesFolder");
        enableGrid = serializedObject.FindProperty("enableGrid");
        scale = serializedObject.FindProperty("scale");
        terrainMaterial = serializedObject.FindProperty("terrainMaterial");
    }

    public override void OnInspectorGUI()
    {
        var obj = (TestMap)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(tilesFolder);
        EditorGUILayout.PropertyField(scale);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(enableGrid);
        if (EditorGUI.EndChangeCheck())
        {
            obj.enableGrid = enableGrid.boolValue;
            obj.UpdateGrid();
        }
        EditorGUILayout.PropertyField(terrainMaterial);
        serializedObject.ApplyModifiedProperties();
    }
}