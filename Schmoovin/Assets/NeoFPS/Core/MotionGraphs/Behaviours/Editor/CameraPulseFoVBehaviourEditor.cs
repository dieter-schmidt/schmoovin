using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPSEditor.CharacterMotion;

namespace NeoFPS
{
    [MotionGraphBehaviourEditor(typeof(CameraPulseFoVBehaviour))]
    public class CameraPulseFoVBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FovMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PulseDuration"));

            EditorGUILayout.CurveField(serializedObject.FindProperty("m_PulseCurve"), Color.green, new Rect(0f, -1f, 1f, 2f));
        }
    }
}
