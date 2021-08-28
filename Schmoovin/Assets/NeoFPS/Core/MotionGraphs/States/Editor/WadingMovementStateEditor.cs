using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(WadingMovementState))]
    //[HelpURL("")]
    public class WadingMovementStateEditor : MovementStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var container = state.parent.container;

            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            InspectParameters();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            InspectMotionData();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Wading Movement Properties", EditorStyles.boldLabel);
            InspectProperties();
        }

        protected override void InspectParameters()
        {
            base.InspectParameters();
            MotionGraphEditorGUI.ParameterDropdownField<TransformParameter>(container, serializedObject.FindProperty("m_WaterZoneParameter"));
        }

        protected override void InspectMotionData()
        {
            base.InspectMotionData();
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_MinSpeedMultiplier"));
        }

        protected override void InspectProperties()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinSpeedDepth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxSpeedDepth"));
            base.InspectProperties();
        }
    }
}