using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(BasicGameObjectMuzzleEffect))]
    public class BasicGameObjectMuzzleEffectEditor : BaseMuzzleEffectEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MuzzleFlash"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MuzzleFlashDuration"));

            DrawAudioProperties();
        }
    }
}