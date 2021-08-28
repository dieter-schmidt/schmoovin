using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(NeoFpsEditorPrefs), true)]
    public class NeoFpsEditorPrefsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowHubOnStart"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
