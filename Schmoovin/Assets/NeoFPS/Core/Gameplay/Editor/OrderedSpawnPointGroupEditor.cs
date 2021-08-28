using NeoFPS;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(OrderedSpawnPointGroup), true)]
    public class OrderedSpawnPointGroupEditor : Editor
    {
        private ReorderableList m_SpawnList = null;

        void CheckSpawnList()
        {
            if (m_SpawnList == null)
            {
                m_SpawnList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_SpawnPoints"), true, false, false, true);
                m_SpawnList.onRemoveCallback += OnSpawnPointRemoved;
                m_SpawnList.drawElementCallback += DrawSpawnPointElement;
            }
        }

        private void DrawSpawnPointElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height -= 4;
            rect.y += 1;
            var prop = m_SpawnList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.ObjectField(rect, prop.objectReferenceValue, typeof(SpawnPoint), true);
        }

        private void OnSpawnPointRemoved(ReorderableList list)
        {
            // Reset group property on spawn point
            var prop = list.serializedProperty.GetArrayElementAtIndex(list.index);
            if (prop.objectReferenceValue != null)
            {
                var spawnSO = new SerializedObject(prop.objectReferenceValue);
                var group = spawnSO.FindProperty("group");
                if (group.objectReferenceValue != null && group.objectReferenceValue == target)
                {
                    group.objectReferenceValue = null;
                    spawnSO.ApplyModifiedProperties();
                }
            }

            SerializedArrayUtility.RemoveAt(list.serializedProperty, list.index);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            CheckSpawnList();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RegisterOnAwake"));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Spawn Points", EditorStyles.boldLabel);

            var obj = EditorGUILayout.ObjectField("Add Spawn Point:", null, typeof(SpawnPoint), true);
            if (obj != null)
            {
                // Check group property on spawn point
                var spawnSO = new SerializedObject(obj);
                var group = spawnSO.FindProperty("group");
                if (group.objectReferenceValue != null && group.objectReferenceValue != target)
                    Debug.LogError("Can't add spawn point to more than one group");
                else
                {
                    SerializedArrayUtility.Add(m_SpawnList.serializedProperty, obj, false);
                    serializedObject.ApplyModifiedProperties();
                    // Set group property and add to array
                    group.objectReferenceValue = target;
                    spawnSO.ApplyModifiedProperties();
                }
            }

            m_SpawnList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}