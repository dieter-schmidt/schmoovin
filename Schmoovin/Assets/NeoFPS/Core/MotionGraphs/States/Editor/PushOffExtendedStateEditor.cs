using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(PushOffExtendedState))]
    //[HelpURL("")]
    public class PushOffExtendedStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(container, serializedObject.FindProperty("m_PushDirection"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_PushUpAngle"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_PushSpeed"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Push-Off Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Additive"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AdditiveVertical"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AdditiveHorizontal"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ClampVerticalPush"));
        }
    }
}