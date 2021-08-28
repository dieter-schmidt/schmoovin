using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;
using NeoFPS.Samples;

namespace NeoFPSEditor.Samples
{
    [CustomEditor(typeof(OptionsMenuInputBindings), true)]
    public class OptionsMenuInputBindingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ContainerTransform"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_KeyboardLayoutMultiChoice"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ResetToDefaultsButton"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PrototypeDivider"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PrototypeBinding"));
            
            var expand = serializedObject.FindProperty("expandRebindableButtons"); 
            var filter = serializedObject.FindProperty("m_RebindableButtons");

            int ignoreButtonCount = NeoFpsInputManager.fixedInputButtons.Length;

            expand.boolValue = EditorGUILayout.Foldout(expand.boolValue, "Filter Rebindable Buttons", true);
            if (expand.boolValue)
            {
                ++EditorGUI.indentLevel;
                if (filter.arraySize != FpsInputButton.count - ignoreButtonCount)
                    NeoFpsEditorGUI.MiniError("Incorrect number of inputs in filter");
                else
                {
                    for (int i = 0; i < filter.arraySize; ++i)
                    {
                        var button = filter.GetArrayElementAtIndex(i);
                        int offset = i + ignoreButtonCount;

                        button.boolValue = EditorGUILayout.Toggle(FpsInputButton.names[offset], button.boolValue);
                    }
                }
                --EditorGUI.indentLevel;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}