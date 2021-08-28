using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPSEditor.CharacterMotion;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(SetTimeScaleBehaviour))]
    public class SetTimeScaleBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TimeScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ChargeDrain"));
        }
    }
}
