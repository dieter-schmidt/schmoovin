using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(FirstPersonCamera))]
    public class FirstPersonCameraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Camera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioListener"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimTransform"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviousCameraAction"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OffsetTransform"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimPositionEffectMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimRotationEffectMultiplier"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
