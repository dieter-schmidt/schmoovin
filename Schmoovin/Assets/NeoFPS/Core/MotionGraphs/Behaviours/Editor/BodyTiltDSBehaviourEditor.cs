using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(BodyTiltBehaviourDS))]
    public class BodyTiltDSBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TiltAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_NormalisedTiltPoint"));

            var tiltMode = serializedObject.FindProperty("m_TiltMode");
            EditorGUILayout.PropertyField(tiltMode);
            if (tiltMode.enumValueIndex <= 1 || tiltMode.enumValueIndex >= 6)
            {
                MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(
                    owner.container,
                    serializedObject.FindProperty("m_DirectionVector"),
                    new GUIContent("Direction Vector")
                    );
            }

            if (tiltMode.enumValueIndex < 6)
            {
                var velocityBased = serializedObject.FindProperty("m_VelocityBased");
                EditorGUILayout.PropertyField(velocityBased);
                if (velocityBased.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxSpeed"));
                }
            }
        }
    }
}
