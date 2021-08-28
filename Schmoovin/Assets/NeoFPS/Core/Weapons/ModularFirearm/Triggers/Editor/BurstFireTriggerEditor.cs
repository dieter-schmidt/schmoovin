using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(BurstFireTrigger), true)]
    public class BurstFireTriggerEditor : BaseFirearmModuleBehaviourEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BurstSize"));

            // Draw spacing property
            var spacingProp = serializedObject.FindProperty("m_BurstSpacing");
            EditorGUILayout.PropertyField(spacingProp);

            // Show rate of fire from spacing
            float ticksPerSecond = 1f / Time.fixedDeltaTime;
            float floatSpacing = (float)spacingProp.intValue;
            EditorGUILayout.HelpBox(
                string.Format(
                    "A shot spacing of {0} gives rate of fire:\n{1} rounds per second, {2} rounds per minute",
                    spacingProp.intValue,
                    ticksPerSecond / floatSpacing,
                    60f * ticksPerSecond / floatSpacing
                ),
                MessageType.Info
                );
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CancelOnRelease"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinRepeatDelay"));

            var holdProp = serializedObject.FindProperty("m_RepeatOnTriggerHold");
            EditorGUILayout.PropertyField(holdProp);
            if (holdProp.boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HoldRepeatDelay"));
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TriggerHoldAnimKey"));
        }
    }
}