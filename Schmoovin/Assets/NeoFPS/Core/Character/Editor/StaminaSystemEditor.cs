using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor (typeof(StaminaSystem), true)]
    public class StaminaSystemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            // Stamina

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Stamina"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxStamina"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StaminaRefreshRate"));

            // Movement speed (MD override)

            var prop = serializedObject.FindProperty("m_AffectMovementSpeed");
            EditorGUILayout.PropertyField(prop);
            if (prop.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinWalkMultiplier"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinSprintMultiplier"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinCrouchMultiplier"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MoveSpeedCurve"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_WalkSpeedData"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimWalkSpeedData"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SprintSpeedData"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimSprintSpeedData"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CrouchSpeedData"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimCrouchSpeedData"));
            }

            // Breathing

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BreatheSlowInterval"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BreatheFastInterval"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BreathingRateCurve"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BreathingStrengthCurve"));

            // Exhaustion

            prop = serializedObject.FindProperty("m_UseExhaustion");
            EditorGUILayout.PropertyField(prop);
            if (prop.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ExhaustionThreshold"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RecoverThreshold"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ExhaustedMotionParameter"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SprintMotionParameter"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnExhausted"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnRecovered"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}