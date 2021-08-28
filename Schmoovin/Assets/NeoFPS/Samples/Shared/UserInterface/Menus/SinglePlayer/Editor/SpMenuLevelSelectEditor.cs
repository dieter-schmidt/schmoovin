using NeoFPS.Samples;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NeoFPSEditor.Samples
{
    [CustomEditor(typeof(SpMenuLevelSelect), true)]
    public class SpMenuLevelSelectEditor : Editor
    {
        ReorderableList m_LevelsList = null;

        private void OnEnable()
        {
            m_LevelsList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("m_Levels"),
                true, true, true, true
                );
            m_LevelsList.drawHeaderCallback = DrawListHeader;
            m_LevelsList.drawElementCallback = DrawListElement;
            m_LevelsList.elementHeight = 112;
        }

        void DrawListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Levels");
        }

        void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            float h = EditorGUIUtility.singleLineHeight;
            float offset = h + EditorGUIUtility.standardVerticalSpacing;

            var prop = m_LevelsList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;
            rect.height = h;
            EditorGUI.PropertyField(rect, prop.FindPropertyRelative("displayName"));
            rect.y += offset;
            EditorGUI.PropertyField(rect, prop.FindPropertyRelative("loadName"));
            rect.y += offset;
            EditorGUI.PropertyField(rect, prop.FindPropertyRelative("screenshot"));
            rect.y += offset;
            rect.height += offset * 2;
            EditorGUI.PropertyField(rect, prop.FindPropertyRelative("description"));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StartingSelection"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnBackButtonPressed"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Level Select", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PrototypeEntry"));
            m_LevelsList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}