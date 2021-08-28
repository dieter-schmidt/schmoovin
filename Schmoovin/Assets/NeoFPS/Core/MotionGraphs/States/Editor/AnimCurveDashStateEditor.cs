using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(AnimCurveDashState))]
    //[HelpURL("")]
    public class AnimCurveDashStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_DashSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_MaxControlSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_StrafeMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_ReverseMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_Acceleration"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Dash Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FrameOfReference"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DashAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DashInTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DashOutTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DashOutCurve"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ApplyGravity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ControlDamping"));
        }
    }
}