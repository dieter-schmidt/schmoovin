using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(StandardShellEject))]
    public class StandardShellEjectEditor : BaseFirearmModuleBehaviourEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShellEjectProxy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShellPrefab"));

            var delayTypeProp = serializedObject.FindProperty("m_DelayType");
            EditorGUILayout.PropertyField(delayTypeProp);
            if (delayTypeProp.enumValueIndex == 1)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Delay"));
                --EditorGUI.indentLevel;
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OutSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BackSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InheritVelocity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AngularVelocityA"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AngularVelocityB"));
        }
    }
}