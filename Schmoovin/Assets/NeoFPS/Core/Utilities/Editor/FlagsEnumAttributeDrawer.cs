using UnityEngine;
using UnityEditor;
using NeoFPS;
using System;

namespace NeoFPSEditor
{
    [CustomPropertyDrawer(typeof(FlagsEnumAttribute))]
    public class FlagsEnumAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            //property.intValue = EditorGUI.EnumFlagsField(position, new GUIContent(property.displayName, property.tooltip), (Enum)property.intValue);
            property.intValue = EditorGUI.MaskField(position, new GUIContent(property.displayName, property.tooltip), property.intValue, property.enumNames);

            EditorGUI.EndProperty();
        }
    }
}
