using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(SurfaceAudioBehaviour))]
    public class SurfaceAudioBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));

            var prop = serializedObject.FindProperty("m_CastDirection");
            EditorGUILayout.PropertyField(prop);

            switch (prop.enumValueIndex)
            {
                case 0:
                    break;
                case 1:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CastVector"));
                    break;
                case 2:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CastVector"));
                    break;
                default:
                    MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(
                        owner.container,
                        serializedObject.FindProperty("m_VectorParameter")
                        );
                    break;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxRayDistance"));
        }
    }
}
