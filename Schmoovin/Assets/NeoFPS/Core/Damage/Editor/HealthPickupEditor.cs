using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(HealthPickup))]
    public class HealthPickupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var healTypeProp = serializedObject.FindProperty("m_HealType");
            var healAmountProp = serializedObject.FindProperty("m_HealAmount");

            EditorGUILayout.PropertyField(healTypeProp);
            if (healTypeProp.enumValueIndex == 0) // Amount, vs factors
            {
                EditorGUILayout.PropertyField(healAmountProp);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SingleUse"));
            }
            else
            {
                EditorGUILayout.Slider(healAmountProp, 0f, 1f);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnHealedCharacter"));

            var consumeProp = serializedObject.FindProperty("m_ConsumeResult");
            EditorGUILayout.PropertyField(consumeProp); // Respawn
            if (consumeProp.enumValueIndex == 2)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RespawnDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DisplayMesh"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}