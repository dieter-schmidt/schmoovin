using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.Hub.Pages
{
    [CustomEditor(typeof (UpgradeNotesReadme), true)]
    public class UpgradeNotesReadmeEditor : Editor
    {
        private string m_VersionString = "Current";
        private int m_Index = 0;

        private void Awake()
        {
            int version = NeoFpsEditorPrefs.currentNeoFPSVersion;
            if (version != 0)
            {
                var cast = target as UpgradeNotesReadme;
                for (int i = 0; i < cast.updates.Length; ++i)
                {
                    if (cast.updates[i].version == version)
                        m_Index = i;
                }
            }

            GetVersionStringFromIndex();
        }

        protected override void OnHeaderGUI()
        {
            if (!ReadmeEditorUtility.editMode)
            {
                var cast = target as UpgradeNotesReadme;
                ReadmeEditorUtility.DrawReadmeHeader(cast.header, false);
            }
            else
                base.OnHeaderGUI();
        }

        public override void OnInspectorGUI()
        {
            ReadmeEditorUtility.DrawEditModeCheck(null);
            if (!ReadmeEditorUtility.editMode)
                DrawUpdates();
            else
            {
                serializedObject.UpdateIfRequiredOrScript();
                ReadmeEditorUtility.EditReadmeHeader(serializedObject.FindProperty("header"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("updates"), true);
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void LayoutEmbedded()
        {
            var cast = target as UpgradeNotesReadme;
            ReadmeEditorUtility.DrawReadmeHeader(cast.header, true);
            GUILayout.Space(10);
            DrawUpdates();
        }

        void DrawUpdates()
        {
            var cast = target as UpgradeNotesReadme;

            // Get the index
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("From Version");
            if (EditorGUILayout.DropdownButton(new GUIContent(m_VersionString), FocusType.Passive))
            {
                var menu = new GenericMenu();
                for (int i = 0; i < cast.updates.Length; ++i)
                {
                    if (i == cast.updates.Length - 1)
                        menu.AddItem(new GUIContent("Current"), false, OnVersionSelection, i);
                    else
                        menu.AddItem(new GUIContent(cast.updates[i].GetVersionCode()), false, OnVersionSelection, i);
                }
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Mark As Read"))
            {
                m_Index = cast.updates.Length - 1;
                GetVersionStringFromIndex();
                NeoFpsEditorPrefs.currentNeoFPSVersion = cast.updates[m_Index].version;
            }

            bool shown = false;
            if (m_Index >= 0)
            {
                for (int i = m_Index + 1; i < cast.updates.Length; ++i)
                {
                    EditorGUILayout.Space();

                    var previous = cast.updates[i - 1];
                    var update = cast.updates[i];

                    // Draw version number
                    EditorGUILayout.LabelField(string.Format("From {0} to {1}", previous.GetVersionCode(), update.GetVersionCode()), ReadmeEditorUtility.h2Style);
                    GUILayout.Space(4);

                    // Draw sections
                    foreach (var section in update.sections)
                    {
                        ReadmeEditorUtility.DrawReadmeSection(section, EditorGUIUtility.currentViewWidth - 200f);
                    }

                    // Padding
                    GUILayout.Space(10);
                    shown = true;
                }
            }

            if (!shown)
                EditorGUILayout.HelpBox("No upgrades required", MessageType.Info);

            EditorGUILayout.Space();
            NeoFpsEditorGUI.Separator();

            EditorGUILayout.LabelField("Release Notes", ReadmeEditorUtility.titleStyle);
            EditorGUILayout.LabelField("Full release notes are available at the following site:", ReadmeEditorUtility.bodyStyle);
            EditorGUILayout.Space();
            ReadmeEditorUtility.DrawWebLink("NeoFPS Website - Release Notes", "https://neofps.com/releases/");
        }

        void OnVersionSelection (object o)
        {
            m_Index = (int)o;
            GetVersionStringFromIndex();
        }

        void GetVersionStringFromIndex()
        {
            var cast = target as UpgradeNotesReadme;

            // Check if current
            if (m_Index >= cast.updates.Length - 1)
                m_VersionString = "Current";
            else
                m_VersionString = cast.updates[m_Index].GetVersionCode();
        }


    }
}