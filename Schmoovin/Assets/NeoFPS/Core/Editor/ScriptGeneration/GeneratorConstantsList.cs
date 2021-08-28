using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace NeoFPSEditor.ScriptGeneration
{
    public class GeneratorConstantsList : BaseReorderableList
    {
        private string m_Heading = string.Empty;
        private string[] m_ReservedNames = null;
        private SerializedProperty m_ErrorProperty = null;
        private SerializedProperty m_DirtyProperty = null;

        public GeneratorConstantsState state
        {
            get;
            private set;
        }
        
        public GeneratorConstantsList(string title, SerializedProperty prop, SerializedProperty dirtyProp, SerializedProperty errorProp = null, string[] reserved = null) : base(prop)
        {
            m_Heading = title;
            m_ErrorProperty = errorProp;
            m_DirtyProperty = dirtyProp;

            // Set reserved names (don't allow zero length, as it adds overhead for nothing)
            if (reserved != null && reserved.Length > 0)
                m_ReservedNames = reserved;
        }

        protected override string heading
        {
            get { return m_Heading; }
        }

        protected override void DrawListElement(Rect line1, int index)
        {
            // Draw index as label
            string label = index.ToString();
            if (index == 0)
                label += " (Default)";
            line1 = EditorGUI.PrefixLabel(line1, new GUIContent(label));

            // Get the element properties
            var element = GetListElement(index);
            var nameProp = element.FindPropertyRelative("name");
            var nameInvalidProp = element.FindPropertyRelative("nameInvalid");
            var nameDuplicateProp = element.FindPropertyRelative("nameNotUnique");

            // Check name
            bool error = false;
            if (nameInvalidProp.boolValue)
            {
                state |= GeneratorConstantsState.NameValidErrors;
                error = true;
            }
            if (nameDuplicateProp.boolValue)
            {
                state |= GeneratorConstantsState.NameDuplicateErrors;
                error = true;
            }
            if (m_ReservedNames != null)
            {
                if (element.FindPropertyRelative("nameReserved").boolValue)
                {
                    state |= GeneratorConstantsState.NameReservedErrors;
                    error = true;
                }
            }

            // Set to red if error
            if (error)
                GUI.color = new Color(1f, 0.5f, 0.5f);

            // Draw name
            string newName = EditorGUI.DelayedTextField(line1, nameProp.stringValue);
            if (newName != nameProp.stringValue)
            {
                nameProp.stringValue = newName;
                m_DirtyProperty.boolValue = true;

                // Check name is valid
                nameInvalidProp.boolValue = !ScriptGenerationUtilities.CheckClassOrPropertyName(nameProp.stringValue, false);

                // Check name is not reserved
                if (m_ReservedNames != null)
                    element.FindPropertyRelative("nameReserved").boolValue = ScriptGenerationUtilities.CheckNameCollision(nameProp.stringValue, m_ReservedNames);

                // Check for duplicates
                if (nameDuplicateProp.boolValue)
                {
                    // It already was a duplicate. Check all list elements
                    for (int i = 0; i < serializedProperty.arraySize; ++i)
                    {
                        var e = serializedProperty.GetArrayElementAtIndex(i);
                        e.FindPropertyRelative("nameNotUnique").boolValue = !ScriptGenerationUtilities.CheckNameIsUnique(serializedProperty, i, "name");
                    }
                }
                else
                {
                    // Get all duplicates and set not unique on each
                    List<int> duplicates = new List<int>();
                    if (ScriptGenerationUtilities.CheckForDuplicateNames(serializedProperty, index, "name", duplicates) > 0)
                    {
                        for (int i = 0; i < duplicates.Count; ++i)
                            GetListElement(duplicates[i]).FindPropertyRelative("nameNotUnique").boolValue = true;
                    }
                    else
                        nameDuplicateProp.boolValue = false;
                }
            }

            // Reset GUI colour
            if (error)
                GUI.color = Color.white;
        }

        protected override void OnAdded(ReorderableList list)
        {
            base.OnAdded(list);

            var newElement = serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);

            // Reset properties
            newElement.FindPropertyRelative("name").stringValue = string.Empty;

            // Reset errors
            newElement.FindPropertyRelative("nameInvalid").boolValue = true;
            newElement.FindPropertyRelative("nameNotUnique").boolValue = false;
            newElement.FindPropertyRelative("nameReserved").boolValue = false;
        }

        protected override void OnRemoved(ReorderableList list)
        {
            // Check for duplicate errors
            bool duplicateName = GetListElement(list.index).FindPropertyRelative("nameNotUnique").boolValue;

            base.OnRemoved(list);

            // Check all remaining for duplicate names
            if (duplicateName)
            {
                for (int i = 0; i < serializedProperty.arraySize; ++i)
                {
                    var element = serializedProperty.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("nameNotUnique").boolValue = !ScriptGenerationUtilities.CheckNameIsUnique(serializedProperty, i, "name");
                }
            }
        }

        protected override void OnChanged(ReorderableList list)
        {
            base.OnChanged(list);
            m_DirtyProperty.boolValue = true;
        }

        public override void DoLayoutList()
        {
            // Reset errors (will be set in DrawListElement)
            state = GeneratorConstantsState.Valid;

            // Draw the list elements
            base.DoLayoutList();

            // Check if requires rebuild
            if (m_DirtyProperty.boolValue)
                state |= GeneratorConstantsState.RequiresRebuild;

            // Record errors
            if (m_ErrorProperty != null)
                m_ErrorProperty.intValue = (int)state;
        }
    }
}