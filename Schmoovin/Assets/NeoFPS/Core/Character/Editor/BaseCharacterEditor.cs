using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(BaseCharacter), true)]
    public class BaseCharacterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HeadTransformHandler"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BodyTransformHandler"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DamageAudioThreshold"));

            var damageProp = serializedObject.FindProperty("m_ApplyFallDamage");
            EditorGUILayout.PropertyField(damageProp);
            if (damageProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LandingMinForce"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LandingFullForce"));
            }

            damageProp = serializedObject.FindProperty("m_BodyImpactDamage");
            EditorGUILayout.PropertyField(damageProp);
            if (damageProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BodyMinForce"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BodyFullForce"));
            }

            damageProp = serializedObject.FindProperty("m_HeadImpactDamage");
            EditorGUILayout.PropertyField(damageProp);
            if (damageProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HeadMinForce"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HeadFullForce"));
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SoftLandings"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HardLandings"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinLandingThreshold"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HardLandingThreshold"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxRayDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RayOffset"));
            
            OnCharacterInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnCharacterInspectorGUI()
        {
            // Iterate through properties after ray offset
            var itr = serializedObject.FindProperty("m_RayOffset");
            while (itr.NextVisible(true))
                EditorGUILayout.PropertyField(itr);
        }
    }
}