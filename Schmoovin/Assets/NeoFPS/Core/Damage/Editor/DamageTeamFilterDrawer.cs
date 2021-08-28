using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NeoFPS
{
    [CustomPropertyDrawer(typeof(DamageTeamFilter))]
    public class DamageTeamFilterDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            
            DamageTeamFilter oldValue = (DamageTeamFilter)property.intValue;
            DamageTeamFilter newValue = (DamageTeamFilter)EditorGUI.EnumFlagsField(position, property.displayName, (DamageTeamFilter)property.intValue);
            if (oldValue != newValue)
                property.intValue = (int)newValue;
            
            EditorGUI.EndProperty();
        }
    }
}