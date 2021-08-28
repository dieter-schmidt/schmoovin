using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(DashState))]
    //[HelpURL("")]
    public class DashStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_DashSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_Acceleration"));

            var cast = target as DashState;
            if (!cast.isInstant)
                MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_DashDistance"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Dash Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FrameOfReference"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DashAngle"));
        }
    }
}