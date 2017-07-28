using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestMap))]
public class TestMapEditor : Editor
{
    SerializedProperty tilesFolder;
    SerializedProperty enableGrid;
    SerializedProperty scale;
    SerializedProperty chunkSize;
    SerializedProperty chunkRadius;
    SerializedProperty terrainMaterial;
    SerializedProperty startingPoint;    

    void OnEnable()
    {
        tilesFolder = serializedObject.FindProperty("tilesFolder");
        enableGrid = serializedObject.FindProperty("enableGrid");
        scale = serializedObject.FindProperty("scale");
        terrainMaterial = serializedObject.FindProperty("terrainMaterial");
        chunkSize = serializedObject.FindProperty("chunkSize");
        chunkRadius = serializedObject.FindProperty("chunkRadius");
        startingPoint = serializedObject.FindProperty("startingPoint");
    }

    public override void OnInspectorGUI()
    {
        var obj = (TestMap)target;
        serializedObject.Update();

        EditorGUILayout.LabelField("Loaded world: " + TestMap.WorldName);
        EditorGUILayout.PropertyField(tilesFolder);
        EditorGUILayout.PropertyField(scale);
        EditorGUILayout.PropertyField(chunkSize);
        EditorGUILayout.PropertyField(chunkRadius);
        EditorGUILayout.PropertyField(startingPoint, true);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(enableGrid);
        if (EditorGUI.EndChangeCheck())
        {
            obj.enableGrid = enableGrid.boolValue;
            obj.UpdateGrid();
        }
        EditorGUILayout.PropertyField(terrainMaterial);
        if (GUILayout.Button("Rebuild"))
        {
            obj.Rebuild();
        }

        serializedObject.ApplyModifiedProperties();
    }
}