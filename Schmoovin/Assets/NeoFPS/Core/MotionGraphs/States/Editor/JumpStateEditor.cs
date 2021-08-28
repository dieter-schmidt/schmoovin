using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(JumpState))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgs-jumpstate.html")]
    public class JumpStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var root = container;

            // Transform parameter
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            var chargeParameter = serializedObject.FindProperty("m_ChargeParameter");
            MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(root, chargeParameter);

            // Motion data
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_MaximumHeight"));
            if (chargeParameter.objectReferenceValue != null)
                MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_MinimumHeight"));
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_GroundInfluence"));

            // Properties
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IgnoreFallSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UseInitialGravity"));
        }
    }
}