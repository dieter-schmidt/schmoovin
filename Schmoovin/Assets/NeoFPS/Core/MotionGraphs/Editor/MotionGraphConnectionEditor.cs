using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.CharacterMotion;
using System;

namespace NeoFPSEditor.CharacterMotion
{
    [CustomEditor(typeof(MotionGraphConnection), true)]
    public class MotionGraphConnectionEditor : Editor
    {
        private Dictionary<int, MotionGraphConditionGroupDrawer> m_ConditionGroupDrawers = new Dictionary<int, MotionGraphConditionGroupDrawer>();
        private Dictionary<int, MotionGraphConditionDrawer> m_ConditionDrawers = new Dictionary<int, MotionGraphConditionDrawer>();
        private ReorderableList m_ConditionsList = null;
        private Texture2D m_Icon = null;
        private GUIStyle m_HeaderStyle = null;
        private GUIStyle m_TitleStyle = null;

        public MotionGraphConnection connection { get; private set; }
        public MotionGraphContainer container { get; private set; }

        private void Awake()
        {
            // Load icon
            var guids = AssetDatabase.FindAssets("EditorImage_NeoFpsInspectorIcon");
            if (guids != null && guids.Length > 0)
                m_Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
            else
                m_Icon = null;

            // Set up background style background
            m_HeaderStyle = new GUIStyle(EditorStyles.inspectorFullWidthMargins);
            m_HeaderStyle.padding = new RectOffset(0, 0, 0, 4);
            if (EditorGUIUtility.isProSkin)
                guids = AssetDatabase.FindAssets("EditorImage_InspectorHeaderDark");
            else
                guids = AssetDatabase.FindAssets("EditorImage_InspectorHeaderLight");
            if (guids != null && guids.Length > 0)
            {
                m_HeaderStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
                m_HeaderStyle.border = new RectOffset(0, 0, 0, 2);
            }

            // Set up title style
            m_TitleStyle = new GUIStyle(EditorStyles.label);
            m_TitleStyle.fontSize = 12;

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDestroy()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        protected virtual void OnEnable()
        {
            connection = target as MotionGraphConnection;
            var mg = connection.source as MotionGraph;
            if (mg != null)
                container = mg.container;
            else
                container = connection.source.parent.container;

            m_ConditionsList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("m_Conditions"),
                true, true, true, true
                );
            m_ConditionsList.drawHeaderCallback = DrawConditionsListHeader;
            m_ConditionsList.drawElementCallback = DrawConditionsListElements;
            m_ConditionsList.elementHeightCallback = GetConditionsListElementHeight;
            m_ConditionsList.onAddDropdownCallback = OnConditionsListAddDropdown;
            m_ConditionsList.onRemoveCallback = OnConditionsListRemoved;
        }

        protected override void OnHeaderGUI()
        {
            EditorGUILayout.BeginHorizontal(m_HeaderStyle);

            // Show NeoFPS icon
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(32), GUILayout.Height(32));
            if (m_Icon != null)
            {
                r.width += 8f;
                r.height += 8f;
                GUI.Label(r, m_Icon);
            }

            // Details
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            // Name
            r = EditorGUILayout.GetControlRect();
            r.width -= 24;
            EditorGUI.LabelField(r, "Connection", m_TitleStyle);

            // Help
            r.x += r.width + 4;
            r.y += 2;
            r.width = 20;
            if (GUI.Button(r, "", MotionGraphEditorStyles.helpButton))
                Application.OpenURL("https://docs.neofps.com/manual/motiongraph-index.html");

            // Source
            r = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(r, "Source");
            r.width -= 140f;
            r.x += 80f;
            EditorGUI.LabelField(r, connection.source.name, EditorStyles.boldLabel);
            r.x += r.width;
            r.width = 60;
            if (GUI.Button(r, "Inspect"))
                Selection.activeObject = connection.source;

            // Destination
            r = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(r, "Destination");
            r.width -= 140f;
            r.x += 80f;
            EditorGUI.LabelField(r, connection.destination.name, EditorStyles.boldLabel);
            r.x += r.width;
            r.width = 60;
            if (GUI.Button(r, "Inspect"))
                Selection.activeObject = connection.destination;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            m_ConditionsList.DoLayoutList();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TransitionOn"));

            DrawConditionGroups();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawConditionGroups()
        {
            // Header
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ConditionGroups", EditorStyles.boldLabel);

            // Info
            EditorGUILayout.HelpBox("Condition groups are evaluated using \"Condition Group\" conditions", MessageType.Info);

            // Create new group
            if (GUILayout.Button("Create New Condition Group"))
                MotionGraphEditorFactory.CreateConditionGroup(connection);

            // Draw individual groups
            SerializedProperty groups = serializedObject.FindProperty("m_ConditionGroups");
            for (int i = 0; i < groups.arraySize; ++i)
            {
                GetConditionGroupDrawer(groups, i).DoLayout();
            }
        }
        
        public void OnGroupMenuMoveUp(object o)
        {
            int index = (int)o;
            SerializedArrayUtility.Move(serializedObject.FindProperty("m_ConditionGroups"), index, index - 1);
            serializedObject.ApplyModifiedProperties();
            Repaint();
        }

        public void OnGroupMenuMoveDown(object o)
        {
            int index = (int)o;
            SerializedArrayUtility.Move(serializedObject.FindProperty("m_ConditionGroups"), index, index + 1);
            serializedObject.ApplyModifiedProperties();
            Repaint();
        }

        public void OnGroupMenuRemove(object o)
        {
            int index = (int)o;
            SerializedArrayUtility.RemoveAt(serializedObject.FindProperty("m_ConditionGroups"), index);
            serializedObject.ApplyModifiedProperties();
            m_ConditionGroupDrawers.Clear();
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

        MotionGraphConditionDrawer GetConditionDrawer (MotionGraphCondition condition)
        {
            MotionGraphConditionDrawer result;
            if (m_ConditionDrawers.TryGetValue(condition.GetInstanceID(), out result))
                return result;

            result = MotionGraphEditorFactory.GetConditionDrawer(container, condition);
            m_ConditionDrawers.Add(condition.GetInstanceID(), result);
            return result;
        }

        MotionGraphConditionGroupDrawer GetConditionGroupDrawer(SerializedProperty groupsProp, int index)
        {
            int id = connection.conditionGroups[index].id;

            MotionGraphConditionGroupDrawer result;
            if (m_ConditionGroupDrawers.TryGetValue(id, out result))
                return result;

            result = new MotionGraphConditionGroupDrawer(groupsProp, index, this);
            m_ConditionGroupDrawers.Add(id, result);
            return result;
        }

        void OnConditionsListAddDropdown(Rect buttonRect, ReorderableList list)
        {
            list.serializedProperty.serializedObject.Update();
            var menu = MotionGraphEditorFactory.GetConditionMenu(container, serializedObject);
            menu.ShowAsContext();
        }

        void OnConditionsListRemoved(ReorderableList list)
        {
            list.serializedProperty.serializedObject.Update();
            
            // Remove from the list
            SerializedArrayUtility.RemoveAt(list.serializedProperty, list.index);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
            list.index = -1;

            // Reset the condition drawers
            m_ConditionDrawers.Clear();
        }

        void OnUndoRedo()
        {
            serializedObject.Update();
            m_ConditionDrawers.Clear();
            m_ConditionGroupDrawers.Clear();
            Repaint();
        }
    }
}
