using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Behaviours;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(ImpactDamageBehaviour))]
    public class ImpactDamageBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FallDamageOnEnter"), new GUIContent("On Enter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FallDamageOnExit"), new GUIContent("On Exit"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BodyImpactDamageOnEnter"), new GUIContent("On Enter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BodyImpactDamageOnExit"), new GUIContent("On Exit"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HeadImpactDamageOnEnter"), new GUIContent("On Enter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HeadImpactDamageOnExit"), new GUIContent("On Exit"));
        }
    }
}
