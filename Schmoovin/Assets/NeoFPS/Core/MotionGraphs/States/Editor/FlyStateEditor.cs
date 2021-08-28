using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(FlyState))]
    //[HelpURL("")]
    public class FlyStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<SwitchParameter>(container, serializedObject.FindProperty("m_JumpHold"));
            MotionGraphEditorGUI.ParameterDropdownField<SwitchParameter>(container, serializedObject.FindProperty("m_CrouchHold"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_TopSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_StrafeMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_ReverseMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_UpDownMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_Acceleration"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Fly Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PitchMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Damping"));
        }
    }
}