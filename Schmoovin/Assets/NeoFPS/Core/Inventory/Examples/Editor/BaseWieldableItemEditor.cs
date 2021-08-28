using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor (typeof (BaseWieldableItem), true)]
    public abstract class BaseWieldableItemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var prop = serializedObject.FindProperty("m_Animator");
            EditorGUILayout.PropertyField(prop);
            if (prop.objectReferenceValue != null)
            {
                prop = serializedObject.FindProperty("m_AnimKeyDraw");
                EditorGUILayout.PropertyField(prop);
                if (!string.IsNullOrWhiteSpace(prop.stringValue))
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DrawDuration"));
                    --EditorGUI.indentLevel;
                }

                prop = serializedObject.FindProperty("m_AnimKeyLower");
                EditorGUILayout.PropertyField(prop);
                if (!string.IsNullOrWhiteSpace(prop.stringValue))
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LowerDuration"));
                    --EditorGUI.indentLevel;
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioSelect"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioDeselect"));

            DrawItemProperties();

            serializedObject.ApplyModifiedProperties();
        }

        protected abstract void DrawItemProperties();
    }
}