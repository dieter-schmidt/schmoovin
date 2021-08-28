using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(MantleState))]
    //[HelpURL("")]
    public class MantleStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(container, serializedObject.FindProperty("m_WallNormal"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_WallCheckDistance"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_ClimbSpeed"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mantling Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_WallCollisionMask"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StartingSpeedMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EndingSpeedMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OvershootDistance"));
        }
    }
}