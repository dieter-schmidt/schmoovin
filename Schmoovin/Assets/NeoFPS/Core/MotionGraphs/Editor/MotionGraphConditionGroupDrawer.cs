using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.CharacterMotion;
using System;

namespace NeoFPSEditor.CharacterMotion
{
    public class MotionGraphConditionGroupDrawer
    {
        static readonly GUIContent k_TitleMoveUp = new GUIContent("Move Up");
        static readonly GUIContent k_TitleMoveDown = new GUIContent("Move Down");
        static readonly GUIContent k_TitleRemove = new GUIContent("Remove");
        private static GUIStyle m_GroupHeaderStyle = null;

        private Dictionary<int, MotionGraphConditionDrawer> m_ConditionDrawers = new Dictionary<int, MotionGraphConditionDrawer>();
        private ReorderableList m_ConditionsList = null;
        private SerializedProperty m_GroupsArrayProp = null;
        private SerializedProperty m_GroupProp = null;
        private int m_Index = -1;

        public MotionGraphConnectionEditor editor { get; private set; }

        public MotionGraphConditionGroupDrawer(SerializedProperty groupsProp, int index, MotionGraphConnectionEditor ed)
        {
            editor = ed;

            m_Index = index;
            m_GroupsArrayProp = groupsProp;
            m_GroupProp = groupsProp.GetArrayElementAtIndex(index);

            m_ConditionsList = new ReorderableList(
                m_GroupsArrayProp.serializedObject,
                m_GroupProp.FindPropertyRelative("m_Conditions"),
                true, true, true, true
                );
            m_ConditionsList.drawHeaderCallback = DrawConditionsListHeader;
            m_ConditionsList.drawElementCallback = DrawConditionsListElements;
            m_ConditionsList.elementHeightCallback = GetConditionsListElementHeight;
            m_ConditionsList.onAddDropdownCallback = OnConditionsListAddDropdown;
            m_ConditionsList.onRemoveCallback = OnConditionsListRemoved;
        }
        
        public void DoLayout()
        {
            if (m_GroupHeaderStyle == null)
            {
                m_GroupHeaderStyle = new GUIStyle(EditorStyles.foldout);
                m_GroupHeaderStyle.fontStyle = FontStyle.Bold;
            }

            EditorGUILayout.Space();
            if (m_Index != 0)
                MotionGraphEditorStyles.DrawSeparator();

            EditorGUILayout.BeginHorizontal();

            var groupProp = m_GroupsArrayProp.GetArrayElementAtIndex(m_Index);
            var nameProp = groupProp.FindPropertyRelative("m_Name");

            var collapsedProp = groupProp.FindPropertyRelative("collapsed");
            bool collapsed = !EditorGUILayout.Foldout(!collapsedProp.boolValue, nameProp.stringValue, true, m_GroupHeaderStyle);
            if (collapsedProp.boolValue != collapsed)
                collapsedProp.boolValue = collapsed;

            if (GUILayout.Button("", MotionGraphEditorStyles.optionsButton))
            {
                GenericMenu menu = new GenericMenu();

                int count = m_GroupsArrayProp.arraySize;

                // Move up / down
                if (m_Index > 0)
                    menu.AddItem(k_TitleMoveUp, false, editor.OnGroupMenuMoveUp, m_Index);
                else
                    menu.AddDisabledItem(k_TitleMoveUp);

                if (m_Index < count - 1)
                    menu.AddItem(k_TitleMoveDown, false, editor.OnGroupMenuMoveDown, m_Index);
                else
                    menu.AddDisabledItem(k_TitleMoveDown);

                // Remove
                menu.AddSeparator("");
                menu.AddItem(k_TitleRemove, false, editor.OnGroupMenuRemove, m_Index);

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();

            // Draw contents
            if (!collapsed)
            {
                //++EditorGUI.indentLevel;

                // Print name change field
                string newName = EditorGUILayout.DelayedTextField("Group Name", nameProp.stringValue);
                if (newName != nameProp.stringValue)
                    nameProp.stringValue = editor.connection.GetUniqueConditionGroupName(newName, m_Index);

                // Print conditions list
                m_ConditionsList.DoLayoutList();

                // Print transition on property
                EditorGUILayout.PropertyField(m_GroupProp.FindPropertyRelative("m_TransitionOn"));

                //--EditorGUI.indentLevel;
            }
        }

        void DrawConditionsListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Conditions");

            // Draw help button and link to docs
            rect.x += rect.width - 13;
            rect.width = 16;
            rect.height = 15;
            if (GUI.Button(rect, "?", EditorStyles.miniButton))
                Application.OpenURL("https://docs.neofps.com/manual/motiongraph-conditions.html");
        }

        void DrawConditionsListElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = m_ConditionsList.serializedProperty.GetArrayElementAtIndex(index);

            var condition = (element.objectReferenceValue as MotionGraphCondition);
            if (condition == null)
                EditorGUI.LabelField(rect, "Invalid condition");
            else
            {
                try
                {
                    var drawer = GetConditionDrawer(condition);
                    drawer.Draw(rect);
                }
                catch
                {
                    EditorGUI.HelpBox(rect, "Error drawing condition", MessageType.Error);
                }
            }
        }

        private float GetConditionsListElementHeight(int index)
        {
            var prop = m_ConditionsList.serializedProperty.GetArrayElementAtIndex(index);
            var condition = prop.objectReferenceValue as MotionGraphCondition;
            if (condition == null)
                return EditorGUIUtility.singleLineHeight + 4;

            var drawer = GetConditionDrawer(condition);
            if (drawer != null)
                return drawer.GetHeight();
            else
                return EditorGUIUtility.singleLineHeight + 4;
        }

        MotionGraphConditionDrawer GetConditionDrawer(MotionGraphCondition condition)
        {
            MotionGraphConditionDrawer result;
            if (m_ConditionDrawers.TryGetValue(condition.GetInstanceID(), out result))
                return result;

            result = MotionGraphEditorFactory.GetConditionDrawer(editor.container, condition);
            m_ConditionDrawers.Add(condition.GetInstanceID(), result);
            return result;
        }

        void OnConditionsListAddDropdown(Rect buttonRect, ReorderableList list)
        {
            list.serializedProperty.serializedObject.Update();
            var menu = MotionGraphEditorFactory.GetConditionMenu(editor.container, editor.serializedObject, m_Index);
            menu.ShowAsContext();
        }

        void OnConditionsListRemoved(ReorderableList list)
        {
            list.serializedProperty.serializedObject.Update();
            SerializedArrayUtility.RemoveAt(list.serializedProperty, list.index);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
            list.index = -1;
            m_ConditionDrawers.Clear();
        }
    }
}
