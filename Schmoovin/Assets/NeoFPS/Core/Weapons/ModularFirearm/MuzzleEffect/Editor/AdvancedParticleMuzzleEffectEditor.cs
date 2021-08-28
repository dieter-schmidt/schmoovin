using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(AdvancedParticleMuzzleEffect))]
    public class AdvancedParticleMuzzleEffectEditor : BaseMuzzleEffectEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EffectTransform"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FollowDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ParticleSystems"), true);

            DrawAudioProperties();
        }
    }
}