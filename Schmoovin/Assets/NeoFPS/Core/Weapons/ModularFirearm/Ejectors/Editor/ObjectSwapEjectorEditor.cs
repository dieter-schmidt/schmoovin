using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(ObjectSwapEjector))]
    public class ObjectSwapEjectorEditor : BaseFirearmModuleBehaviourEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var ejectOnFireProp = serializedObject.FindProperty("m_EjectOnFire");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TargetTransform"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShellPrefab"));
            EditorGUILayout.PropertyField(ejectOnFireProp);
            if (!ejectOnFireProp.boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Delay"));
                --EditorGUI.indentLevel;
            }
        }
    }
}