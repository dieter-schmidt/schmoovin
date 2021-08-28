using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(MovementState))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgs-movementstate.html")]
    public class MovementStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var container = state.parent.container;
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            InspectMotionData();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Movement Properties", EditorStyles.boldLabel);
            InspectProperties();
        }

        protected virtual void InspectParameters()
        {

        }

        protected virtual void InspectMotionData()
        {
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_TopSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_StrafeMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_ReverseMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_Acceleration"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_Deceleration"));
        }

        protected virtual void InspectProperties()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SlopeSpeedCurve"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GravityMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Damping"));
        }
    }
}