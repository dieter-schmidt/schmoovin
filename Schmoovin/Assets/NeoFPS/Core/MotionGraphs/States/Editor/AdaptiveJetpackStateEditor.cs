using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(AdaptiveJetpackState))]
    //[HelpURL("")]
    public class AdaptiveJetpackStateEditor : MotionGraphStateEditor
    {
        private GUIContent m_ClampSpeedLabel = null;
        private GUIContent m_DampingLabel = null;

        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            var fuelProp = serializedObject.FindProperty("m_JetpackFuel");
            MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(container, fuelProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_JetpackForce"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_HorizontalAcceleration"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_TopSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_StrafeMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_ReverseMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_HorizontalDrag"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Jetpack Properties", EditorStyles.boldLabel);

            var modeProp = serializedObject.FindProperty("m_Mode");
            EditorGUILayout.PropertyField(modeProp);
            switch (modeProp.enumValueIndex)
            {
                case 0:
                    {
                        if (fuelProp.objectReferenceValue != null)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FuelBurnRate"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinFuelBurn"));
                        }

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxVerticalSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SpeedFalloff"));
                    }
                    break;
                case 1:
                    {
                        if (fuelProp.objectReferenceValue != null)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FuelBurnRate"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FuelDamping"));
                        }

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxVerticalSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Hysteresis"));
                    }
                    break;
            }

            var clampHorizProp = serializedObject.FindProperty("m_ClampSpeed");
            if (m_ClampSpeedLabel == null)
                m_ClampSpeedLabel = new GUIContent("Clamp Horizontal Velocity", clampHorizProp.tooltip);
            EditorGUILayout.PropertyField(clampHorizProp, m_ClampSpeedLabel);

            var dampingProp = serializedObject.FindProperty("m_Damping");
            if (m_DampingLabel == null)
                m_DampingLabel = new GUIContent("Horizontal Damping", dampingProp.tooltip);
            EditorGUILayout.PropertyField(dampingProp, m_DampingLabel);
        }
    }
}