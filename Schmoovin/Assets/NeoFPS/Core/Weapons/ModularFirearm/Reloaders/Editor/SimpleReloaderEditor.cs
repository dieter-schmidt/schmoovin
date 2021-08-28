using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(SimpleReloader))]
    public class SimpleReloaderEditor : BaseReloaderEditor
    {
        protected override void InspectReloaderSettings()
        {
            var delayTypeProp = serializedObject.FindProperty("m_ReloadDelayType");

            EditorGUILayout.PropertyField(delayTypeProp);
            if (delayTypeProp.enumValueIndex == 1)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadDuration"));
                --EditorGUI.indentLevel;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadAnimTrigger"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReloadAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Volume"));
        }
    }
}