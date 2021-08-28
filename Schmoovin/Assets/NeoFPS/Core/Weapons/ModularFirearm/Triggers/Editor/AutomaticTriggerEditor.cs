using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor (typeof(AutomaticTrigger), true)]
    public class AutomaticTriggerEditor : BaseFirearmModuleBehaviourEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            // Draw spacing property
            var spacingProp = serializedObject.FindProperty("m_ShotSpacing");
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
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TriggerHoldAnimKey"));
        }
    }
}