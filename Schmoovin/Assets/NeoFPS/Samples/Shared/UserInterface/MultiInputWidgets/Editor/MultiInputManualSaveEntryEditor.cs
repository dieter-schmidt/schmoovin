using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.Samples;

namespace NeoFPSEditor.Samples
{
    [CustomEditor(typeof(MultiInputManualSaveEntry), true)]
    public class MultiInputManualSaveEntryEditor : MultiInputWidgetEditor
    {
        public override void OnChildInspectorGUI()
        {
            EditorGUILayout.LabelField("Manual Save Entry", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ThumbnailImage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TitleText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DateText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Spinner"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DegreesPerSecond"));
        }
    }
}
