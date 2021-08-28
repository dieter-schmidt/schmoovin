using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(DebugBehaviour))]
    public class DebugBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnEnterMessage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnExitMessage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LogElapsedTime"));
        }
    }
}
