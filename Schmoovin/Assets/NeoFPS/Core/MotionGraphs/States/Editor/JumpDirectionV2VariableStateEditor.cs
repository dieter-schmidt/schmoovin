using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(JumpDirectionV2VariableState))]
    //[HelpURL("")]
    public class JumpDirectionV2VariableStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var root = container;

            // Transform parameter
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            var chargeParameter = serializedObject.FindProperty("m_ChargeParameter");
            MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(root, chargeParameter);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_MaxHorizontalSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_MaxVerticalSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_GroundInfluence"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Jump (Directional) Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_VelocityMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_JumpPowerMode"));
        }
    }
}