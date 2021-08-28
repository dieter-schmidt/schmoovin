using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(ContactLadderState))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgs-contactladderstate.html")]
    public class ContactLadderStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var container = state.parent.container;

            // Transform parameter
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<TransformParameter>(container, serializedObject.FindProperty("m_TransformParameter"));

            // Motion data
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_ClimbSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_GroundSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_StrafeMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_ReverseMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_Acceleration"));

            // Vertical aim
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ladder Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UseAimerV"));
            int aimerVIndex = serializedObject.FindProperty("m_UseAimerV").enumValueIndex;
            if (aimerVIndex == 1 || aimerVIndex == 2 || aimerVIndex == 4) // Only show for absolute, smooth or all axes
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CenterZone"));

            // Horizontal aim
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UseAimerH"));
        }
    }
}