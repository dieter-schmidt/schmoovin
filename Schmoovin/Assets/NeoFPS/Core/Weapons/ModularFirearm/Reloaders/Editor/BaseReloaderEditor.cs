using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    public abstract class BaseReloaderEditor : BaseFirearmModuleBehaviourEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MagazineSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StartingMagazine"));

            InspectReloaderSettings();
        }

        protected abstract void InspectReloaderSettings();
    }
}