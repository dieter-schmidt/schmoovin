using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(DirectionalDashState))]
    //[HelpURL("")]
    public class DirectionalDashStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_DashSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_Acceleration"));

            var cast = target as DirectionalDashState;
            if (!cast.isInstant)
                MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_DashDistance"));
        }
    }
}