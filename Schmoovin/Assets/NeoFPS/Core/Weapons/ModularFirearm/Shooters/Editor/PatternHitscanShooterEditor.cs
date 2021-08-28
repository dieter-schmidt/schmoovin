using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(PatternHitscanShooter), true)]
    public class PatternHitscanShooterEditor : PatternShooterEditorBase
    {
        protected override void OnInspectorGUIInternal()
        {
            // Draw the pattern property
            DrawPatternProperty(target, serializedObject.FindProperty("m_PatternPoints"), serializedObject.FindProperty("m_PatternDistance"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MuzzleTip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Layers"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinAimOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxAimOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UseCameraAim"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TracerPrototype"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TracerSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TracerDuration"));
        }
    }
}