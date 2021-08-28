using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using UnityEditor.Animations;

namespace NeoFPSEditor.CharacterMotion
{
    [CustomPropertyDrawer(typeof(MotionGraphParameterKeyAttribute))]
    public class MotionGraphParameterKeyDrawer : PropertyDrawer
    {
        private static Texture2D m_FoundTexture = null;
        private static Texture2D m_NotFoundTexture = null;
        string m_Error = null;
        bool m_IsValid = false;

        public static Texture2D foundTexture
        {
            get
            { 
                if (m_FoundTexture == null)
                    m_FoundTexture = EditorGUIUtility.Load("CollabNew") as Texture2D;
                return m_FoundTexture;
            }
        }

        public static Texture2D notFoundTexture
        {
            get
            {
                if (m_NotFoundTexture == null)
                    m_NotFoundTexture = EditorGUIUtility.Load("ColorPicker-2DThumb") as Texture2D;
                return m_NotFoundTexture;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            m_IsValid = CheckValid(property, out m_Error);

            if (m_Error != null)
                position.height = EditorGUIUtility.singleLineHeight;

            position.width -= EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(position, property, label);

            position.x += position.width;
            position.width = EditorGUIUtility.singleLineHeight;

            if (m_IsValid)
                GUI.Label(position, foundTexture);
            else
                GUI.Label(position, notFoundTexture);

            if (m_Error != null)
            {
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                Color c = GUI.color;
                GUI.color = NeoFpsEditorGUI.errorRed;
                EditorGUI.HelpBox(position, m_Error, MessageType.Error);
                GUI.color = c;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            m_IsValid = CheckValid(property, out m_Error);
            if (m_Error == null)
                return base.GetPropertyHeight(property, label);
            else
                return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        bool CheckValid(SerializedProperty property, out string error)
        {
            // Do nothing in play mode
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                error = null;
                return true;
            }

            // Check property is the correct type
            if (property.propertyType != SerializedPropertyType.String)
            {
                error = "Can only use MotionGraphParameterKey attributes on string properties.";
                return false;
            }

            // Empty is always valid (should it be?)
            if (string.IsNullOrEmpty(property.stringValue))
            {
                error = null;
                return true;
            }

            // Get cast attribute
            var castAttribute = attribute as MotionGraphParameterKeyAttribute;
            
            // Check if inspecting Monobehaviour
            var monobehaviour = property.serializedObject.targetObject as MonoBehaviour;
            if (monobehaviour == null)
            {
                error = "MotionGraphParameterKey attributes are MonoBehaviour only.";
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
            SerializedProperty parameters = so.FindProperty("m_Parameters");
            for (int i = 0; i < parameters.arraySize; ++i)
            {
                var p = parameters.GetArrayElementAtIndex(i);
                var obj = p.objectReferenceValue;
                if (obj != null)
                {
                    bool correctType = false;
                    switch (castAttribute.parameterType)
                    {
                        case MotionGraphParameterType.Float:
                            correctType = (obj is FloatParameter);
                            break;
                        case MotionGraphParameterType.Int:
                            correctType = (obj is IntParameter);
                            break;
                        case MotionGraphParameterType.Trigger:
                            correctType = (obj is TriggerParameter);
                            break;
                        case MotionGraphParameterType.Switch:
                            correctType = (obj is SwitchParameter);
                            break;
                        case MotionGraphParameterType.Transform:
                            correctType = (obj is TransformParameter);
                            break;
                        case MotionGraphParameterType.Vector:
                            correctType = (obj is VectorParameter);
                            break;
                        case MotionGraphParameterType.Event:
                            correctType = (obj is EventParameter);
                            break;
                    }
                    
                    if (correctType && obj.name == property.stringValue)
                    {
                        error = null;
                        return true;
                    }
                }
                else
                    Debug.Log("Null parameter");
            }

            // None found
            error = null;
            return false;
        }
    }
}