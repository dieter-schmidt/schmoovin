using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor (typeof(FpsInventoryKeyDbTable), true)]
    public class FpsInventoryKeyDbTableEditor : Editor
    {
        bool m_Expand = false;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.HelpBox("The FpsInventoryKey constants table maps database entries to the C# constants.", MessageType.Info);

            ++EditorGUI.indentLevel;
            m_Expand = EditorGUILayout.Foldout(m_Expand, "Show Contents", true);
            if (m_Expand)
            {
                var cast = target as FpsInventoryKeyDbTable;
                ++EditorGUI.indentLevel;
                for (int i = 0; i < cast.count; ++i)
                    EditorGUILayout.LabelField(cast.entries[i].displayName, EditorStyles.miniLabel);
                --EditorGUI.indentLevel;
            }
            --EditorGUI.indentLevel;

            serializedObject.ApplyModifiedProperties();
        }
    }
}