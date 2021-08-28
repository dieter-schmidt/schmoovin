using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(SetAnimatorFloatBehaviour))]
    public class SetAnimatorFloatBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<TransformParameter>(
                owner.container,
                serializedObject.FindProperty("m_AnimatorTransform"),
                new GUIContent("Animator Transform", "The transform containing the animator controller to affect")
                );

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ParameterName"));

            var whenProp = serializedObject.FindProperty("m_When");
            EditorGUILayout.PropertyField(whenProp);

            switch(whenProp.enumValueIndex)
            {
                case 0:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnEnterValue"));
                    break;
                case 1:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnExitValue"));
                    break;
                case 2:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnEnterValue"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnExitValue"));
                    break;
            }
        }
    }
}
