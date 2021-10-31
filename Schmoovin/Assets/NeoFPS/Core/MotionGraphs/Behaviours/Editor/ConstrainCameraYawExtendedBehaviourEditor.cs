using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(ConstrainCameraYawExtendedBehaviour))]
    public class ConstrainCameraYawExtendedBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            //default
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(
                owner.container,
                serializedObject.FindProperty("m_Direction")
                );

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AngleRange"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Flipped"));

            //alternate
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(
                owner.container,
                serializedObject.FindProperty("m_Alternate_Direction")
                );

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Alternate_AngleRange"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Alternate_Flipped"));

            MotionGraphEditorGUI.ParameterDropdownField<SwitchParameter>(
                owner.container,
                serializedObject.FindProperty("m_Toggle_Parameter")
                );

            //both
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Continuous"));
        }
    }
}
