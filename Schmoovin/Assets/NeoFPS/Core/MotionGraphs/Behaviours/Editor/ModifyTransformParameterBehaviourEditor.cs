using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(ModifyTransformParameterBehaviour))]
    public class ModifyTransformParameterBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<TransformParameter>(
                owner.container,
                serializedObject.FindProperty("m_Parameter"),
                new GUIContent("Parameter")
                );

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));

            var what = serializedObject.FindProperty("m_What");
            EditorGUILayout.PropertyField(what);

            if (what.enumValueIndex == 1) // Set value
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Find"));
        }
    }
}
