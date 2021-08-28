using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(RepulseState))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgs-repulsestate.html")]
    public class RepulseStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            // Transform property
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<TransformParameter>(container, serializedObject.FindProperty("m_RepulsorTransform"));

            // Multiplier
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_RepulseMultiplier"));

            // Vector
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Repulse Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_NullifyTransform"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RepulsionVector"));
        }
    }
}