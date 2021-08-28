using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(LockInventorySelectionBehaviour))]
    public class LockInventorySelectionBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));

            var lockTo = serializedObject.FindProperty("m_LockSelectionTo");
            EditorGUILayout.PropertyField(lockTo);

            if (lockTo.enumValueIndex == 2)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SlotIndex"));
            else
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Silent"));
        }
    }
}
