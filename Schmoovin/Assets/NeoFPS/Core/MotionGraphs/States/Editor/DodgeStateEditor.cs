using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(DodgeState))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgs-dodgestate.html")]
    public class DodgeStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var root = container;

            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<IntParameter>(root, serializedObject.FindProperty("m_DodgeDirectionParameter"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_VerticalSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_HorizontalSpeed"));
        }
    }
}