using NeoFPS.Hub;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.Hub
{
    [CustomEditor(typeof(ReadmeAsset), true)]
    public class ReadmeAssetEditor : Editor
    {
        protected override void OnHeaderGUI()
        {
            if (!ReadmeEditorUtility.editMode)
            {
                var cast = target as ReadmeAsset;
                ReadmeEditorUtility.DrawReadmeHeader(cast.header, false);
            }
            else
                base.OnHeaderGUI();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            ReadmeEditorUtility.DrawEditModeCheck(target as ReadmeAsset);

            if (ReadmeEditorUtility.editMode)
            {
                OnPreEdit();
                EditorGUILayout.Space();
                ReadmeEditorUtility.EditReadmeHeader(serializedObject.FindProperty("m_Header"));
                EditorGUILayout.Space();
                ReadmeEditorUtility.EditReadmeSections(serializedObject.FindProperty("m_Sections"));
                OnPostEdit();
            }
            else
            {
                var cast = target as ReadmeAsset;
                foreach (var section in cast.sections)
                    ReadmeEditorUtility.DrawReadmeSection(section, EditorGUIUtility.currentViewWidth);
            }

            ReadmeEditorUtility.DrawEditModeCheck(target as ReadmeAsset);

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnPreEdit() { }
        protected virtual void OnPostEdit() { }

        public void LayoutEmbedded()
        {
            var cast = target as ReadmeAsset;
            ReadmeEditorUtility.DrawReadmeHeader(cast.header, true);
            GUILayout.Space(10);
            foreach (var section in cast.sections)
                ReadmeEditorUtility.DrawReadmeSection(section, EditorGUIUtility.currentViewWidth - 200f);
        }
    }
}
