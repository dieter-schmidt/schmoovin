using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(ClampFloatParameterBehaviour))]
    public class ClampFloatParameterBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(
                owner.container,
                serializedObject.FindProperty("m_Parameter"),
                new GUIContent("Parameter")
                );

            var fromProp = serializedObject.FindProperty("m_From");
            var toProp = serializedObject.FindProperty("m_To");

            EditorGUILayout.PropertyField(fromProp);
            if (fromProp.floatValue > toProp.floatValue)
                toProp.floatValue = fromProp.floatValue;

            EditorGUILayout.PropertyField(toProp);
            if (fromProp.floatValue > toProp.floatValue)
                fromProp.floatValue = toProp.floatValue;
        }
    }
}
