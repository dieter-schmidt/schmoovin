using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(InteractiveLadderState))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgs-interactiveladderstate.html")]
    public class InteractiveLadderStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            // Transform property
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<TransformParameter>(container, serializedObject.FindProperty("m_TransformParameter"));

            // Motion data
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_ClimbSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_Acceleration"));

            // Vertical aim
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ladder Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UseAimerV"));
            int aimerVIndex = serializedObject.FindProperty("m_UseAimerV").enumValueIndex;
            if (aimerVIndex != 0) // Only show for up/down or smooth
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CenterZone"));

            // Climbing
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DismountDelay"));

            // Camera constraints
            bool constrainFrom = serializedObject.FindProperty("m_ConstrainCamera").boolValue;
            bool constrainTo = EditorGUILayout.Toggle("Constrain Camera", constrainFrom);
            if (constrainFrom != constrainTo)
                serializedObject.FindProperty("m_ConstrainCamera").boolValue = constrainTo;
            if (constrainTo)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LookRange"));
        }
    }
}