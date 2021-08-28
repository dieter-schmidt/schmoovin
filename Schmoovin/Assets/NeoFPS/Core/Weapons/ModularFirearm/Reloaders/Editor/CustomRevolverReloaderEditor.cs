using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(CustomRevolverReloader))]
    public class CustomRevolverReloaderEditor : SimpleReloaderEditor
    {
        protected override void InspectReloaderSettings()
        {
            base.InspectReloaderSettings();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EmptyShells"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LoaderShells"), true);
        }
    }
}