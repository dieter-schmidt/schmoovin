using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(ModifyFloatParameterBehaviour))]
    public class ModifyFloatParameterBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(
                owner.container,
                serializedObject.FindProperty("m_Parameter"),
                new GUIContent("Parameter")
                );

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));

            var what = serializedObject.FindProperty("m_What");
            EditorGUILayout.PropertyField(what);

            if (what.enumValueIndex != 1 && what.enumValueIndex != 4 && what.enumValueIndex != 5) // Don't show for "Reset"
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Value"));
        }
    }
}
