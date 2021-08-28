using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(LadderAudioBehaviour))]
    public class LadderAudioBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<TransformParameter>(
                owner.container,
                serializedObject.FindProperty("m_LadderTransform"),
                new GUIContent("Ladder Transform")
                );
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SpacingMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinimumSpeed"));
        }
    }
}
