using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPSEditor.CharacterMotion.MotionData;
using NeoFPSEditor.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion
{
    public class MotionGraphEditorGUI
    {
        const string k_NullReferenceString = "<None Selected>";
        static readonly GUIContent k_SelectNull = new GUIContent("<None>");
        static readonly GUIContent k_CreateData = new GUIContent("Create New");

        private static List<MotionGraphParameter> s_ParameterOptions = new List<MotionGraphParameter>();
        private static List<MotionGraphDataBase> s_DataOptions = new List<MotionGraphDataBase>();
        private static SerializedProperty s_Property = null;
        private static MotionGraphContainer s_Container = null;

        public static void ParameterDropdownField<ParameterType>(MotionGraphContainer container, SerializedProperty property) where ParameterType : MotionGraphParameter
        {
            ParameterDropdownField<ParameterType>(container, property, new GUIContent(property.displayName, property.tooltip));
        }

        public static void ParameterDropdownField<ParameterType> (MotionGraphContainer container, SerializedProperty property, GUIContent label) where ParameterType : MotionGraphParameter
        {
            EditorGUILayout.BeginHorizontal();

            // Draw the label if set
            if (label != null && !string.IsNullOrEmpty(label.text))
                EditorGUILayout.PrefixLabel(label);

            // Draw the dropdown
            string propertyRefName = k_NullReferenceString;
            if (property.objectReferenceValue != null)
                propertyRefName = property.objectReferenceValue.name;

            var r = EditorGUILayout.GetControlRect();
            if (EditorGUI.DropdownButton(r, new GUIContent(propertyRefName), FocusType.Passive))
            {
                s_Container = container;
                s_Property = property;
                s_ParameterOptions.Clear();
                container.CollectParameters(s_ParameterOptions);

                GenericMenu menu = new GenericMenu();
                menu.AddItem(k_SelectNull, false, OnParameterSelect, -1);

                menu.AddSeparator("");
                r.y += EditorGUIUtility.singleLineHeight;
                menu.AddItem(k_CreateData, false, OnParameterCreate<ParameterType>, r);

                FilterParameters<ParameterType>();
                if (s_ParameterOptions.Count > 0)
                {
                    menu.AddSeparator("");
                    for (int i = 0; i < s_ParameterOptions.Count; ++i)
                        menu.AddItem(new GUIContent(s_ParameterOptions[i].name), false, OnParameterSelect, i);
                }
                menu.ShowAsContext();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        static void OnParameterCreate<ParameterType>(object r) where ParameterType : MotionGraphParameter
        {
            var popup = new ParameterWizard
            {
                container = s_Container,
                property = s_Property,
                parameterType = typeof(ParameterType)
            };
            try
            {
                PopupWindow.Show((Rect)r, popup);
            }
            catch { }
        }

        public static void ParameterDropdownField<ParameterType>(Rect r, MotionGraphContainer container, SerializedProperty property) where ParameterType : MotionGraphParameter
        {
            // Draw the dropdown
            string propertyRefName = k_NullReferenceString;
            if (property.objectReferenceValue != null)
                propertyRefName = property.objectReferenceValue.name;
            if (EditorGUI.DropdownButton(r, new GUIContent(propertyRefName), FocusType.Passive))
            {
                s_Property = property;
                s_ParameterOptions.Clear();
                container.CollectParameters(s_ParameterOptions);

                GenericMenu menu = new GenericMenu();
                menu.AddItem(k_SelectNull, false, OnParameterSelect, -1);
                FilterParameters<ParameterType>();
                if (s_ParameterOptions.Count > 0)
                {
                    menu.AddSeparator("");
                    for (int i = 0; i < s_ParameterOptions.Count; ++i)
                        menu.AddItem(new GUIContent(s_ParameterOptions[i].name), false, OnParameterSelect, i);
                }
                menu.ShowAsContext();
            }
        }

        static void FilterParameters<ParameterType> () where ParameterType : MotionGraphParameter
        {
            List<MotionGraphParameter> result = new List<MotionGraphParameter>();

            for (int i = 0; i < s_ParameterOptions.Count; ++i)
            {
                var cast = s_ParameterOptions[i] as ParameterType;
                if (cast != null)
                    result.Add(cast);
            }

            s_ParameterOptions = result;
        }

        static void OnParameterSelect(object i)
        {
            int index = (int)i;
            if (index == -1)
                s_Property.objectReferenceValue = null;
            else
                s_Property.objectReferenceValue = s_ParameterOptions[index];
            s_Property.serializedObject.ApplyModifiedProperties();
            s_Property = null;
        }

        public static void BoolDataReferenceField(MotionGraphContainer container, SerializedProperty property)
        {
            DataReferenceField<bool, BoolDataWizard>(container, property, new GUIContent(property.displayName, property.tooltip));
        }

        public static void FloatDataReferenceField(MotionGraphContainer container, SerializedProperty property)
        {
            DataReferenceField<float, FloatDataWizard>(container, property, new GUIContent(property.displayName, property.tooltip));
        }

        public static void IntDataReferenceField(MotionGraphContainer container, SerializedProperty property)
        {
            DataReferenceField<int, IntDataWizard>(container, property, new GUIContent(property.displayName, property.tooltip));
        }

        public static void BoolDataReferenceField(MotionGraphContainer container, SerializedProperty property, GUIContent label)
        {
            DataReferenceField<bool, BoolDataWizard>(container, property, label);
        }

        public static void FloatDataReferenceField(MotionGraphContainer container, SerializedProperty property, GUIContent label)
        {
            DataReferenceField<float, FloatDataWizard>(container, property, label);
        }

        public static void IntDataReferenceField(MotionGraphContainer container, SerializedProperty property, GUIContent label)
        {
            DataReferenceField<int, IntDataWizard>(container, property, label);
        }

        public static bool BoolDataReferenceField(Rect line1, MotionGraphContainer container, SerializedProperty property)
        {
            return DataReferenceField<bool, BoolDataWizard>(line1, container, property, new GUIContent(property.displayName, property.tooltip));
        }

        public static bool FloatDataReferenceField(Rect line1, MotionGraphContainer container, SerializedProperty property)
        {
            return DataReferenceField<float, FloatDataWizard>(line1, container, property, new GUIContent(property.displayName, property.tooltip));
        }

        public static bool IntDataReferenceField(Rect line1, MotionGraphContainer container, SerializedProperty property)
        {
            return DataReferenceField<int, IntDataWizard>(line1, container, property, new GUIContent(property.displayName, property.tooltip));
        }

        public static bool BoolDataReferenceField(Rect line1, MotionGraphContainer container, SerializedProperty property, GUIContent label)
        {
            return DataReferenceField<bool, BoolDataWizard>(line1, container, property, label);
        }

        public static bool FloatDataReferenceField(Rect line1, MotionGraphContainer container, SerializedProperty property, GUIContent label)
        {
            return DataReferenceField<float, FloatDataWizard>(line1, container, property, label);
        }

        public static bool IntDataReferenceField(Rect line1, MotionGraphContainer container, SerializedProperty property, GUIContent label)
        {
            return DataReferenceField<int, IntDataWizard>(line1, container, property, label);
        }

        static void DataReferenceField<DataType, PopupType>(MotionGraphContainer container, SerializedProperty property, GUIContent label) where PopupType : DataWizard, new()
        {
            string dataRefName = k_NullReferenceString;
            var dataProp = property.FindPropertyRelative("m_Data");
            if (dataProp.objectReferenceValue != null)
                dataRefName = dataProp.objectReferenceValue.name;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            var r = EditorGUILayout.GetControlRect();
            if (EditorGUI.DropdownButton(r, new GUIContent(dataRefName), FocusType.Passive))
            {
                s_Property = property;
                s_Container = container;

                s_DataOptions.Clear();
                container.CollectData(s_DataOptions);

                GenericMenu menu = new GenericMenu();
                menu.AddItem(k_SelectNull, false, OnDataSelect, -1);
                menu.AddSeparator("");
                r.y += EditorGUIUtility.singleLineHeight;
                menu.AddItem(k_CreateData, false, OnDataCreate<PopupType>, r);

                FilterData<DataType>();
                if (s_DataOptions.Count > 0)
                {
                    menu.AddSeparator("");
                    for (int i = 0; i < s_DataOptions.Count; ++i)
                        menu.AddItem(new GUIContent(s_DataOptions[i].name), false, OnDataSelect, i);
                }
                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();

            if (dataProp.objectReferenceValue == null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Value"));
                EditorGUI.indentLevel--;
            }
        }

        static bool DataReferenceField<DataType, PopupType>(Rect line1, MotionGraphContainer container, SerializedProperty property, GUIContent label) where PopupType : DataWizard, new()
        {
            string dataRefName = k_NullReferenceString;
            var dataProp = property.FindPropertyRelative("m_Data");
            if (dataProp.objectReferenceValue != null)
                dataRefName = dataProp.objectReferenceValue.name;

            Rect r1 = line1;
            r1.width *= 0.5f;
            Rect r2 = r1;
            r2.x += r2.width;

            EditorGUI.LabelField(r1, label);

            if (EditorGUI.DropdownButton(r2, new GUIContent(dataRefName), FocusType.Passive))
            {
                s_Property = property;
                s_Container = container;

                s_DataOptions.Clear();
                container.CollectData(s_DataOptions);

                GenericMenu menu = new GenericMenu();
                menu.AddItem(k_SelectNull, false, OnDataSelect, -1);
                menu.AddSeparator("");
                r2.y += EditorGUIUtility.singleLineHeight;
                menu.AddItem(k_CreateData, false, OnDataCreate<PopupType>, r2);

                FilterData<DataType>();
                if (s_DataOptions.Count > 0)
                {
                    menu.AddSeparator("");
                    for (int i = 0; i < s_DataOptions.Count; ++i)
                        menu.AddItem(new GUIContent(s_DataOptions[i].name), false, OnDataSelect, i);
                }
                menu.ShowAsContext();
            }
            
            if (dataProp.objectReferenceValue == null)
            {
                line1.y += EditorGUIUtility.singleLineHeight;
                r1 = line1;
                r2 = line1;
                r1.width = r1.width * 0.5f - 20f;
                r1.x += 20f;
                r2.width *= 0.5f;
                r2.x += r2.width;
                
                EditorGUI.LabelField(r1, "Value");
                EditorGUI.PropertyField(r2, property.FindPropertyRelative("m_Value"), new GUIContent());

                return true;
            }
            return false;
        }

        public static bool DataReferenceFieldNotSet(SerializedProperty property)
        {
            var data = property.FindPropertyRelative("m_Data");
            return (data == null || data.objectReferenceValue == null);
        }

        static void FilterData<DataType>()
        {
            List<MotionGraphDataBase> result = new List<MotionGraphDataBase>();

            for (int i = 0; i < s_DataOptions.Count; ++i)
            {
                var cast = s_DataOptions[i] as MotionGraphData<DataType>;
                if (cast != null)
                    result.Add(cast);
            }

            s_DataOptions = result;
        }

        static void OnDataSelect(object i)
        {
            int index = (int)i;
            if (index == -1)
                s_Property.FindPropertyRelative("m_Data").objectReferenceValue = null;
            else
                s_Property.FindPropertyRelative("m_Data").objectReferenceValue = s_DataOptions[index];
            s_Property.serializedObject.ApplyModifiedProperties();
            s_Property = null;
        }

        static void OnDataCreate<PopupType>(object r) where PopupType : DataWizard, new()
        {
            var popup = new PopupType
            {
                container = s_Container,
                property = s_Property
            };
            try
            {
                PopupWindow.Show((Rect)r, popup);
            }
            catch { }
        }
    }
}
