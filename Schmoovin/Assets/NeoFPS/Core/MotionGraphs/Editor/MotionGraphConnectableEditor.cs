using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.CharacterMotion;

namespace NeoFPSEditor.CharacterMotion
{
    [CustomEditor(typeof(MotionGraphConnectable), true)]
    public abstract class MotionGraphConnectableEditor : Editor
    {
        private Dictionary<int, MotionGraphBehaviourEditor> m_BehaviourEditors = new Dictionary<int, MotionGraphBehaviourEditor>();
        private ReorderableList m_ConnectionsList = null;
        private bool m_BehaviourEditorsInitialised = false;
        private Texture2D m_Icon = null;
        private GUIStyle m_HeaderStyle = null;
        private string m_HelpURL = string.Empty;

        enum ConnectableType
        {
            State,
            Graph
        }

        public MotionGraphConnectable connectable { get; private set; }

        public abstract MotionGraphContainer container
        {
            get;
        }

        public abstract string typeName
        {
            get;
        }

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
        }

        protected virtual void OnEnable()
        {
            connectable = target as MotionGraphConnectable;

            m_ConnectionsList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("m_Connections"),
                true, true, false, false
                );
            m_ConnectionsList.drawHeaderCallback = DrawConnectionsListHeader;
            m_ConnectionsList.drawElementCallback = DrawConnectionsListElements;

            m_BehaviourEditorsInitialised = false;

            // Get help URL
            var t = connectable.GetType();
            var attributes = t.GetCustomAttributes(true);
            foreach (var attr in attributes)
            {
                var help = attr as HelpURLAttribute;
                if (help != null)
                    m_HelpURL = help.URL;
            }

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        protected virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        protected sealed override void OnHeaderGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

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

            // Get top rectangle
            r = EditorGUILayout.GetControlRect();
            bool showHelp = !string.IsNullOrEmpty(m_HelpURL);
            if (showHelp)
                r.width -= 24;

            // Name
            EditorGUI.PropertyField(r, serializedObject.FindProperty("m_Name"), GUIContent.none);

            // Help button
            if (showHelp)
            {
                r.x += r.width + 4;
                r.width = 20;
                if (GUI.Button(r, "", MotionGraphEditorStyles.helpButton))
                    Application.OpenURL(m_HelpURL);
            }

            // Parent (include inspect button if not root)
            r = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(r, "Parent");
            r.width -= 60f;
            r.x += 60f;
            if (connectable.parent != null)
            {
                EditorGUI.LabelField(r, connectable.parent.name, EditorStyles.boldLabel);

                float width = r.width;
                r.width = 60f;
                r.x += width - 60f;
                if (GUI.Button(r, "Inspect"))
                    Selection.activeObject = connectable.parent;
            }
            else
                EditorGUI.LabelField(r, "<None>", EditorStyles.boldLabel);

            // Connectable type name
            string t = typeName;
            if (t != string.Empty)
            {
                r = EditorGUILayout.GetControlRect();

                EditorGUI.LabelField(r, "Type");
                r.width -= 60f;
                r.x += 60f;
                EditorGUI.LabelField(r, t, EditorStyles.boldLabel);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        public sealed override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            // Script
            DisplayScript();
            
            // Contents
            OnInspectorGUIInternal();

            // Connections
            EditorGUILayout.Space();
            m_ConnectionsList.DoLayoutList();

            // Behaviours
            if (!m_BehaviourEditorsInitialised)
                ResetBehaviourEditors();
            DrawBehaviours();

            // Behaviour Picker
            DrawBehaviourPicker();

            serializedObject.ApplyModifiedProperties();
        }

        void DisplayScript ()
        {
            var script = MotionGraphEditorFactory.GetScriptForGraphElement(target.GetType());
            if (script != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Script");
                GUI.enabled = false;
                EditorGUILayout.ObjectField(script, typeof(MonoScript), false);
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }
        }

        protected abstract void OnInspectorGUIInternal();

        void DrawConnectionsListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Out Connections");
        }

        void DrawConnectionsListElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = m_ConnectionsList.serializedProperty.GetArrayElementAtIndex(index);

            Rect labelRect = new Rect(rect);
            labelRect.width -= 80f;
            labelRect.y += 2f;
            Rect buttonRect = new Rect(rect);
            buttonRect.width = 80f;
            buttonRect.x += labelRect.width;
            buttonRect.yMax -= 3f;

            var con = (element.objectReferenceValue as MotionGraphConnection);
            if (con == null || con.destination == null)
                EditorGUI.LabelField(rect, "Invalid connection");
            else
            {
                EditorGUI.LabelField(labelRect, "this->" + con.destination.name);
                if (GUI.Button(buttonRect, "Inspect"))
                    Selection.activeObject = element.objectReferenceValue;
            }
        }

        public void ResetBehaviourEditors()
        {
            m_BehaviourEditors.Clear();
            for (int i = 0; i < connectable.behaviours.Count; ++i)
            {
                var be = MotionGraphEditorFactory.GetBehaviourEditor(connectable.behaviours[i]);
                be.Initialise(connectable.behaviours[i], this);
                m_BehaviourEditors.Add(connectable.behaviours[i].GetInstanceID(), be);
            }
            m_BehaviourEditorsInitialised = true;
        }

        void DrawBehaviours ()
        {
            var behaviours = serializedObject.FindProperty("m_Behaviours");
            for (int i = 0; i < behaviours.arraySize; ++i)
            {
                int id = behaviours.GetArrayElementAtIndex(i).objectReferenceInstanceIDValue;
                MotionGraphBehaviourEditor be;
                if (m_BehaviourEditors.TryGetValue(id, out be))
                    be.DoLayout(connectable, i);
            }
        }

        void DrawBehaviourPicker ()
        {
            MotionGraphEditorStyles.DrawSeparator();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            Rect r = EditorGUILayout.GetControlRect(false, GUILayout.Width(200f));
            if (GUI.Button(r, "Add Behaviour"))
            {
                var mg = connectable as MotionGraph;
                if (mg != null)
                {
                    var popup = MotionGraphEditorFactory.GetStateBehaviourPopup(mg.container, serializedObject);
                    popup.onSelect += OnBehaviourCreated;
                    PopupWindow.Show(r, popup);
                }
                else
                {
                    var popup = MotionGraphEditorFactory.GetStateBehaviourPopup(connectable.parent.container, serializedObject);
                    popup.onSelect += OnBehaviourCreated;
                    PopupWindow.Show(r, popup);
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        void OnBehaviourCreated()
        {
            ResetBehaviourEditors();
            Repaint();
        }

        void OnUndoRedo()
        {
            serializedObject.Update();
            ResetBehaviourEditors();
            Repaint();
        }

        public void ReorderMotionGraphBehaviour(MotionGraphBehaviour behaviour, int offset)
        {
            var prop = serializedObject.FindProperty("m_Behaviours");
            SerializedArrayUtility.Move(prop, behaviour, offset);
            serializedObject.ApplyModifiedProperties();
            ResetBehaviourEditors();
        }

        public void RemoveBehaviour(MotionGraphBehaviour behaviour)
        {
            var prop = serializedObject.FindProperty("m_Behaviours");
            SerializedArrayUtility.Remove(prop, behaviour);
            serializedObject.ApplyModifiedProperties();
            Undo.DestroyObjectImmediate(behaviour);
            ResetBehaviourEditors();
            Repaint();
        }
    }
}
