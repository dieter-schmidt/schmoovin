using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    public class BaseMuzzleEffectEditor : BaseFirearmModuleBehaviourEditor
    {
        protected void DrawAudioProperties()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FiringSounds"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShotVolume"), true);
        }
    }
}