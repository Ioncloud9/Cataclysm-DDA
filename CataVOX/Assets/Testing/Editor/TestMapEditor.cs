using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestMap))]
public class TestMapEditor : Editor
{
    SerializedProperty tilesFolder;
    SerializedProperty enableGrid;
    SerializedProperty removeEdges;
    SerializedProperty scale;
    SerializedProperty chunkSize;
    SerializedProperty chunkRadius;
    SerializedProperty terrainMaterial;
    SerializedProperty startingPoint;    

    void OnEnable()
    {
        tilesFolder = serializedObject.FindProperty("tilesFolder");
        enableGrid = serializedObject.FindProperty("enableGrid");
        removeEdges = serializedObject.FindProperty("removeEdges");
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

        EditorGUILayout.LabelField("Loaded world: " + TestGame.WorldName);
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
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(removeEdges);
        if (EditorGUI.EndChangeCheck())
        {
            obj.removeEdges = removeEdges.boolValue;
            obj.RebuildCache();
        }        
        EditorGUILayout.PropertyField(terrainMaterial);
        
        if (GUILayout.Button("Rebuild")) obj.Rebuild();
        //if (GUILayout.Button("Rebuild Cache")) obj.RebuildCache();

        serializedObject.ApplyModifiedProperties();
    }
}