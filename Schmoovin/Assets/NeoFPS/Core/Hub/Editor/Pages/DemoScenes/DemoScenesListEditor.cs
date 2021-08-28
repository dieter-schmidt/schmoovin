using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NeoFPSEditor.Hub.Pages
{
    [CustomEditor(typeof(DemoScenesList))]
    public class DemoScenesListEditor : Editor
    {
        ReorderableList m_List = null;

        private void OnEnable()
        {
            m_List = new ReorderableList(serializedObject, serializedObject.FindProperty("scenes"));
            m_List.drawHeaderCallback = DrawListHeader;
            m_List.drawElementCallback = DrawListElement;
            m_List.elementHeight = 112;
        }

        void DrawListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Demo Scenes");
        }

        void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            float h = EditorGUIUtility.singleLineHeight;
            float offset = h + EditorGUIUtility.standardVerticalSpacing;

            var prop = m_List.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;
            rect.height = h;
            EditorGUI.PropertyField(rect, prop.FindPropertyRelative("title"));
            rect.y += offset;
            EditorGUI.PropertyField(rect, prop.FindPropertyRelative("loadName"));
            rect.y += offset;
            EditorGUI.PropertyField(rect, prop.FindPropertyRelative("thumbnail"));
            rect.y += offset;
            rect.height += offset * 2;
            EditorGUI.PropertyField(rect, prop.FindPropertyRelative("description"));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("category"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"));
            m_List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
