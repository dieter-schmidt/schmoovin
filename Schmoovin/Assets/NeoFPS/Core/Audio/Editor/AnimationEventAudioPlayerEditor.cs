using NeoFPS;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(AnimationEventAudioPlayer), true)]
    public class AnimationEventAudioPlayerEditor : Editor
    {
        private List<ModeEntry> m_Entries = new List<ModeEntry>();

        public static bool resetEntries = true;

        public class ModeEntry
        {
            private SubInspectorTitlebar m_Titlebar = null;
            private ReorderableList m_ClipsList = null;
            private SerializedProperty m_ClipSetsProp = null;
            private int m_Index = 0;

            public ModeEntry(int i, SerializedProperty clipSetsProp)
            {
                m_Index = i;
                m_ClipSetsProp = clipSetsProp;

                m_Titlebar = new SubInspectorTitlebar(true);
                m_Titlebar.getLabel = GetTitle;
                m_Titlebar.AddContextOption("Move Up", OnMenuMoveUp, CanMenuMoveUp);
                m_Titlebar.AddContextOption("Move Down", OnMenuMoveDown, CanMenuMoveDown);
                m_Titlebar.AddContextOption("Remove", OnMenuRemove, null);

                m_ClipsList = new ReorderableList(
                    clipSetsProp.serializedObject, 
                    clipSetsProp.GetArrayElementAtIndex(i).FindPropertyRelative("clips")
                    );
                m_ClipsList.drawHeaderCallback = OnListDrawHeader;
                m_ClipsList.drawElementCallback = OnListDrawElement;
                m_ClipsList.onAddDropdownCallback = OnListAdd;
                m_ClipsList.onRemoveCallback = OnListRemove;
            }

            void OnListDrawElement(Rect r, int index, bool isActive, bool isFocussed)
            {
                r.height -= 4;
                r.y += 1;
                var p = m_ClipsList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(r, p, GUIContent.none);
            }

            void OnListDrawHeader(Rect r)
            {
                EditorGUI.LabelField(r, "Audio Clips");
            }

            void OnListAdd(Rect buttonRect, ReorderableList list)
            {
                ++list.serializedProperty.arraySize;
                m_ClipSetsProp.serializedObject.ApplyModifiedProperties();
            }

            void OnListRemove(ReorderableList list)
            {
                int i = list.index;
                if (i == -1)
                    return;

                SerializedArrayUtility.RemoveAt(list.serializedProperty, i);
                m_ClipSetsProp.serializedObject.ApplyModifiedProperties();
            }

            string GetTitle()
            {
                var title = m_ClipSetsProp.GetArrayElementAtIndex(m_Index).FindPropertyRelative("key").stringValue;
                if (string.IsNullOrEmpty(title))
                    return "<Key Required>";
                else
                    return title;
            }

            public void DoLayout()
            {
                if (m_Titlebar.DoLayout())
                {
                    var modeProp = m_ClipSetsProp.GetArrayElementAtIndex(m_Index);

                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(modeProp.FindPropertyRelative("key"), true);
                    EditorGUILayout.PropertyField(modeProp.FindPropertyRelative("volume"), true);
                    EditorGUILayout.PropertyField(modeProp.FindPropertyRelative("nextClip"), true);
                    --EditorGUI.indentLevel;
                    m_ClipsList.DoLayoutList();
                    EditorGUILayout.Space();
                }
            }

            void OnMenuMoveUp()
            {
                SerializedArrayUtility.Move(m_ClipSetsProp, m_Index, m_Index - 1);
                m_ClipSetsProp.serializedObject.ApplyModifiedProperties();
                resetEntries = true;
            }

            void OnMenuMoveDown()
            {
                SerializedArrayUtility.Move(m_ClipSetsProp, m_Index, m_Index + 1);
                m_ClipSetsProp.serializedObject.ApplyModifiedProperties();
                resetEntries = true;
            }

            bool CanMenuMoveUp()
            {
                return m_Index > 0;
            }

            bool CanMenuMoveDown()
            {
                return m_Index < m_ClipSetsProp.arraySize - 1;
            }

            void OnMenuRemove()
            {
                SerializedArrayUtility.RemoveAt(m_ClipSetsProp, m_Index);
                m_ClipSetsProp.serializedObject.ApplyModifiedProperties();
                resetEntries = true;
            }
        }

        private void Awake()
        {
            resetEntries = true;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioSource"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Clip Sets", EditorStyles.boldLabel);

            // Get the clips set property
            var clipSetsProp = serializedObject.FindProperty("m_ClipSets");

            // Add new clips set
            if (GUILayout.Button("Add New Set"))
            {
                ++clipSetsProp.arraySize;
                var newModeProp = clipSetsProp.GetArrayElementAtIndex(clipSetsProp.arraySize - 1);
                newModeProp.FindPropertyRelative("key").stringValue = string.Empty;
                newModeProp.FindPropertyRelative("nextClip").enumValueIndex = 0;
                newModeProp.FindPropertyRelative("volume").floatValue = 1f;
                newModeProp.FindPropertyRelative("clips").arraySize = 0;
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.Space();

            // Reset entries
            if (resetEntries || m_Entries.Count != clipSetsProp.arraySize)
            {
                resetEntries = false;

                m_Entries.Clear();

                for (int i = 0; i < clipSetsProp.arraySize; ++i)
                    m_Entries.Add(new ModeEntry(i, clipSetsProp));
            }

            // Draw entries
            for (int i = 0; i < clipSetsProp.arraySize; ++i)
                m_Entries[i].DoLayout();

            serializedObject.ApplyModifiedProperties();
        }
    }
}