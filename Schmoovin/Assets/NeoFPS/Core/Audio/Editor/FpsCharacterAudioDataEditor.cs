using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.Constants;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor (typeof (FpsCharacterAudioData))]
    public class FpsCharacterAudioDataEditor : Editor
    {
        private FpsCharacterAudioData m_Data = null;
        private DataEditor[] m_SubEditors = null;

        private static ClipboardData m_Clipboard = null;

        private class ClipboardData
        {
            public AudioClip[] clips;
            public float volume;
            public float minSpacing;

            public ClipboardData(AudioClip[] clips, float volume, float minSpacing)
            {
                this.clips = clips;
                this.volume = volume;
                this.minSpacing = minSpacing;
            }
        }

        private class DataEditor
        {
            SubInspectorTitlebar m_TitleBar;
            ReorderableList m_ClipsList;
            FpsCharacterAudio m_ID;

            SerializedProperty m_VolumeProperty;
            SerializedProperty m_MinSpacingProperty;

            public DataEditor(SerializedObject so, FpsCharacterAudio id)
            {
                m_ID = id;

                m_TitleBar = new SubInspectorTitlebar();
                m_TitleBar.AddContextOption("Copy", ContextCopy, null);
                m_TitleBar.AddContextOption("Paste Values", ContextPaste, CheckContextPaste);
                m_TitleBar.getLabel = GetEntryLabel;

                var dataProp = so.FindProperty("m_Data").GetArrayElementAtIndex(id);

                m_ClipsList = new ReorderableList(
                    so,
                    dataProp.FindPropertyRelative("m_Clips"),
                    true,
                    true,
                    true,
                    true
                );
                m_ClipsList.drawHeaderCallback = DrawClipsHeader;
                m_ClipsList.drawElementCallback = DrawClipsElement;

                m_VolumeProperty = dataProp.FindPropertyRelative("m_Volume");
                m_MinSpacingProperty = dataProp.FindPropertyRelative("m_MinSpacing");
            }

            public void DoLayout()
            {
                // Draw the name
                bool expanded = m_TitleBar.DoLayout();

                if (expanded)
                {
                    // Draw the clips array
                    m_ClipsList.DoLayoutList();

                    // Draw the other properties
                    EditorGUILayout.PropertyField(m_VolumeProperty);
                    EditorGUILayout.PropertyField(m_MinSpacingProperty);
                }
            }

            void DrawClipsHeader(Rect rect)
            {
                EditorGUI.LabelField(rect, "Audio Clips");
            }

            void DrawClipsElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                rect.height -= 4f;
                rect.y += 1f;
                var element = m_ClipsList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            }

            string GetEntryLabel()
            {
                return FpsCharacterAudio.names[m_ID];
            }

            void ContextCopy()
            {
                // Get clips
                AudioClip[] clips = new AudioClip[m_ClipsList.count];
                for (int i = 0; i < clips.Length; ++i)
                    clips[i] = m_ClipsList.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue as AudioClip;

                m_Clipboard = new ClipboardData (
                    clips,
                    m_VolumeProperty.floatValue,
                    m_MinSpacingProperty.floatValue
                );
            }

            void ContextPaste()
            {
                // Set array
                int missing = 0;
                int count = m_Clipboard.clips.Length;
                m_ClipsList.serializedProperty.ClearArray();
                m_ClipsList.serializedProperty.arraySize = count;
                for (int i = 0; i < count; ++i)
                {
                    if (m_Clipboard.clips[i] != null)
                        m_ClipsList.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue = m_Clipboard.clips[i];
                    else
                        ++missing;
                }
                if (missing > 0)
                    m_ClipsList.serializedProperty.arraySize -= missing;
                
                // Set float properties
                m_VolumeProperty.floatValue = m_Clipboard.volume;
                m_MinSpacingProperty.floatValue = m_Clipboard.minSpacing;
            }

            bool CheckContextPaste()
            {
                return m_Clipboard != null;
            }
        }

        void OnEnable()
        {
            m_Data = target as FpsCharacterAudioData;
            m_Data.CheckValidity();

            m_SubEditors = new DataEditor[FpsCharacterAudio.count];
            for (int i = 0; i < FpsCharacterAudio.count; ++i)
            {
                m_SubEditors[i] = new DataEditor(serializedObject, (FpsCharacterAudio)i);
            }
        }

        void OnDisable()
        {
            m_Clipboard = null;
        }

        public override void OnInspectorGUI()
        {
            for (int i = 0; i < m_SubEditors.Length; ++i)
                m_SubEditors[i].DoLayout();
            serializedObject.ApplyModifiedProperties();
        }
    }
}