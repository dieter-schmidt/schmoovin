using NeoFPS.Hub;
using UnityEditor;

namespace NeoFPSEditor.Hub
{
    [CustomEditor(typeof(ReadmeBehaviour), true)]
    public class ReadmeBehaviourEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            if (ReadmeEditorUtility.DrawEditModeCheck(target as ReadmeBehaviour))
            {
                EditorGUILayout.Space();
                ReadmeEditorUtility.EditReadmeHeader(serializedObject.FindProperty("m_Header"));
                EditorGUILayout.Space();
                ReadmeEditorUtility.EditReadmeSections(serializedObject.FindProperty("m_Sections"));
            }
            else
            {
                var cast = target as ReadmeBehaviour;
                ReadmeEditorUtility.DrawReadmeHeader(cast.header, true);
                foreach (var section in cast.sections)
                    ReadmeEditorUtility.DrawReadmeSection(section, EditorGUIUtility.currentViewWidth);
            }

            ReadmeEditorUtility.DrawEditModeCheck(target as ReadmeAsset);

            serializedObject.ApplyModifiedProperties();
        }

        public void LayoutEmbedded()
        {
            var cast = target as ReadmeBehaviour;
            ReadmeEditorUtility.DrawReadmeHeader(cast.header, true);
            foreach (var section in cast.sections)
                ReadmeEditorUtility.DrawReadmeSection(section, EditorGUIUtility.currentViewWidth - 200);
        }
    }
}
