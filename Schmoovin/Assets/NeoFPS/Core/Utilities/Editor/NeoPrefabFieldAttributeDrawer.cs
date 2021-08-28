using System;
using UnityEngine;
using UnityEditor;
using NeoFPS;
using System.Collections.Generic;
using System.Collections;

namespace NeoFPSEditor
{
    [CustomPropertyDrawer(typeof(NeoPrefabFieldAttribute))]
    public class NeoPrefabFieldAttributeDrawer : PropertyDrawer
    {
        private static SerializedProperty s_PendingBrowserProperty = null;

        bool m_IsValid;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var castAttr = attribute as NeoPrefabFieldAttribute;
            m_IsValid = !castAttr.required || property.objectReferenceValue != null;
            if (m_IsValid)
                return base.GetPropertyHeight(property, label);
            else
                return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        static bool CheckPrefabHasTypes(GameObject obj, Type[] types)
        {
            if (types != null)
            {
                for (int i = 0; i < types.Length; ++i)
                    if (obj.GetComponent(types[i]) == null)
                        return false;
            }
            return true;
        }

        static bool CheckPrefabHasTypes(Component obj, Type[] types)
        {
            if (types != null)
            {
                for (int i = 0; i < types.Length; ++i)
                    if (obj.GetComponent(types[i]) == null)
                        return false;
            }
            return true;
        }

        static bool IsPrefabFiltered(UnityEngine.Object obj, NeoPrefabFieldAttribute attr, Type fieldType)
        {
            if (obj != null)
            {
                // Check if it's a game object or component
                if (fieldType == typeof(GameObject))
                {
                    var gameobject = obj as GameObject;
                    if (gameobject == null || !CheckPrefabHasTypes(gameobject, attr.filterTypes))
                        return false;
                }
                else
                {
                    var component = obj as Component;
                    if (component == null || !CheckPrefabHasTypes(component, attr.filterTypes))
                        return false;
                }
            }

            return true;
        }

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, prop);

            var castAttr = attribute as NeoPrefabFieldAttribute;
            var fieldType = fieldInfo.FieldType;

            // Check
            if (!IsPrefabFiltered(prop.objectReferenceValue, castAttr, fieldType))
                prop.objectReferenceValue = null;

            // Validity
            m_IsValid = !castAttr.required || prop.objectReferenceValue != null;
            if (!m_IsValid)
                position.height = EditorGUIUtility.singleLineHeight;

            // Show the object field
            if (DrawCustomObjectField(position, prop, fieldType))
            {
                if (fieldType == typeof(GameObject))
                    ProjectHierarchyBrowser.GetPrefab(OnGameObjectPicked, OnObjectPickingCancelled, (obj) => { return CheckPrefabHasTypes(obj, castAttr.filterTypes); });
                else
                    ProjectHierarchyBrowser.GetPrefab(OnGameObjectPicked, OnObjectPickingCancelled, (obj) => { return obj.GetComponent(fieldType) != null && CheckPrefabHasTypes(obj, castAttr.filterTypes); });
            }

            // Draw error
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

        static bool DrawCustomObjectField(Rect position, SerializedProperty prop, Type t)
        {
            bool result = false;

            // Show the object field
            EditorGUI.PrefixLabel(position, new GUIContent(prop.displayName, prop.tooltip));

            position.x += EditorGUIUtility.labelWidth;
            position.width -= EditorGUIUtility.labelWidth;

            // Get the button rect
            var buttonRect = position;
            buttonRect.x += buttonRect.width - 20;
            buttonRect.width = 20;

            // Check for button click and override
            var e = Event.current;
            if (e.isMouse && buttonRect.Contains(Event.current.mousePosition))
            {
                if (e.type == EventType.MouseDown)
                {
                    s_PendingBrowserProperty = prop;
                    result = true;
                }
                Event.current.Use();
            }

            // Show object field
            prop.objectReferenceValue = EditorGUI.ObjectField(position, GUIContent.none, prop.objectReferenceValue, t, true);

            return result;
        }

        void OnGameObjectPicked(GameObject obj)
        {
            if (obj == null)
                s_PendingBrowserProperty.objectReferenceValue = null;
            else
            {
                var t = fieldInfo.FieldType;
                if (t == typeof(GameObject))
                    s_PendingBrowserProperty.objectReferenceValue = obj;
                else
                    s_PendingBrowserProperty.objectReferenceValue = obj.GetComponent(t);
            }

            s_PendingBrowserProperty.serializedObject.ApplyModifiedProperties();
            s_PendingBrowserProperty = null;
        }

        static void OnObjectPickingCancelled()
        {
            s_PendingBrowserProperty = null;
        }
    }
}