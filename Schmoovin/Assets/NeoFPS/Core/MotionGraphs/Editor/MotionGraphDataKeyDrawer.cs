using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.MotionData;
using UnityEditor.Animations;

namespace NeoFPSEditor.CharacterMotion
{
    [CustomPropertyDrawer(typeof(MotionGraphDataKeyAttribute))]
    public class MotionGraphDataKeyDrawer : PropertyDrawer
    {
        string m_Error;
        bool m_IsValid;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            m_IsValid = CheckValid(property, out m_Error);
            if (m_IsValid)
                return base.GetPropertyHeight(property, label);
            else
                return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            m_IsValid = CheckValid(property, out m_Error);
            if (!m_IsValid)
                position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(position, property, label);

            if (!m_IsValid)
            {
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                Color c = GUI.color;
                GUI.color = NeoFpsEditorGUI.errorRed;
                EditorGUI.HelpBox(position, m_Error, MessageType.Error);
                GUI.color = c;
            }

            EditorGUI.EndProperty();
        }

        bool CheckValid(SerializedProperty property, out string error)
        {
            // Do nothing in play mode
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                error = string.Empty;
                return true;
            }

            // Check property is the correct type
            if (property.propertyType != SerializedPropertyType.String)
            {
                error = "Can only use MotionGraphDataKey attributes on string properties.";
                return false;
            }

            // Empty is always valid (should it be?)
            if (string.IsNullOrEmpty(property.stringValue))
            {
                error = string.Empty;
                return true;
            }

            // Get cast attribute
            var castAttribute = attribute as MotionGraphDataKeyAttribute;
            
            // Check if inspecting Monobehaviour
            var monobehaviour = property.serializedObject.targetObject as MonoBehaviour;
            if (monobehaviour == null)
            {
                error = "MotionGraphDataKey attributes are MonoBehaviour only.";
                return false;
            }

            // Get animator (on object if property name is not specified)
            MotionController controller = monobehaviour.GetComponent<MotionController>();
            if (controller == null)
            {
                error = "No motion controller found on object.";
                return false;
            }
            if (controller.motionGraph == null)
            {
                error = "Motion controller does not have a motion graph asset assigned.";
                return false;
            }

            var so = new SerializedObject(controller.motionGraph);

            // Get parameters based on type
            SerializedProperty parameters = so.FindProperty("m_Data");
            for (int i = 0; i < parameters.arraySize; ++i)
            {
                var p = parameters.GetArrayElementAtIndex(i);
                var obj = p.objectReferenceValue;
                if (obj != null)
                {
                    bool correctType = false;
                    switch (castAttribute.dataType)
                    {
                        case MotionGraphDataType.Float:
                            correctType = (obj is FloatData);
                            break;
                        case MotionGraphDataType.Int:
                            correctType = (obj is IntData);
                            break;
                        case MotionGraphDataType.Bool:
                            correctType = (obj is BoolData);
                            break;
                    }
                    
                    if (correctType && obj.name == property.stringValue)
                    {
                        error = string.Empty;
                        return true;
                    }
                }
                else
                    Debug.Log("Null parameter");
            }

            // None found
            error = "Motion data with matching name not found.";
            return false;
        }
    }
}