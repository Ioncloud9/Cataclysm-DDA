using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestChunk))]
public class TestChunkEditor : Editor
{
    SerializedProperty start;
    SerializedProperty end;
    SerializedProperty needRebuild;

    void OnEnable()
    {
        start = serializedObject.FindProperty("start");
        end = serializedObject.FindProperty("end");
        needRebuild = serializedObject.FindProperty("needRebuild");
    }

    public override void OnInspectorGUI()
    {
        var obj = (TestChunk)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(start, true);
        EditorGUILayout.PropertyField(end, true);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(needRebuild);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Rebuild();
        }
        serializedObject.ApplyModifiedProperties();
    }
}