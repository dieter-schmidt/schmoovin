using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(InstantScopedAimer))]
    public class InstantScopedAimerEditor : BaseAimerEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            base.OnInspectorGUIInternal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HudScopeKey"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FovMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PositionSpringMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RotationSpringMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CrosshairUp"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CrosshairDown"));
        }

        protected override void SetGizmoProperties()
        {
            gizmoRootTransform = ((InstantScopedAimer)target).transform;
        }
    }
}