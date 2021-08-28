using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor.ScriptGeneration
{
    public class ConstantsGenerator
    {
        GeneratorConstantsList m_List = null;
        private string[] m_ReservedNames = null;
        private string m_ReservedNamesMessage = null;
        private SerializedProperty m_ErrorProperty = null;
        private SerializedProperty m_DirtyProperty = null;

        public ConstantsGenerator(SerializedObject so, string listProperty, string dirtyProperty, string errorProperty = null, string[] reserved = null)
        {
            // Get relevant properties
            m_DirtyProperty = so.FindProperty(dirtyProperty);
            if (!string.IsNullOrEmpty(errorProperty))
                m_ErrorProperty = so.FindProperty(errorProperty);

            // Create the reorderable list
            m_List = new GeneratorConstantsList (
                "Constants",
                so.FindProperty(listProperty),
                m_DirtyProperty,
                m_ErrorProperty,
                reserved
                );

            // Store the reserved names
            if (reserved != null && reserved.Length > 0)
            {
                m_ReservedNames = reserved;
                BuildReservedNamesMessage(reserved);
            }
        }

        void BuildReservedNamesMessage(string[] reserved)
        {
            string message = "The following button names are reserved:";
            foreach (var r in reserved)
                message += "\n• " + r;
            m_ReservedNamesMessage = message;
        }

        public bool DoLayoutGenerator()
        {
            bool result = false;

            // Show reserved names
            if (m_ReservedNamesMessage != null)
                EditorGUILayout.HelpBox(m_ReservedNamesMessage, MessageType.None);

            // Draw the list
            m_List.DoLayoutList();
            EditorGUILayout.Space();

            // Get buttons state
            var state = (GeneratorConstantsState)m_ErrorProperty.intValue;

            // Show buttons state
            switch (state)
            {
                case GeneratorConstantsState.Valid:
                    EditorGUILayout.HelpBox("Constants are valid and up to date.", MessageType.Info);
                    break;
                case GeneratorConstantsState.RequiresRebuild:
                    EditorGUILayout.HelpBox("Constants settings have changed and need generating.", MessageType.Warning);
                    break;
                default:
                    string message = "The following errors were found:";
                    if ((state & GeneratorConstantsState.NameValidErrors) != GeneratorConstantsState.Valid)
                        message += "\n- One or more constant names are not valid.";
                    if ((state & GeneratorConstantsState.NameDuplicateErrors) != GeneratorConstantsState.Valid)
                        message += "\n- Duplicate constant names were found.";
                    if ((state & GeneratorConstantsState.NameReservedErrors) != GeneratorConstantsState.Valid)
                        message += "\n- One or more constant names is reserved.";
                    EditorGUILayout.HelpBox(message, MessageType.Error);
                    break;
            }

            // Disable if unchanged or invalid
            if (state != GeneratorConstantsState.RequiresRebuild)
                GUI.enabled = false;

            // Generate FpsInputButton Constants
            if (GUILayout.Button("Generate Constants Scripts"))
                result = true;

            GUI.enabled = true;
            EditorGUILayout.Space();

            return result;
        }

        public void GenerateConstants(SerializedProperty folderObjectProp, string constantsName, string source)
        {
            m_DirtyProperty.boolValue = false;

            int reservedCount = 0;
            if (m_ReservedNames != null)
                reservedCount += m_ReservedNames.Length;

            // Get the lines as an array
            var linesProp = m_List.serializedProperty;
            var lineCount = linesProp.arraySize + reservedCount;
            string[] lines = new string[lineCount];
            int i = 0;
            for (; i < reservedCount; ++i)
                lines[i] = m_ReservedNames[i];
            for (; i < lineCount; ++i)
                lines[i] = linesProp.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;

            // Apply lines
            source = ScriptGenerationUtilities.ReplaceKeyword(source, "NAME", constantsName);
            source = ScriptGenerationUtilities.InsertMultipleLines(source, "VALUES", "\t\tpublic const int {0} = {1};", lines, "COUNT");
            source = ScriptGenerationUtilities.InsertMultipleLines(source, "VALUE_NAMES", (int index) =>
            {
                if (index >= lines.Length)
                    return null;
                if (index == lines.Length - 1)
                    return string.Format("\t\t\t\"{0}\"", lines[index]);
                else
                    return string.Format("\t\t\t\"{0}\",", lines[index]);
            });

            // Write the script
            ScriptGenerationUtilities.WriteScript(ScriptGenerationUtilities.GetFullScriptPath(folderObjectProp, constantsName), source, true);
        }
    }
}