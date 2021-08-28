using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(ConstrainCameraYawBehaviour))]
    public class ConstrainCameraYawBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(
                owner.container,
                serializedObject.FindProperty("m_Direction")
                );

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AngleRange"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Flipped"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Continuous"));
        }
    }
}
