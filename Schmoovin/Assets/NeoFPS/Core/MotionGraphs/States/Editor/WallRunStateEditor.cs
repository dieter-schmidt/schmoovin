using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(WallRunState))]
    //[HelpURL("")]
    public class WallRunStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var horizontalMode = serializedObject.FindProperty("m_HorizontalMode");
            var verticalMode = serializedObject.FindProperty("m_VerticalMode");
            var capVertical = serializedObject.FindProperty("m_CapFallSpeed");

            // Transform parameter
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(container, serializedObject.FindProperty("m_WallNormal"));

            // Motion data
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_ClimbGravityMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_FallGravityMultiplier"));

            // Vertical mode dependent properties
            switch ((WallRunState.VerticalStartSpeed)verticalMode.enumValueIndex)
            {
                case WallRunState.VerticalStartSpeed.VerticalBoost:
                    MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_VerticalBoost"));
                    break;
                case WallRunState.VerticalStartSpeed.CappedBoost:
                    MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_VerticalTarget"));
                    MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_VerticalBoost"));
                    break;
                case WallRunState.VerticalStartSpeed.Minimum:
                    MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_VerticalTarget"));
                    break;
                case WallRunState.VerticalStartSpeed.FixedSpeed:
                    MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_VerticalTarget"));
                    break;
            }

            if (capVertical.boolValue)
                MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_MaxFallSpeed"));

            // Horizontal mode dependent properties
            var hModeEnum = (WallRunState.HorizontalSpeed)horizontalMode.enumValueIndex;
            switch (hModeEnum)
            {
                case WallRunState.HorizontalSpeed.MinimumSpeed:
                    MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_HorizontalSpeed"));
                    MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_Acceleration"));
                    break;
                case WallRunState.HorizontalSpeed.TargetSpeed:
                    MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_HorizontalSpeed"));
                    MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_Acceleration"));
                    MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_Deceleration"));
                    break;
            }

            EditorGUILayout.LabelField("Wall Run Properties", EditorStyles.boldLabel);

            // Properties speed
            EditorGUILayout.PropertyField(horizontalMode);
            if (hModeEnum != WallRunState.HorizontalSpeed.MaintainExisting)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HorizontalDamping"));
            EditorGUILayout.PropertyField(verticalMode);
            EditorGUILayout.PropertyField(capVertical);
        }
    }
}