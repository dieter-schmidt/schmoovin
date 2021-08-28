using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(SwimSmoothUnderwaterState))]
    //[HelpURL("")]
    public class SwimSmoothUnderwaterStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<TransformParameter>(container, serializedObject.FindProperty("m_WaterZoneParameter"));
            MotionGraphEditorGUI.ParameterDropdownField<SwitchParameter>(container, serializedObject.FindProperty("m_CrouchHold"));
            MotionGraphEditorGUI.ParameterDropdownField<SwitchParameter>(container, serializedObject.FindProperty("m_JumpHold"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_SwimSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_UpDownSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_Acceleration"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_StrafeMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_ReverseMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_IdleMultiplier"));
        }
    }
}