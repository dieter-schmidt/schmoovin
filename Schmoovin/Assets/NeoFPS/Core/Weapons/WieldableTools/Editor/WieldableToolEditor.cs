using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;
using NeoFPS.WieldableTools;
using System;
using System.Collections.Generic;

namespace NeoFPSEditor.WieldableTools
{
    [CustomEditor (typeof (WieldableTool))]
    public class WieldableToolEditor : BaseWieldableItemEditor
    {
        private ReorderableList m_PrimaryActionsList = null;
        private ReorderableList m_SecondaryActionsList = null;

        const float k_TimingIconWidth = 12;

        void OnEnable()
        {
            if (m_PrimaryActionsList == null)
            {
                m_PrimaryActionsList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_PrimaryModules"), true, true, false, true);
                m_PrimaryActionsList.drawHeaderCallback = (Rect r) => { EditorGUI.LabelField(r, "Primary Modules", EditorStyles.boldLabel); };
                m_PrimaryActionsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => { DrawActionListElement(rect, index, m_PrimaryActionsList); };
                m_PrimaryActionsList.onRemoveCallback = OnRemoveObject;

                m_SecondaryActionsList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_SecondaryModules"), true, true, false, true);
                m_SecondaryActionsList.drawHeaderCallback = (Rect r) => { EditorGUI.LabelField(r, "Secondary Modules", EditorStyles.boldLabel); };
                m_SecondaryActionsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => { DrawActionListElement(rect, index, m_SecondaryActionsList); };
                m_SecondaryActionsList.onRemoveCallback = OnRemoveObject;

            }
        }

        private void OnRemoveObject(ReorderableList list)
        {
            var action = list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue;
            if (action != null)
            {
                SerializedArrayUtility.Remove(m_PrimaryActionsList.serializedProperty, action);
                SerializedArrayUtility.Remove(m_SecondaryActionsList.serializedProperty, action);
                SerializedArrayUtility.Add(serializedObject.FindProperty("m_UnmappedModules"), action);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawActionListElement(Rect rect, int index, ReorderableList list)
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            var action = element.objectReferenceValue as BaseWieldableToolModule;

            rect.y += 1;
            rect.width -= k_TimingIconWidth * 3;
            EditorGUI.LabelField(rect, GetActionName(action));

            if (action != null)
            {
                rect.x += rect.width;
                rect.width = k_TimingIconWidth;
                DrawTimingIcon(rect, WieldableToolActionTiming.Start, action.timing);
                rect.x += k_TimingIconWidth;
                DrawTimingIcon(rect, WieldableToolActionTiming.Continuous, action.timing);
                rect.x += k_TimingIconWidth;
                DrawTimingIcon(rect, WieldableToolActionTiming.End, action.timing);
            }
        }

        string GetActionName(BaseWieldableToolModule action)
        {
            if (action == null)
                return "<Null>";
            else
            {
                var tool = target as WieldableTool;
                return NeoFpsEditorUtility.GetCurrentComponentName(tool.gameObject, typeof(BaseWieldableToolModule), action);
            }
        }

        protected override void DrawItemProperties()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Crosshair"));

            // Unmapped actions (button up/down for primary/secondary)
            EditorGUILayout.Space();
            NeoFpsEditorGUI.Separator();

            var unmappedProp = serializedObject.FindProperty("m_UnmappedModules");
            if (unmappedProp.arraySize == 0)
                EditorGUILayout.LabelField("No Unmapped Modules", EditorStyles.boldLabel);
            else
            {
                EditorGUILayout.LabelField("Unmapped Modules", EditorStyles.boldLabel);


                for (int i = unmappedProp.arraySize - 1; i >= 0; --i)
                {
                    var elementProp = unmappedProp.GetArrayElementAtIndex(i);
                    var action = elementProp.objectReferenceValue as BaseWieldableToolModule;

                    if (action != null)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        // Action name & timings
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField(GetActionName(action));

                        LayoutTimingIcon(WieldableToolActionTiming.Start, action.timing);
                        LayoutTimingIcon(WieldableToolActionTiming.Continuous, action.timing);
                        LayoutTimingIcon(WieldableToolActionTiming.End, action.timing);

                        EditorGUILayout.EndHorizontal();

                        // Action mapping
                        EditorGUILayout.BeginHorizontal();

                        if (GUILayout.Button("Primary"))
                        {
                            SerializedArrayUtility.Add(m_PrimaryActionsList.serializedProperty, action);
                            SerializedArrayUtility.RemoveAt(unmappedProp, i);
                            serializedObject.ApplyModifiedProperties();

                            throw new ExitGUIException();
                        }

                        if (GUILayout.Button("Secondary"))
                        {
                            SerializedArrayUtility.Add(m_SecondaryActionsList.serializedProperty, action);
                            SerializedArrayUtility.RemoveAt(unmappedProp, i);
                            serializedObject.ApplyModifiedProperties();

                            throw new ExitGUIException();
                        }

                        if (GUILayout.Button("Remove"))
                        {
                            SerializedArrayUtility.RemoveAt(unmappedProp, i);
                            Undo.DestroyObjectImmediate(action);
                            serializedObject.ApplyModifiedProperties();

                            throw new ExitGUIException();
                        }

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();
                    }
                }

            }

            NeoFpsEditorGUI.Separator();

            m_PrimaryActionsList.DoLayoutList();
            m_SecondaryActionsList.DoLayoutList();

            EditorGUILayout.Space();
            NeoFpsEditorGUI.Separator();

            // Add action button
            if (GUILayout.Button("Add New Module"))
            {
                List<Type> availableTypes = new List<Type>();

                // Gather list of scripts that implement BaseWieldableToolModules
                var guids = AssetDatabase.FindAssets("t:MonoScript");
                if (guids.Length > 0)
                {
                    var actionType = typeof(BaseWieldableToolModule);
                    foreach (var guid in guids)
                    {
                        var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
                        var t = script.GetClass();
                        if (actionType.IsAssignableFrom(t) && !t.IsAbstract)
                            availableTypes.Add(t);
                    }
                }
                
                // Show dropdown menu
                if (availableTypes.Count > 0)
                {
                    var menu = new GenericMenu();
                    foreach (var t in availableTypes)
                        menu.AddItem(new GUIContent(t.Name), false, AddActionFromType, t);
                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.Space();
        }

        void LayoutTimingIcon (WieldableToolActionTiming timing, WieldableToolActionTiming actionTiming)
        {
            string t = string.Empty;
            switch (timing)
            {
                case WieldableToolActionTiming.Start:
                    t = "S";
                    break;
                case WieldableToolActionTiming.Continuous:
                    t = "C";
                    break;
                case WieldableToolActionTiming.End:
                    t = "E";
                    break;
            }

            bool active = (actionTiming & timing) == timing;
            if (active)
                EditorGUILayout.LabelField(t, EditorStyles.boldLabel, GUILayout.Width(k_TimingIconWidth));
            else
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField(t, GUILayout.Width(k_TimingIconWidth));
                GUI.enabled = true;
            }
        }

        void DrawTimingIcon(Rect rect, WieldableToolActionTiming timing, WieldableToolActionTiming actionTiming)
        {
            string t = string.Empty;
            switch (timing)
            {
                case WieldableToolActionTiming.Start:
                    t = "S";
                    break;
                case WieldableToolActionTiming.Continuous:
                    t = "C";
                    break;
                case WieldableToolActionTiming.End:
                    t = "E";
                    break;
            }

            bool active = (actionTiming & timing) == timing;
            if (active)
                EditorGUI.LabelField(rect, t, EditorStyles.boldLabel);
            else
            {
                GUI.enabled = false;
                EditorGUI.LabelField(rect, t);
                GUI.enabled = true;
            }
        }

        void AddActionFromType(object o)
        {
            var tool = target as WieldableTool;
            tool.gameObject.AddComponent((Type)o);
        }
    }
}