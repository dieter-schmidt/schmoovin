using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;

namespace NeoFPSEditor
{
    public abstract class BasePoolInfoEditor : Editor
    {
        private ReorderableList m_PoolInfoList = null;

        protected abstract SerializedProperty GetPoolInfoArrayProperty();

        [SerializeField] private int m_NewPoolSize = 20;

        protected virtual void OnEnable()
        {
            m_PoolInfoList = new ReorderableList(
                serializedObject,
                GetPoolInfoArrayProperty(),
                true, true, false, true
            );

            m_PoolInfoList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Pools");
            };

            m_PoolInfoList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                // Shift down by 2 (compensate for single line height)
                rect.y += 2;

                // Get the element
                var element = m_PoolInfoList.serializedProperty.GetArrayElementAtIndex(index);

                // Get the rects
                var protoRect = new Rect(rect.x, rect.y, rect.width - 55, EditorGUIUtility.singleLineHeight);
                var countRect = new Rect(rect.x + rect.width - 50, rect.y, 50, EditorGUIUtility.singleLineHeight);

                // Draw fields - pass GUIContent.none to each so they are drawn without labels
                EditorGUI.PropertyField(protoRect, element.FindPropertyRelative("prototype"), GUIContent.none);
                EditorGUI.PropertyField(countRect, element.FindPropertyRelative("count"), GUIContent.none);
            };
        }

        void AddObjectToPool(Object dropObject)
        {
            var prop = GetPoolInfoArrayProperty();
            
            // Check if it's already in use
            bool found = false;
            for (int i = 0; i < prop.arraySize; ++i)
            {
                var prototype = prop.GetArrayElementAtIndex(i).FindPropertyRelative("prototype");
                if (prototype.objectReferenceValue == dropObject)
                {
                    found = true;
                    break;
                }
            }

            // Add if not
            if (!found)
            {
                int index = prop.arraySize++;
                var pool = prop.GetArrayElementAtIndex(index);
                pool.FindPropertyRelative("prototype").objectReferenceValue = dropObject;
                pool.FindPropertyRelative("count").intValue = m_NewPoolSize;
            }
        }

        protected virtual void DoLayoutPoolInfo()
        {
            // Add prefab from drop field
            var dropObject = EditorGUILayout.ObjectField("Add Prefab", null, typeof(PooledObject), false);
            if (dropObject != null)
                AddObjectToPool(dropObject);

            // Add multiple prefabs from folder drop field
            var dropFolder = EditorGUILayout.ObjectField("Add Folder", null, typeof(DefaultAsset), false);
            if (dropFolder != null)
            {
                // Get path
                string folderPath = AssetDatabase.GetAssetPath(dropFolder);

                // Add prefabs from folder
                var guids = AssetDatabase.FindAssets("t:GameObject", new string[] { folderPath });
                for (int i = 0; i < guids.Length; ++i)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[i]));
                    if (obj != null)
                    {
                        var cast = obj.GetComponent<PooledObject>();
                        if (cast != null)
                            AddObjectToPool(cast);
                    }
                }
            }

            m_NewPoolSize = EditorGUILayout.IntField("New Pool Size", m_NewPoolSize);

            if (GUILayout.Button("Clear Pools"))
            {
                GetPoolInfoArrayProperty().arraySize = 0;
            }

            EditorGUILayout.Space();
            m_PoolInfoList.DoLayoutList();
            EditorGUILayout.Space();
        }
    }
}