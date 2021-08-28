using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(SetTargetHeightBehaviour))]
    public class SetTargetHeightBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            var when = serializedObject.FindProperty("m_When");
            EditorGUILayout.PropertyField(when);

            if (when.enumValueIndex != (int)SetTargetHeightBehaviour.When.ExitOnly)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnEnterValue"));
            if (when.enumValueIndex != (int)SetTargetHeightBehaviour.When.EnterOnly)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnExitValue"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ResizeDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FromPoint"));
        }
    }
}
