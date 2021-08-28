using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(SimpleParticleMuzzleEffect))]
    public class SimpleParticleMuzzleEffectEditor : BaseMuzzleEffectEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ParticleSystem"));

            DrawAudioProperties();
        }
    }
}