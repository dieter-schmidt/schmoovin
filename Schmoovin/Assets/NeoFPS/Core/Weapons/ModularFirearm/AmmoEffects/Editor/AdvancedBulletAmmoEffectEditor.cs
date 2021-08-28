using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(AdvancedBulletAmmoEffect))]
    public class AdvancedBulletAmmoEffectEditor : Editor
    {
        private const string k_NameRangeLower = "Effective Range";
        private const string k_TooltipRangeLower = "The range up to which the bullet does full damage";
        private const string k_NameRangeUpper = "Ineffective Range";
        private const string k_TooltipRangeUpper = "The range over which the bullet does zero damage";
        private const string k_NameSpeedLower = "Minimum Speed";
        private const string k_TooltipSpeedLower = "The speed below which the bullet does zero damage";
        private const string k_NameSpeedUpper = "Effective Speed";
        private const string k_TooltipSpeedUpper = "The speed above which the bullet does full damage";

        private GUIContent m_DamageLabel = null;
        public GUIContent damageLabel
        {
            get
            {
                if (m_DamageLabel == null)
                    m_DamageLabel = new GUIContent("Damage", "The damage to apply (not including falloff)");
                return m_DamageLabel;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            // Randomised damage
            var toggleRandom = serializedObject.FindProperty("m_RandomiseDamage");
            EditorGUILayout.PropertyField(toggleRandom);
            if (toggleRandom.boolValue)
            {
                var minDamage = serializedObject.FindProperty("m_MinDamage");
                var maxDamage = serializedObject.FindProperty("m_MaxDamage");

                EditorGUILayout.DelayedFloatField(minDamage);
                if (minDamage.floatValue > maxDamage.floatValue)
                    maxDamage.floatValue = minDamage.floatValue;

                EditorGUILayout.DelayedFloatField(maxDamage);
                if (minDamage.floatValue > maxDamage.floatValue)
                    minDamage.floatValue = maxDamage.floatValue;
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxDamage"), damageLabel);
            }

            // DamageFalloff
            var falloffMode = serializedObject.FindProperty("m_FalloffMode");
            EditorGUILayout.PropertyField(falloffMode);
            switch(falloffMode.enumValueIndex)
            {
                // 0 = no falloff
                case 1: // Range based
                    {
                        var lower = serializedObject.FindProperty("m_FalloffSettingLower");
                        var upper = serializedObject.FindProperty("m_FalloffSettingUpper");

                        EditorGUILayout.DelayedFloatField(lower, new GUIContent(k_NameRangeLower, k_TooltipRangeLower));
                        if (lower.floatValue > upper.floatValue)
                            upper.floatValue = lower.floatValue;

                        EditorGUILayout.DelayedFloatField(upper, new GUIContent(k_NameRangeUpper, k_TooltipRangeUpper));
                        if (lower.floatValue > upper.floatValue)
                            lower.floatValue = upper.floatValue;
                    }
                    break;
                case 2: // Speed based
                    {
                        var lower = serializedObject.FindProperty("m_FalloffSettingLower");
                        var upper = serializedObject.FindProperty("m_FalloffSettingUpper");

                        EditorGUILayout.DelayedFloatField(upper, new GUIContent(k_NameSpeedUpper, k_TooltipSpeedUpper));
                        if (lower.floatValue > upper.floatValue)
                            lower.floatValue = upper.floatValue;

                        EditorGUILayout.DelayedFloatField(lower, new GUIContent(k_NameSpeedLower, k_TooltipSpeedLower));
                        if (lower.floatValue > upper.floatValue)
                            upper.floatValue = lower.floatValue;
                    }
                    break;
            }


            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BulletSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ImpactForce"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}