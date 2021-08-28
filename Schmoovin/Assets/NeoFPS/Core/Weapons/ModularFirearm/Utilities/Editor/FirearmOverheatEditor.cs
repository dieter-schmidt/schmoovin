using NeoFPS;
using UnityEditor;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(FirearmOverheat))]
    public class FirearmOverheatEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GlowRenderer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GlowMaterialIndex"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GlowThreshold"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HazeRenderer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HazeMaterialIndex"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HazeThreshold"));

            var doOverheat = serializedObject.FindProperty("m_DoOverheat");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HeatPerShot"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HeatLostPerSecond"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Damping"));
            EditorGUILayout.PropertyField(doOverheat);
            if (doOverheat.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CoolingThreshold"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OverheatSound"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Volume"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnOverheat"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}