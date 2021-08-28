using System;
using UnityEngine;
using UnityEditor;
using NeoFPS;
using System.Collections.Generic;

namespace NeoFPSEditor
{
    [CustomPropertyDrawer(typeof(ComponentOnObjectAttribute))]
    public class ComponentOnObjectAttributeDrawer : PropertyDrawer
    {
        const float k_ButtonWidth = 16f;
        const float k_Spacing = 4f;

        static SerializedProperty s_Property = null;
        static List<Component> s_Components = new List<Component>();

        struct MenuEntry
        {
            public GUIContent label;
            public int index;

            public MenuEntry(string n, int i)
            {
                index = i;
                label = new GUIContent(n);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            //position.width -= k_ButtonWidth + k_Spacing;

            bool valid = false;
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                // Get details
                var castAttr = attribute as ComponentOnObjectAttribute;
                var behaviour = property.serializedObject.targetObject as MonoBehaviour;
                if (behaviour != null)
                {
                    var gameObject = behaviour.gameObject;
                    var current = property.objectReferenceValue;

                    position = EditorGUI.PrefixLabel(position, label);

                    // Get the current component name
                    string currentName = "<Not Selected>";
                    if (current != null)
                        currentName = NeoFpsEditorUtility.GetCurrentComponentName(gameObject, castAttr.componentType, current);
                    else
                    {
                        if (castAttr.required)
                            GUI.color = NeoFpsEditorGUI.errorRed;
                    }
                    
                    // Show the selection dropdown button
                    bool clicked = EditorGUI.DropdownButton(position, new GUIContent(currentName), FocusType.Passive);
                    GUI.color = Color.white;

                    // Show the dropdown if clicked
                    if (clicked)
                    {
                        s_Property = property;
                        ShowComponentsDropdown(gameObject, behaviour);
                    }

                    valid = true;
                }
            }

            if (!valid)
                EditorGUI.PropertyField(position, property, label);

            EditorGUI.EndProperty();
        }

        void ShowComponentsDropdown(GameObject gameObject, UnityEngine.Object owner)
        {
            var castAttr = attribute as ComponentOnObjectAttribute;

            // Get relevant components
            s_Components.Clear();
            gameObject.GetComponents(castAttr.componentType, s_Components);
            if (s_Components.Count < 1)
            {
                s_Property = null;
                return;
            }

            // Get the names for each component
            var entries = new List<MenuEntry>();
            for (int i = 0; i < s_Components.Count; ++i)
            {
                int index = 0;
                int count = 0;                

                var checkType = s_Components[i].GetType();
                for (int j = 0; j < s_Components.Count; ++j)
                {
                    if (s_Components[j].GetType() == checkType)
                    {
                        ++count;
                        if (i == j)
                            index = count;
                    }
                }
                
                if (castAttr.allowSelf || s_Components[i] != owner)
                {
                    if (count <= 1)
                        entries.Add(new MenuEntry(checkType.Name, i));
                    else
                        entries.Add(new MenuEntry(string.Format("{0} ({1})", checkType.Name, index), i));
                }
            }

            // Sort the entries by name
            entries.Sort((lhs, rhs) => { return string.Compare(lhs.label.text, rhs.label.text); });

            // Create the menu and show
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < entries.Count; ++i)
                menu.AddItem(entries[i].label, false, OnComponentSelect, entries[i].index);
            menu.ShowAsContext();
        }

        void OnComponentSelect(object o)
        {
            // Apply the new component
            s_Property.objectReferenceValue = s_Components[(int)o];
            s_Property.serializedObject.ApplyModifiedProperties();

            // Reset
            s_Components.Clear();
            s_Property = null;
        }
    }
}
