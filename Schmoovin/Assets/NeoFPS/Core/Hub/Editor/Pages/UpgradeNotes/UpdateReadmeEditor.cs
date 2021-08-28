using NeoFPS.Hub;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.Hub.Pages
{
    [CustomEditor(typeof(UpdateReadme), true)]
    public class UpdateReadmeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            // Draw version number
            var version = serializedObject.FindProperty("version");
            EditorGUILayout.PropertyField(version);

            // Output formatted version code
            {
                int number = version.intValue;
                int major = number / 1000;
                int minor = (number / 100) % 10;
                int revision = number % 100;
                EditorGUILayout.HelpBox(string.Format("Version code: {0}.{1}.{2:D2}", major, minor, revision), MessageType.Info);
            }

            // Draw sections
            var sections = serializedObject.FindProperty("sections");
            ReadmeEditorUtility.EditReadmeSections(sections);

            serializedObject.ApplyModifiedProperties();
        }
    }
}