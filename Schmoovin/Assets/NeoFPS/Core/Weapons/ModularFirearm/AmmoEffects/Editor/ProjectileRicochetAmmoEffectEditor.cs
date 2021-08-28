using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(RicochetProjectileAmmoEffect), true)]
    public class ProjectileRicochetAmmoEffectEditor : Editor
    {
        // Multiplier
        private const string k_NameMultiplier = "Speed Multiplier";
        private const string k_TooltipMultiplier = "A multiplier applied to the bullet speed after ricochet";

        // FixedSpeed
        private const string k_NameFixedSpeed = "Fixed Speed";
        private const string k_TooltipFixedSpeed = "The speed of the projectile after ricochet";

        // AngleBasedMultiplier
        private const string k_NameAngleBasedMultiplierMin = "Straight On Multiplier";
        private const string k_TooltipAngleBasedMultiplierMin = "A multiplier applied to the bullet speed after ricochet when the entry velocity was perpendicular to the surface";
        private const string k_NameAngleBasedMultiplierMax = "Glancing Multiplier";
        private const string k_TooltipAngleBasedMultiplierMax = "A multiplier applied to the bullet speed after ricochet if the initial bullet was travelling parallel to the surface";

        // AngleBasedMultiplier
        private const string k_NameAngleBasedSpeedMin = "Straight On Speed";
        private const string k_TooltipAngleBasedSpeedMin = "The speed of the projectile after ricochet when the entry velocity was perpendicular to the surface";
        private const string k_NameAngleBasedSpeedMax = "Glancing Speed";
        private const string k_TooltipAngleBasedSpeedMax = "The speed of the projectile after ricochet if the initial bullet was travelling parallel to the surface";

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var itr = serializedObject.GetIterator();
            while (itr.NextVisible(true))
            {
                if (itr.name == "m_SpeedMode")
                {
                    EditorGUILayout.PropertyField(itr);
                    var mode = (RicochetProjectileAmmoEffect.RicochetSpeedMode)itr.enumValueIndex;

                    itr.NextVisible(true);
                    var minProp = itr;
                    itr.NextVisible(true);
                    var maxProp = itr;

                    InspectSpeedSettings(minProp, maxProp, mode);
                }
                else
                    EditorGUILayout.PropertyField(itr);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public static void InspectSpeedSettings(SerializedProperty minProp, SerializedProperty maxProp, RicochetProjectileAmmoEffect.RicochetSpeedMode mode)
        {
            switch (mode)
            {
                case RicochetProjectileAmmoEffect.RicochetSpeedMode.Multiplier:
                    EditorGUILayout.PropertyField(minProp, new GUIContent(k_NameMultiplier, k_TooltipMultiplier));
                    break;
                case RicochetProjectileAmmoEffect.RicochetSpeedMode.FixedSpeed:
                    EditorGUILayout.PropertyField(minProp, new GUIContent(k_NameFixedSpeed, k_TooltipFixedSpeed));
                    break;
                case RicochetProjectileAmmoEffect.RicochetSpeedMode.AngleBasedMultiplier:
                    EditorGUILayout.PropertyField(minProp, new GUIContent(k_NameAngleBasedMultiplierMin, k_TooltipAngleBasedMultiplierMin));
                    EditorGUILayout.PropertyField(maxProp, new GUIContent(k_NameAngleBasedMultiplierMax, k_TooltipAngleBasedMultiplierMax));
                    break;
                case RicochetProjectileAmmoEffect.RicochetSpeedMode.AngleBasedSpeed:
                    EditorGUILayout.PropertyField(minProp, new GUIContent(k_NameAngleBasedSpeedMin, k_TooltipAngleBasedSpeedMin));
                    EditorGUILayout.PropertyField(maxProp, new GUIContent(k_NameAngleBasedSpeedMax, k_TooltipAngleBasedSpeedMax));
                    break;
            }
        }
    }
}