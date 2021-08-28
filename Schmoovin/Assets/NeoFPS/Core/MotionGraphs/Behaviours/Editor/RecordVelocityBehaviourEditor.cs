using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(RecordVelocityBehaviour))]
    public class RecordVelocityBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(
                owner.container,
                serializedObject.FindProperty("m_Parameter"),
                new GUIContent("Parameter")
                );

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));
        }
    }
}
