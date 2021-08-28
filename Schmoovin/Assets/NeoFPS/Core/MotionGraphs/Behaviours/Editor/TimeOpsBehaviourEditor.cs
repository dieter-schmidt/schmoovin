using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(TimeOpsBehaviour))]
    public class TimeOpsBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            // Show what the operation is
            var prop = serializedObject.FindProperty("m_What");
            EditorGUILayout.PropertyField(prop);

            // Show the output parameter
            MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(
                owner.container,
                serializedObject.FindProperty("m_Parameter"),
                new GUIContent("Output")
                );

            // Show multiplier if "what" is "AddElapsedTimeScaled"
            if (prop.enumValueIndex == 1)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Multiplier"));

        }
    }
}
