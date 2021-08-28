using System;
using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomPropertyDrawer(typeof(NeoObjectInHierarchyFieldAttribute))]
    public class NeoObjectInHierarchyFieldAttributeDrawer : PropertyDrawer
    {
        private static SerializedProperty s_PendingBrowserProperty = null;

        bool m_IsValid;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var castAttr = attribute as NeoObjectInHierarchyFieldAttribute;
            m_IsValid = !castAttr.required || property.objectReferenceValue != null;
            if (m_IsValid)
                return base.GetPropertyHeight(property, label);
            else
                return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        static Transform GetRootTransform(NeoObjectInHierarchyFieldAttribute attr, SerializedProperty prop)
        {
            if (attr.rootProperty == null)
            {
                var target = prop.serializedObject.targetObject;
                var component = target as Component;
                if (component != null)
                    return component.transform;
                else
                    return null;
            }
            else
            {
                switch (attr.rootPropertyType)
                {
                    case RootPropertyType.Transform:
                        {
                            // For relative, does the following work?
                            //var targetProp = prop.FindPropertyRelative("../attr.rootProperty");
                            var targetProp = prop.serializedObject.FindProperty(attr.rootProperty);
                            if (prop == null)
                                return null;
                            return prop.objectReferenceValue as Transform;
                        }
                    case RootPropertyType.GameObject:
                        {
                            var targetProp = prop.serializedObject.FindProperty(attr.rootProperty);
                            if (prop == null)
                                return null;
                            var gameObject = prop.objectReferenceValue as GameObject;
                            if (gameObject != null)
                                return gameObject.transform;
                            else
                                return null;
                        }
                    case RootPropertyType.Component:
                        {
                            var targetProp = prop.serializedObject.FindProperty(attr.rootProperty);
                            if (prop == null)
                                return null;
                            var component = prop.objectReferenceValue as Component;
                            if (component != null)
                                return component.transform;
                            else
                                return null;
                        }
                }
            }

            return null;
        }

        static bool IsChildFiltered(UnityEngine.Object obj, Transform root, NeoObjectInHierarchyFieldAttribute attr, Type fieldType)
        {
            if (obj != null)
            {
                if (fieldType == typeof(GameObject))
                {
                    var gameobject = obj as GameObject;
                    if (gameobject == null || (attr.filter != null && !attr.filter(gameobject)) || !gameobject.transform.IsChildOf(root) || (!attr.allowRoot && gameobject.transform == root))
                        return false;
                }
                else
                {
                    var component = obj as Component;
                    if (component == null || (attr.filter != null && !attr.filter(component.gameObject)) || !component.transform.IsChildOf(root) || (!attr.allowRoot && component.transform == root))
                        return false;
                }
            }
            return true;
        }

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, prop);

            var castAttr = attribute as NeoObjectInHierarchyFieldAttribute;
            var obj = prop.objectReferenceValue as GameObject;
            var root = GetRootTransform(castAttr, prop);
            var fieldType = fieldInfo.FieldType;

            // Check
            if (!IsChildFiltered(prop.objectReferenceValue, root, castAttr, fieldType))
                prop.objectReferenceValue = null;

            // Validity
            m_IsValid = !castAttr.required || prop.objectReferenceValue != null;
            if (!m_IsValid)
                position.height = EditorGUIUtility.singleLineHeight;

            // Show the object field
            if (DrawCustomObjectField(position, prop, fieldType))
            {
                if (fieldType == typeof(GameObject))
                    ObjectHierarchyBrowser.GetChildObject(root, castAttr.allowRoot, OnGameObjectPicked, OnObjectPickingCancelled, castAttr.filter);
                else
                {
                    ObjectHierarchyBrowser.GetChildObject(root, castAttr.allowRoot, OnGameObjectPicked, OnObjectPickingCancelled, (o) =>
                    {
                        return o != null && o.GetComponent(fieldType) != null && (castAttr.filter == null || castAttr.filter(o));
                    });
                }
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