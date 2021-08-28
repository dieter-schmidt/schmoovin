using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;

namespace NeoFPSEditor.CharacterMotion
{
    public class MotionGraphBehaviourPopup : PopupWindowContent
    {
        private readonly char[] k_SplitTokens = new char[] { '/', '\\' };
        private const float k_Width = 200f;

        private MotionGraphContainer m_Container = null;
        private SerializedObject m_ConnectableSO = null;
        private string m_SearchString = string.Empty;
        private Vector2 m_Scroll = Vector2.zero;
        private Folder m_RootFolder = null;
        private bool m_Close = false;

        public event Action onSelect = null;

        public string filter
        {
            get { return m_SearchString; }
            set
            {
                m_SearchString = value;
                m_RootFolder.Filter(m_SearchString);
            }
        }

        private class Entry
        {
            public MotionGraphBehaviourPopup popup = null;
            public string name = string.Empty;
            public int key = 0;
            public bool visible = true;

            public Entry(MotionGraphBehaviourPopup p, string n, int k)
            {
                popup = p;
                name = n;
                key = k;
            }

            public void Draw()//Rect r)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 16);
                if (GUILayout.Button(name, EditorStyles.label))
                {
                    popup.SelectEntry(key);
                }
                GUILayout.EndHorizontal();
            }

            public void Filter(string filter)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    visible = true;
                    return;
                }
                if (name.ToLower().Contains(filter.ToLower()))
                    visible = true;
                else
                    visible = false;
            }
        }

        private class Folder
        {
            public string name = string.Empty;
            public List<Folder> folders = new List<Folder>();
            public List<Entry> entries = new List<Entry>();
            public bool expanded = true;

            public bool visible
            {
                get
                {
                    for (int i = 0; i < entries.Count; ++i)
                        if (entries[i].visible)
                            return true;
                    return false;
                }
            }

            public Folder(string n)
            {
                name = n;
                expanded = true;
            }

            public void Draw()
            {
                if (name != string.Empty)
                {
                    expanded = EditorGUILayout.Foldout(expanded, name, true);
                    if (expanded)
                    {
                        ++EditorGUI.indentLevel;

                        for (int i = 0; i < folders.Count; ++i)
                            if (folders[i].visible)
                                folders[i].Draw();
                        for (int i = 0; i < entries.Count; ++i)
                            if (entries[i].visible)
                                entries[i].Draw();

                        --EditorGUI.indentLevel;
                    }
                }
                else
                {
                    for (int i = 0; i < folders.Count; ++i)
                        if (folders[i].visible)
                            folders[i].Draw();
                    for (int i = 0; i < entries.Count; ++i)
                        if (entries[i].visible)
                            entries[i].Draw();
                }
            }

            public float GetHeight()
            {
                float h = EditorGUIUtility.singleLineHeight;
                float result = 0f;
                // Check if root
                if (name == string.Empty)
                    expanded = true;
                else
                    result += h;
                // Add contents heights
                if (expanded)
                {
                    for (int i = 0; i < folders.Count; ++i)
                        if (folders[i].visible)
                            result += folders[i].GetHeight();
                    for (int i = 0; i < entries.Count; ++i)
                        if (entries[i].visible)
                            result += h;
                    return result;
                }
                // Finished
                return result;
            }

            public void Filter(string filter)
            {
                for (int i = 0; i < folders.Count; ++i)
                    folders[i].Filter(filter);
                for (int i = 0; i < entries.Count; ++i)
                    entries[i].Filter(filter);
            }
        }

        public MotionGraphBehaviourPopup(MotionGraphContainer container, SerializedObject connectableSO)
        {
            m_Container = container;
            m_ConnectableSO = connectableSO;
            m_RootFolder = new Folder(string.Empty);
        }

        public void Add(string entry, int id)
        {
            if (string.IsNullOrEmpty(entry))
                return;

            string[] tokens = entry.Split(k_SplitTokens);
            if (tokens.Length == 0)
                return;

            // Set up folders
            Folder f = m_RootFolder;
            int index = 0;
            for (; index < tokens.Length - 1; ++index)
            {
                // Find folder
                bool found = false;
                string lowerToken = tokens[index].ToLower();
                for (int i = 0; i < f.folders.Count; ++i)
                {
                    if (f.folders[i].name.ToLower() == lowerToken)
                    {
                        f = f.folders[i];
                        found = true;
                        break;
                    }
                }
                // Not found, create it
                if (!found)
                {
                    Folder newFolder = new Folder(tokens[index]);
                    f.folders.Add(newFolder);
                    f.folders.Sort((Folder lhs, Folder rhs) =>
                    {
                        return string.Compare(lhs.name, rhs.name, true);
                    });
                    f = newFolder;
                }
            }

            // Add the entry
            f.entries.Add(new Entry(this, tokens[index], id));
            f.entries.Sort((Entry lhs, Entry rhs) =>
            {
                return string.Compare(lhs.name, rhs.name, true);
            });
            f.Filter(filter);
        }

        void SelectEntry(int index)
        {
            // Create the behaviour
            MotionGraphEditorFactory.CreateBehaviour(m_Container, m_ConnectableSO, index);

            // Fire event
            if (onSelect != null)
                onSelect();

            m_Close = true;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(k_Width, 280f);
        }

        public override void OnGUI(Rect rect)
        {
            // Draw search field
            EditorGUILayout.BeginHorizontal();
            filter = EditorGUILayout.TextField(filter, MotionGraphEditorStyles.search, GUILayout.Height(19));
            if (GUILayout.Button("", MotionGraphEditorStyles.searchCancel, GUILayout.Height(19)))
                filter = string.Empty;
            EditorGUILayout.EndHorizontal();

            // Draw entries
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            m_RootFolder.Draw();
            EditorGUILayout.EndScrollView();

            // Close the popup
            if (m_Close)
                EditorWindow.GetWindow<PopupWindow>().Close();
        }
    }
}