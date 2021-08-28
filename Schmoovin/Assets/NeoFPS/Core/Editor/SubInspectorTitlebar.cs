using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace NeoFPSEditor
{
    public class SubInspectorTitlebar
    {
        public delegate bool CheckMenuEntryDelegate();
        public delegate string GetLabelDelegate();
        public GetLabelDelegate getLabel = null;

        private List<MenuEntry> m_ContextEntries = new List<MenuEntry>();
        private bool m_Expanded = true;
        private GUIStyle m_Style = null;

        private struct MenuEntry
        {
            public GUIContent content;
            public GenericMenu.MenuFunction onClick;
            public CheckMenuEntryDelegate checkEntry;

            public MenuEntry(string t, GenericMenu.MenuFunction oc, CheckMenuEntryDelegate ce)
            {
                content = new GUIContent(t);
                onClick = oc;
                checkEntry = ce;
            }
        }

        public string helpURL
        {
            get;
            set;
        }

        public SubInspectorTitlebar(bool expanded = true) : this (string.Empty, expanded)
        {
        }

        public SubInspectorTitlebar(string help, bool expanded = true)
        {
            helpURL = help;
            m_Expanded = expanded;
            
            try
            {
                // Have to use try block to prevent error if hitting play with
                // inspectable object using this selected
                if (EditorStyles.foldout != null)
                {
                    m_Style = new GUIStyle(EditorStyles.foldout);
                    m_Style.fontStyle = FontStyle.Bold;
                }
                else
                {
                    m_Style = new GUIStyle();
                    m_Style.fontStyle = FontStyle.Bold;
                    m_Style.normal.textColor = Color.white;
                }
            }
            catch
            {
                m_Style = new GUIStyle();
                m_Style.fontStyle = FontStyle.Bold;
                m_Style.normal.textColor = Color.white;
            }
        }

        public void AddContextOption (string name, GenericMenu.MenuFunction onClick, CheckMenuEntryDelegate checkEntry)
        {
            if (onClick == null)
                onClick = DefaultOnClick;
            if (checkEntry == null)
                checkEntry = DefaultCheck;
            m_ContextEntries.Add(new MenuEntry(name, onClick, checkEntry));
        }

        public bool DoLayout (bool drawSeparator = true)
        {
            if (drawSeparator)
                NeoFPSEditor.CharacterMotion.MotionGraphEditorStyles.DrawSeparator();
            EditorGUILayout.BeginHorizontal();

            m_Expanded = EditorGUILayout.Foldout(m_Expanded, GetLabel(), true, m_Style);

            if (!string.IsNullOrEmpty(helpURL))
            {
                if (GUILayout.Button("", NeoFPSEditor.CharacterMotion.MotionGraphEditorStyles.helpButton))
                    Application.OpenURL(helpURL);
            }

            if (GUILayout.Button("", NeoFPSEditor.CharacterMotion.MotionGraphEditorStyles.optionsButton) && m_ContextEntries.Count > 0)
            {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < m_ContextEntries.Count; ++i)
                {
                    if (m_ContextEntries[i].checkEntry())
                        menu.AddItem(m_ContextEntries[i].content, false, m_ContextEntries[i].onClick);
                    else
                        menu.AddDisabledItem(m_ContextEntries[i].content);
                }
                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();
            return m_Expanded;
        }

        bool DefaultCheck ()
        {
            return true;
        }

        void DefaultOnClick()
        {
        }

        string GetLabel()
        {
            if (getLabel != null)
                return getLabel();
            return "Header";
        }
    }
}