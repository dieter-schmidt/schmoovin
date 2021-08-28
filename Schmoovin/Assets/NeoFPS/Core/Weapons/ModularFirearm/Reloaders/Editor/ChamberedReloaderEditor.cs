using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(ChamberedReloader))]
    public class ChamberedReloaderEditor : BaseReloaderEditor
    {
        protected override void InspectReloaderSettings()
        {
            var delayTypeProp = serializedObject.FindProperty("m_ReloadDelayType");

            EditorGUILayout.PropertyField(delayTypeProp);
            if (delayTypeProp.enumValueIndex == 1)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadDurationEmpty"));
                --EditorGUI.indentLevel;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadAnimTrigger"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EmptyAnimBool"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadAudioEmpty"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Volume"));
        }
    }
}