using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.Samples;

namespace NeoFPSEditor.Samples
{
    [CustomEditor(typeof(MultiInputSaveBrowserEntry), true)]
    public class MultiInputSaveBrowserEntryEditor : MultiInputWidgetEditor
    {
        public override void OnChildInspectorGUI()
        {
            EditorGUILayout.LabelField("Save Browser Entry", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ThumbnailImage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TypeText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DateText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LoadButton"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Spinner"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DegreesPerSecond"));
        }
    }
}
