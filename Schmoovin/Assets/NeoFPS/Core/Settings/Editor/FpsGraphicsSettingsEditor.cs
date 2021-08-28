using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(FpsGraphicsSettings), true)]
    public class FpsGraphicsSettingsEditor : SettingsContextEditor
    {
        GUIContent m_HorizontalFoVLabel = null;

        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.HelpBox("Resolution, fullscreen and vsync settings are initialised on first run based on the Unity player settings.", MessageType.None);
            EditorGUILayout.Space();

            // Script
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            GUI.enabled = true;

            // FoV
            var fovProp = serializedObject.FindProperty("m_VerticalFOV");
            EditorGUILayout.PropertyField(fovProp, new GUIContent("Vertical FoV", fovProp.tooltip));

            if (m_HorizontalFoVLabel == null)
                m_HorizontalFoVLabel = new GUIContent("Horizontal 16:9", "This value is derived from the vertical resolution. Changing it will change the vertical to match.");

            float verticalOld = fovProp.floatValue / 0.5625f;
            float vertical = Mathf.Clamp(EditorGUILayout.DelayedFloatField(m_HorizontalFoVLabel, verticalOld), 40f, 160f);
            if (!Mathf.Approximately(vertical, verticalOld))
                fovProp.floatValue = vertical * 0.5625f;
        }
    }
}
