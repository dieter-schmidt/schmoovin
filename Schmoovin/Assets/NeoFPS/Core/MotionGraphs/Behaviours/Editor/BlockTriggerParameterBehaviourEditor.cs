using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(BlockTriggerParameterBehaviour))]
    public class BlockTriggerParameterBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<TriggerParameter>(
                owner.container,
                serializedObject.FindProperty("m_Parameter"),
                new GUIContent("Parameter")
                );
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnEnter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnExit"));
        }
    }
}
