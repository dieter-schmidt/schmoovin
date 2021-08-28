using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;
using UnityEditor.Animations;

namespace NeoFPSEditor
{
    [CustomPropertyDrawer(typeof(AnimatorParameterKeyAttribute))]
    public class AnimatorParameterKeyDrawer : PropertyDrawer
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
                error = "Can only use AnimatorParameterKey attributes on string properties.";
                return false;
            }

            // Empty is always valid (should it be?)
            if (string.IsNullOrEmpty(property.stringValue))
            {
                error = string.Empty;
                return true;
            }

            // Get cast attribute
            var castAttribute = attribute as AnimatorParameterKeyAttribute;

            // Get animator (on object if property name is not specified)
            Animator animator = null;
            if (castAttribute.animatorSource == AnimatorParameterKeyAttribute.AnimatorSource.Property)
            {
                // Check property name is valid
                if (string.IsNullOrEmpty(castAttribute.animatorProperty))
                {
                    error = "Invalid property specified by name.";
                    return false;
                }

                // Check property exists
                // This should probably be adapted to account for nested properties
                // Something like get the current property path, step up one and then apply animator path
                var animatorProp = property.serializedObject.FindProperty(castAttribute.animatorProperty);
                if (animatorProp == null)
                {
                    error = "No animator property with name found.";
                    return false;
                }

                // Check if property has animator assigned
                animator = animatorProp.objectReferenceValue as Animator;
                if (animator == null)
                {
                    error = "Animator property is null.";
                    return false;
                }
            }
            else
            {
                // Check if inspecting Monobehaviour
                var monobehaviour = property.serializedObject.targetObject as MonoBehaviour;
                if (monobehaviour == null)
                {
                    error = "AnimatorParameterKey attributes are MonoBehaviour only.";
                    return false;
                }

                // Find animator in heirarchy
                switch (castAttribute.animatorSource)
                {
                    case AnimatorParameterKeyAttribute.AnimatorSource.GameObject:
                        animator = monobehaviour.GetComponent<Animator>();
                        break;
                    case AnimatorParameterKeyAttribute.AnimatorSource.ChildObject:
                        animator = monobehaviour.GetComponentInChildren<Animator>(true);
                        break;
                    case AnimatorParameterKeyAttribute.AnimatorSource.Parent:
                        animator = monobehaviour.GetComponentInParent<Animator>();
                        break;
                    case AnimatorParameterKeyAttribute.AnimatorSource.FullHeirarchy:
                        {
                            animator = monobehaviour.GetComponentInChildren<Animator>(true);
                            if (animator == null)
                                animator = monobehaviour.GetComponentInParent<Animator>();
                        }
                        break;
                }
                if (animator == null)
                {
                    error = "No animator found in heirarchy.";
                    return false;
                }
            }
            
            SerializedObject animatorSO = new SerializedObject(animator);
            var controllerProp = animatorSO.FindProperty("m_Controller");
            var controller = controllerProp.objectReferenceValue as AnimatorController;
            if (controller == null)
            {
                var overrideController = controllerProp.objectReferenceValue as AnimatorOverrideController;
                if (overrideController != null)
                {
                    controller = overrideController.runtimeAnimatorController as AnimatorController;
                    if (controller == null)
                    {
                        error = "Animator has an override controller assigned that does not override a specific animator controller.";
                        return false;
                    }
                }
                else
                {
                    error = "Animator has no controller assigned.";
                    return false;
                }
            }

            // Check through parameters for correct name and type
            var parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; ++i)
            {
                var p = parameters[i];
                if (p.type == castAttribute.parameterType && p.name == property.stringValue)
                {
                    error = string.Empty;
                    return true;
                }
            }

            // None found
            error = "Parameter with matching name not found.";
            return false;
        }

        public static bool CheckValid(AnimatorParameterKeyAttribute attribute, SerializedProperty prop)
        {
            // Check property is the correct type
            if (prop.propertyType != SerializedPropertyType.String)
                return false;

            // Empty is always valid (should it be?)
            if (string.IsNullOrEmpty(prop.stringValue))
                return true;

            // Get animator (on object if property name is not specified)
            Animator animator = null;
            if (attribute.animatorSource == AnimatorParameterKeyAttribute.AnimatorSource.Property)
            {
                // Check property name is valid
                if (string.IsNullOrEmpty(attribute.animatorProperty))
                    return false;

                // Check property exists
                // This should probably be adapted to account for nested properties
                // Something like get the current property path, step up one and then apply animator path
                var animatorProp = prop.serializedObject.FindProperty(attribute.animatorProperty);
                if (animatorProp == null)
                    return false;

                // Check if property has animator assigned
                animator = animatorProp.objectReferenceValue as Animator;
                if (animator == null)
                    return false;
            }
            else
            {
                // Check if inspecting Monobehaviour
                var monobehaviour = prop.serializedObject.targetObject as MonoBehaviour;
                if (monobehaviour == null)
                    return false;

                // Find animator in heirarchy
                switch (attribute.animatorSource)
                {
                    case AnimatorParameterKeyAttribute.AnimatorSource.GameObject:
                        animator = monobehaviour.GetComponent<Animator>();
                        break;
                    case AnimatorParameterKeyAttribute.AnimatorSource.ChildObject:
                        animator = monobehaviour.GetComponentInChildren<Animator>(true);
                        break;
                    case AnimatorParameterKeyAttribute.AnimatorSource.Parent:
                        animator = monobehaviour.GetComponentInParent<Animator>();
                        break;
                    case AnimatorParameterKeyAttribute.AnimatorSource.FullHeirarchy:
                        {
                            animator = monobehaviour.GetComponentInChildren<Animator>(true);
                            if (animator == null)
                                animator = monobehaviour.GetComponentInParent<Animator>();
                        }
                        break;
                }
                if (animator == null)
                    return false;
            }

            SerializedObject animatorSO = new SerializedObject(animator);
            var controllerProp = animatorSO.FindProperty("m_Controller");
            var controller = controllerProp.objectReferenceValue as AnimatorController;
            if (controller == null)
            {
                var overrideController = controllerProp.objectReferenceValue as AnimatorOverrideController;
                if (overrideController != null)
                {
                    controller = overrideController.runtimeAnimatorController as AnimatorController;
                    if (controller == null)
                        return false;
                }
                else
                    return false;
            }

            // Check through parameters for correct name and type
            var parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; ++i)
            {
                var p = parameters[i];
                if (p.type == attribute.parameterType && p.name == prop.stringValue)
                    return true;
            }

            // None found
            return false;
        }
    }
}