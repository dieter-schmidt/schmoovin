using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(ImpulseState))]
    //[HelpURL("")]
    public class ImpulseStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(container, serializedObject.FindProperty("m_Impulse"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Impulse Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FrameOfReference"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ImpulseMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GroundConstrained"));
        }
    }
}