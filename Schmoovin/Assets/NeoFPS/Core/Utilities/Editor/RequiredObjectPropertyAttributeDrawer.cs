using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;
using UnityEditor.Animations;

namespace NeoFPSEditor
{
    [CustomPropertyDrawer(typeof(RequiredObjectPropertyAttribute))]
    public class RequiredObjectPropertyAttributeDrawer : PropertyDrawer
    {
        bool m_IsValid;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            m_IsValid = property.objectReferenceValue != null;
            if (m_IsValid)
                return base.GetPropertyHeight(property, label);
            else
                return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            m_IsValid = property.objectReferenceValue != null;
            if (!m_IsValid)
                position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(position, property, label);

            if (!m_IsValid)
            {
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                Color c = GUI.color;
                GUI.color = NeoFpsEditorGUI.errorRed;
                EditorGUI.HelpBox(position, "Required value", MessageType.Error);
                GUI.color = c;
            }

            EditorGUI.EndProperty();
        }
    }
}
