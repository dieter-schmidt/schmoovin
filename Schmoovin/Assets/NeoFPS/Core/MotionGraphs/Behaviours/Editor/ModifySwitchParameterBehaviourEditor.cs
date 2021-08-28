using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(ModifySwitchParameterBehaviour))]
    public class ModifySwitchParameterBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<SwitchParameter>(
                owner.container,
                serializedObject.FindProperty("m_Parameter"),
                new GUIContent("Parameter")
                );
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnEnter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnExit"));
        }
    }
}
