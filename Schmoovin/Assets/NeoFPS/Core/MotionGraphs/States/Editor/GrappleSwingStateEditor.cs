using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPSEditor.CharacterMotion;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(GrappleSwingState))]
    public class GrappleSwingStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(container, serializedObject.FindProperty("m_GrapplePoint"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grapple Swing Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TargetDistanceMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AccelerationPerMeter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxAccel"));
        }
    }
}