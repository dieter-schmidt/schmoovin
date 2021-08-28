using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPSEditor.CharacterMotion;

namespace NeoFPS
{
    [MotionGraphBehaviourEditor(typeof(CameraJiggleSpringBehaviour))]
    public class CameraJiggleSpringBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Optional Conditions", EditorStyles.boldLabel);

            MotionGraphEditorGUI.ParameterDropdownField<SwitchParameter>(
                owner.container,
                serializedObject.FindProperty("m_SwitchCondition")
                );
            MotionGraphEditorGUI.ParameterDropdownField<SwitchParameter>(
                owner.container,
                serializedObject.FindProperty("m_TriggerCondition")
                );

            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_JiggleStrength"));
        }
    }
}
