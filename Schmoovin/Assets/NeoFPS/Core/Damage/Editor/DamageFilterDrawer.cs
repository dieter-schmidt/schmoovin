using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NeoFPS
{
    [CustomPropertyDrawer(typeof(DamageFilter))]
    public class DamageFilterDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            // Calculate rects
            var teamRect = new Rect(
                position.x,// + EditorGUIUtility.labelWidth,
                position.y,
                position.width,// - EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight
                );
            var typeRect = new Rect(
                position.x,// + EditorGUIUtility.labelWidth,
                position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width,// - EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight
                );

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            var prop = property.FindPropertyRelative("m_Value");
            DamageFilter filter = (ushort)prop.intValue;

            DamageTeamFilter team = filter.GetTeamFilter();
            DamageType damageType = filter.GetDamageType();

            DamageTeamFilter newTeam = (DamageTeamFilter)EditorGUI.EnumFlagsField(teamRect, team);
            DamageType newType = (DamageType)EditorGUI.EnumFlagsField(typeRect, damageType);

            if (team != newTeam || damageType != newType)
            {
                DamageFilter newFilter = new DamageFilter(newType, newTeam);
                prop.intValue = newFilter;
            }
            //            prop.intValue = Mathf.Clamp(EditorGUI.IntField(numRect, prop.intValue), 1, 32);
            //            EditorGUI.LabelField(descRect, "[Num Sources]");
            //            EditorGUI.PropertyField(position, property.FindPropertyRelative("m_OutputMixerGroup"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight * 2f) + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}