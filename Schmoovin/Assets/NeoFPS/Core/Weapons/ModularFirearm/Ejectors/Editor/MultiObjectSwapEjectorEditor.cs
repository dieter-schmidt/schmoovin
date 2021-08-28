using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(MultiObjectSwapEjector))]
    public class MultiObjectSwapEjectorEditor : BaseFirearmModuleBehaviourEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var ejectOnFireProp = serializedObject.FindProperty("m_EjectOnFire");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TargetTransforms"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShellPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SwapInactive"));
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