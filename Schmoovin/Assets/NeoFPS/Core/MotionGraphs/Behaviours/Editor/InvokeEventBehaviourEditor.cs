using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(InvokeEventBehaviour))]
    public class InvokeEventBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<EventParameter>(
                owner.container,
                serializedObject.FindProperty("m_Property"),
                new GUIContent("Parameter")
                );
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));
        }
    }
}
