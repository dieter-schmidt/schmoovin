using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NeoFPS
{
    [CustomPropertyDrawer(typeof(DamageType))]
    public class DamageTypeDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            DamageType oldValue = (DamageType)property.intValue;
            DamageType newValue = (DamageType)EditorGUI.EnumFlagsField(position, property.displayName, oldValue);
            if (oldValue != newValue)
                property.intValue = (int)newValue;

            EditorGUI.EndProperty();
        }
    }
}