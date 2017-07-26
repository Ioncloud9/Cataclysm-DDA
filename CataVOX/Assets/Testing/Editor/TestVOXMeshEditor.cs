using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestVOXMesh))]
public class TestVOXMeshEditor : Editor
{
    SerializedProperty voxPath;
    SerializedProperty enableGrid;
    SerializedProperty texture;

    void OnEnable()
    {
        voxPath = serializedObject.FindProperty("voxPath");
        enableGrid = serializedObject.FindProperty("enableGrid");
        texture = serializedObject.FindProperty("texture");
    }

    public override void OnInspectorGUI()
    {
        var obj = (TestVOXMesh)target;
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(voxPath);
        if (EditorGUI.EndChangeCheck())
        {
            obj.voxPath = voxPath.stringValue;
            obj.ReInit();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(enableGrid);
        if (EditorGUI.EndChangeCheck())
        {
            obj.enableGrid = enableGrid.boolValue;
            obj.UpdateGrid();
        }

        serializedObject.ApplyModifiedProperties();
    }
}