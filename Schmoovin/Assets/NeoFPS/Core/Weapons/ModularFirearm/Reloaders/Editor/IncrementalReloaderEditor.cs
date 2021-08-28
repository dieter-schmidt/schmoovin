using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(IncrementalReloader))]
    public class IncrementalReloaderEditor : BaseReloaderEditor
    {
        protected override void InspectReloaderSettings()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanInterrupt"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RoundsPerIncrement"));

            var triggersProp = serializedObject.FindProperty("m_UseExternalTriggers");

            EditorGUILayout.PropertyField(triggersProp);
            if (!triggersProp.boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadStartDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadIncrementDuration"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadIncrementTail"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadEndDuration"));
                --EditorGUI.indentLevel;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadAnimTrigger"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadAnimCountProp"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadAudioStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadAudioIncrement"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadAudioEnd"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Volume"));
        }
    }
}