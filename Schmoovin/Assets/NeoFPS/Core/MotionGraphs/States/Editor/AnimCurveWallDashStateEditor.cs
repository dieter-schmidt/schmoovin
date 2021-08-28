using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(AnimCurveWallDashState))]
    //[HelpURL("")]
    public class AnimCurveWallDashStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(container, serializedObject.FindProperty("m_WallNormal"));

            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_DashSpeed"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Dash Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DashInTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DashOutTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DashOutCurve"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_YawWithCurve"));
        }
    }
}