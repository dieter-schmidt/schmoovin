using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(ClampIntParameterBehaviour))]
    public class ClampIntParameterBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<IntParameter>(
                owner.container,
                serializedObject.FindProperty("m_Parameter"),
                new GUIContent("Parameter")
                );

            var fromProp = serializedObject.FindProperty("m_From");
            var toProp = serializedObject.FindProperty("m_To");

            EditorGUILayout.PropertyField(fromProp);
            if (fromProp.intValue > toProp.intValue)
                toProp.intValue = fromProp.intValue;

            EditorGUILayout.PropertyField(toProp);
            if (fromProp.intValue > toProp.intValue)
                fromProp.intValue = toProp.intValue;
        }
    }
}
