using UnityEngine;
using NeoFPS;
using UnityEditor;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(SpawnPoint))]
    public class SpawnPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var group = serializedObject.FindProperty("group");
            EditorGUILayout.PropertyField(group);
            if (group.objectReferenceValue == null)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RegisterOnAwake"));
            else
            {
                NeoFpsEditorGUI.MiniInfo("Registration is controlled by an OrderedSpawnPointGroup component");
                GUI.enabled = false;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RegisterOnAwake"));
                GUI.enabled = true;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReuseDelay"));

            var overlapTest = serializedObject.FindProperty("m_OverlapTest");
            EditorGUILayout.PropertyField(overlapTest);
            if (overlapTest.enumValueIndex != 2)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BoundsHeight"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BoundsHorizontal"));
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReorientGravity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnSpawn"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}