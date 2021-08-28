using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(AnimOnlyAimer))]
    public class AnimOnlyAimerEditor : BaseAimerEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            base.OnInspectorGUIInternal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimAnimBool"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BlockTrigger"));
        }

        protected override void SetGizmoProperties()
        {
            gizmoRootTransform = ((AnimOnlyAimer)target).transform;
        }
    }
}