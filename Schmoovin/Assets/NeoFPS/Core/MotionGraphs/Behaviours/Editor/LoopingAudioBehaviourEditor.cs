using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(LoopingAudioBehaviour))]
    public class LoopingAudioBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Source"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Volume"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Pitch"));
        }
    }
}
