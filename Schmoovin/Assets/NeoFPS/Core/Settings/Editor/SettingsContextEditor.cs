#if UNITY_STANDALONE // Should other platforms use Json text files saved to disk?
#define SETTINGS_USES_JSON
#endif

using UnityEngine;
using UnityEditor;
using NeoFPS;
using NeoFPS.Constants;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(SettingsContextBase), true)]
    public class SettingsContextEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("These settings act as a template and will be overridden by the user settings files that are created when the game is run.", MessageType.Info);
            if (GUILayout.Button("Delete User Settings File"))
            {
                var sc = target as SettingsContextBase;
                sc.DeleteSaveFile();
            }
            EditorGUILayout.Space();

            OnInspectorGUIInternal();
        }

        protected virtual void OnInspectorGUIInternal()
        {
            base.OnInspectorGUI();
        }
    }
}