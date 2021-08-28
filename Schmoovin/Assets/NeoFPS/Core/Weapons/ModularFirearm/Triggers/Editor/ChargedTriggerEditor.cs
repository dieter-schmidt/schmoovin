using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(ChargedTrigger), true)]
    public class ChargedTriggerEditor : BaseFirearmModuleBehaviourEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ChargeDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UnchargeDuration"));

            // Draw repeat property
            var repeatProp = serializedObject.FindProperty("m_Repeat");
            EditorGUILayout.PropertyField(repeatProp);
            if (repeatProp.boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RepeatDelay"));

            // Audio

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioSource"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TriggerAudioCharge"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TriggerAudioRelease"));

            // Animation

            var animationTypeProp = serializedObject.FindProperty("m_ChargeAnimation");
            EditorGUILayout.PropertyField(animationTypeProp);
            int index = animationTypeProp.enumValueIndex;
            if (index == 0)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LayerIndex"));
            if (index == 1)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ChargeAnimKey"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnChargeChanged"));
        }
    }
}