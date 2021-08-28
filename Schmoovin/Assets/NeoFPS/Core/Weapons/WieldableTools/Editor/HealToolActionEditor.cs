using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;
using NeoFPS.WieldableTools;
using System;
using System.Collections.Generic;

namespace NeoFPSEditor.WieldableTools
{
    [CustomEditor(typeof(HealToolAction))]
    public class HealToolActionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Timing"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HealAmount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HealInterval"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Instant"));

            var prop = serializedObject.FindProperty("m_Subject");
            EditorGUILayout.PropertyField(prop);
            if (prop.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TargetLayers"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxRange"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}