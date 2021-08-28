using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public static class WizardGUI
    {
        private static GUIStyle m_ReadonlyFieldStyle = null;
        private static GUIStyle GetFieldStyle()
        {
            if (m_ReadonlyFieldStyle == null)
            {
                m_ReadonlyFieldStyle = new GUIStyle(EditorStyles.textField);
                //m_ReadonlyFieldStyle.stretchWidth = true;
                m_ReadonlyFieldStyle.clipping = TextClipping.Clip;
                m_ReadonlyFieldStyle.stretchWidth = false;
            }
            return m_ReadonlyFieldStyle;
        }

        private static GUILayoutOption GetFieldWidth()
        {
            return GUILayout.Width(EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 20);
        }

        public static void DoSummary(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(label));
            EditorGUILayout.LabelField(value);
            EditorGUILayout.EndHorizontal();
        }

        public static void DoSummary(string label, bool value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(label));
            EditorGUILayout.LabelField(value.ToString());
            EditorGUILayout.EndHorizontal();
        }

        public static void DoSummary(string label, int value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(label));
            EditorGUILayout.LabelField(value.ToString());
            EditorGUILayout.EndHorizontal();
        }

        public static void DoSummary(string label, float value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(label));
            EditorGUILayout.LabelField(value.ToString("F3"));
            EditorGUILayout.EndHorizontal();
        }


        public static void DoSummary(string label, Vector3 value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(label));
            EditorGUILayout.LabelField(value.ToString("F3"));
            EditorGUILayout.EndHorizontal();
        }

        public static void DoSummary(string label, float value, int decimalPlaces)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(label));
            EditorGUILayout.LabelField(value.ToString("F" + decimalPlaces));
            EditorGUILayout.EndHorizontal();
        }

        public static void DoSummary(string label, Vector3 value, int decimalPlaces)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(label));
            EditorGUILayout.LabelField(value.ToString("F" + decimalPlaces));
            EditorGUILayout.EndHorizontal();
        }

        public static void MultiChoiceSummary(string label, int value, string[] options)
        {
            string summary = (value < 0 || value >= options.Length) ? "<Not Selected>" : options[value];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(label));
            EditorGUILayout.LabelField(summary);
            EditorGUILayout.EndHorizontal();
        }

        public static void ObjectSummary<T>(string label, T value) where T : class
        {
            using (var scope = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(label));

                // Get the full rect
                var fullRect = scope.rect;
                fullRect.x += EditorGUIUtility.labelWidth;
                fullRect.width -= EditorGUIUtility.labelWidth;

                var obj = value as Object;
                if (obj == null)
                {
                    GUI.Button(fullRect, "<None Selected>", GetFieldStyle());
                }
                else
                {
                    if (GUI.Button(fullRect, obj.name, GetFieldStyle()))
                    {
                        var component = obj as Component;
                        if (component != null)
                            EditorGUIUtility.PingObject(component.gameObject);
                        else
                            EditorGUIUtility.PingObject(obj.GetInstanceID());
                    }
                }
            }
        }

        public static void ObjectListSummary<T>(string label, T[] values) where T : class
        {
            float max = Mathf.Min(values.Length, 20f);
            for (int i = 0; i < max; ++i)
            {
                using (var scope = new EditorGUILayout.HorizontalScope())
                {
                    if (i == 0)
                        EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(label));
                    else
                        EditorGUILayout.PrefixLabel(GUIContent.none);

                    // Get the full rect
                    var fullRect = scope.rect;
                    fullRect.x += EditorGUIUtility.labelWidth;
                    fullRect.width -= EditorGUIUtility.labelWidth;

                    var obj = values[i] as Object;
                    if (obj == null)
                    {
                        GUI.Button(fullRect, "<None Selected>", GetFieldStyle());
                    }
                    else
                    {
                        if (GUI.Button(fullRect, obj.name, GetFieldStyle()))
                        {
                            var component = obj as Component;
                            if (component != null)
                                EditorGUIUtility.PingObject(component.gameObject);
                            else
                                EditorGUIUtility.PingObject(obj.GetInstanceID());
                        }
                    }
                }
            }

            if (values.Length > max)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(GUIContent.none);
                EditorGUILayout.LabelField("...");
                EditorGUILayout.EndHorizontal();
            }
        }

        public static void ObjectListSummary<T>(string label, List<T> values) where T : class
        {
            float max = Mathf.Min(values.Count, 20f);
            for (int i = 0; i < max; ++i)
            {
                using (var scope = new EditorGUILayout.HorizontalScope())
                {
                    if (i == 0)
                        EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(label));
                    else
                        EditorGUILayout.PrefixLabel(GUIContent.none);

                    // Get the full rect
                    var fullRect = scope.rect;
                    fullRect.x += EditorGUIUtility.labelWidth;
                    fullRect.width -= EditorGUIUtility.labelWidth;

                    var obj = values[i] as Object;
                    if (obj == null)
                    {
                        GUI.Button(fullRect, "<None Selected>", GetFieldStyle());
                    }
                    else
                    {
                        if (GUI.Button(fullRect, obj.name, GetFieldStyle()))
                        {
                            var component = obj as Component;
                            if (component != null)
                                EditorGUIUtility.PingObject(component.gameObject);
                            else
                                EditorGUIUtility.PingObject(obj.GetInstanceID());
                        }
                    }
                }
            }

            if (values.Count > max)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(GUIContent.none);
                EditorGUILayout.LabelField("...");
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}