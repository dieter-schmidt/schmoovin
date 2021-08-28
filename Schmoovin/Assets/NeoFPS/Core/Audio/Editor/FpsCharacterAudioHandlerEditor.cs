using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.Constants;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor (typeof (FpsCharacterAudioHandler))]
    public class FpsCharacterAudioHandlerEditor : Editor
    {
        ReorderableList m_OneShotList = null;
        ReorderableList m_LoopList = null;

        void OnEnable()
        {
            m_OneShotList = new ReorderableList(
                       serializedObject,
                       serializedObject.FindProperty("m_OneShotSources"),
                       true,
                       true,
                       false,
                       false
                   );
            m_OneShotList.drawHeaderCallback = DrawOneShotHeader;
            m_OneShotList.drawElementCallback = DrawOneShotElement;

            m_LoopList = new ReorderableList(
                       serializedObject,
                       serializedObject.FindProperty("m_LoopSources"),
                       true,
                       true,
                       false,
                       false
                   );
            m_LoopList.drawHeaderCallback = DrawLoopHeader;
            m_LoopList.drawElementCallback = DrawLoopElement;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MixerGroup"));

            EditorGUILayout.HelpBox("The different sources here are based on the FpsCharacterAudioSource constant. You can add more sources by modifying the constants settings file and regenerating this constant.", MessageType.Info);

            m_OneShotList.DoLayoutList();
            m_LoopList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawOneShotHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "One Shot Sources");
        }

        void DrawLoopHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Looping Sources");
        }

        void DrawOneShotElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height -= 4f;
            rect.y += 1f;
            var element = m_OneShotList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, new GUIContent(FpsCharacterAudioSource.names[index]));
        }

        void DrawLoopElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height -= 4f;
            rect.y += 1f;
            var element = m_LoopList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, new GUIContent(FpsCharacterAudioSource.names[index]));
        }
    }
}