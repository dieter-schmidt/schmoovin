using UnityEngine;
using UnityEditor;
using NeoFPS;
using NeoFPS.Constants;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(FpsKeyBindings), true)]
    public class FpsKeyBindingsEditor : SettingsContextEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.HelpBox("Default key bindings should be set in the NeoFpsInputManager.", MessageType.None);
            EditorGUILayout.Space();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            GUI.enabled = true;
        }
    }
}