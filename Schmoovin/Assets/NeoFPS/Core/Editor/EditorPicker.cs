using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor.CharacterMotion
{
    public class EditorPicker
    {
        public delegate void SelectionHandler (int id);
        public event SelectionHandler onSelect;

        private const float k_DropdownWidth = 200f;
        private const float k_DropdownHeight = 300f;
        private readonly char[] k_SplitTokens = new char[] { '/', '\\' };

        private string m_SearchString = string.Empty;
        private string m_ButtonString = string.Empty;
        private string m_HeaderString = string.Empty;
        private bool m_Show = false;
        private Vector2 m_Scroll = Vector2.zero;
        private Folder m_RootFolder = null;
        private bool m_StartExpanded = false;
        private bool m_EarlyIndent = false;

        public string filter
        {
            get { return m_SearchString; }
            set
            {
                m_SearchString = value;
                m_RootFolder.Filter(m_SearchString);
            }
        }

        public bool showing
        {
            get { return m_Show; }
        }

        private class Entry
        {
            public EditorPicker picker = null;
            public string name = string.Empty;
            public int key = 0;
            public bool visible = true;

            public Entry(EditorPicker p, string n, int k)
            {
                picker = p;
                name = n;
                key = k;
            }

            public Rect Draw (Rect r)
            {
                if (GUI.Button(r, name, EditorStyles.label))
                    picker.SelectEntry(key);
                r.y += EditorGUIUtility.singleLineHeight;
                return r;
            }

            public void Filter (string filter)
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
            public List<Entry> entries = new List<Entry> ();
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

            public Folder (string n, bool startExpanded)
            {
                name = n;
                expanded = startExpanded;
            }

            public Rect Draw (Rect r, bool earlyIndent)
            {
                if (name != string.Empty)
                {
                    if (earlyIndent)
                    {
                        r.x += 12;
                        r.width -= 12;
                    }
                    expanded = EditorGUI.Foldout(r, expanded, name, true);
                    r.y += EditorGUIUtility.singleLineHeight;
                    if (expanded)
                    {
                        if (!earlyIndent)
                        {
                            r.x += 12;
                            r.width -= 12;
                        }
                        for (int i = 0; i < folders.Count; ++i)
                            if (folders[i].visible)
                                r = folders[i].Draw(r, earlyIndent);
                        for (int i = 0; i < entries.Count; ++i)
                            if (entries[i].visible)
                                r = entries[i].Draw(r);
                        if (!earlyIndent)
                        {
                            r.x -= 12;
                            r.width += 12;
                        }
                    }
                    if (earlyIndent)
                    {
                        r.x -= 12;
                        r.width += 12;
                    }
                }
                else
                {
                    for (int i = 0; i < folders.Count; ++i)
                        if (folders[i].visible)
                            r = folders[i].Draw(r, earlyIndent);
                    for (int i = 0; i < entries.Count; ++i)
                        if (entries[i].visible)
                            r = entries[i].Draw(r);
                }
                return r;
            }

            public float GetHeight ()
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

            public void Filter (string filter)
            {
                for (int i = 0; i < folders.Count; ++i)
                    folders[i].Filter(filter);
                for (int i = 0; i < entries.Count; ++i)
                    entries[i].Filter(filter);
            }
        }

        public EditorPicker(string buttonText, string headerText, bool startExpanded, bool earlyIndent = true)
        {
            m_ButtonString = buttonText;
            m_HeaderString = headerText;
            m_RootFolder = new Folder(string.Empty, startExpanded);
            m_StartExpanded = startExpanded;
            m_EarlyIndent = earlyIndent;
        }

        public void Add (string entry, int id)
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
                    Folder newFolder = new Folder(tokens[index], m_StartExpanded);
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

        void SelectEntry (int id)
        {
            if (onSelect != null)
                onSelect(id);
            m_Show = false;
        }

        public bool Draw ()
        {
            // Draw the dropdown button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (EditorGUILayout.DropdownButton(new GUIContent(m_ButtonString), FocusType.Passive, GUILayout.Width(200f)))
            {
                m_Show = !m_Show;
                Event.current.Use();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Draw the popup
            if (m_Show)
            {     
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                Rect boxRect = EditorGUILayout.BeginVertical();
                GUILayout.Box("", MotionGraphEditorStyles.box, GUILayout.Width(k_DropdownWidth), GUILayout.Height(k_DropdownHeight));

                // Check if clicked outside the popup and cancel if so
                Event e = Event.current;
                if (e.isMouse && e.type == EventType.MouseDown && !boxRect.Contains(e.mousePosition))
                {
                    m_Show = false;

                    // Close layouts
                    EditorGUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    return true;
                }

                // Draw search field
                float h = EditorGUIUtility.singleLineHeight;
                Rect r = boxRect;
                r.position += new Vector2(2f, 2f);
                r.height = h + 4f;
                r.width -= 84;
                filter = EditorGUI.TextField(r, filter, MotionGraphEditorStyles.search);

                // Draw the search cancel
                r.x += r.width;
                r.width = 80;
                if (GUI.Button(r, "", MotionGraphEditorStyles.searchCancel))
                    filter = string.Empty;

                // Draw the header
                r.x = boxRect.x;
                r.y += h + 6;
                r.width = boxRect.width;
                r.height = h;
                GUI.Box(r, m_HeaderString, MotionGraphEditorStyles.boxDark);

                // Resize box to remaining space
                float used = (2f * h) + 8;
                boxRect.height -= used;
                boxRect.y += used;

                // Check if contents size is bigger than available area
                float contentsHeight = m_RootFolder.GetHeight();
                if (contentsHeight > boxRect.height)
                {
                    // Draw the contents in a scroll rect
                    Rect contentsRect = new Rect(Vector2.zero, new Vector2(k_DropdownWidth - 16f, contentsHeight));
                    m_Scroll = GUI.BeginScrollView(boxRect, m_Scroll, contentsRect);
                    contentsRect.height = h;
                    m_RootFolder.Draw(contentsRect, m_EarlyIndent);
                    GUI.EndScrollView();
                }
                else
                {
                    // Just draw each line in turn
                    boxRect.height = h;
                    m_RootFolder.Draw(boxRect, m_EarlyIndent);
                }

                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            return false;
        }
    }
}