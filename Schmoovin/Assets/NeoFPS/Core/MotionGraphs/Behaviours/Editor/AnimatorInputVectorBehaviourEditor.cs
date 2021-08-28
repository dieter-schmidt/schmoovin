using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(AnimatorInputVectorBehaviour))]
    public class AnimatorInputVectorBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<TransformParameter>(
                owner.container,
                serializedObject.FindProperty("m_AnimatorTransform"),
                new GUIContent("Animator Transform", "The transform containing the animator controller to affect")
                );

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ForwardParamName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StrafeParamName"));
        }
    }
}