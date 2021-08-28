using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace NeoFPSEditor.Hub
{
    public class HubTocEntry : IComparable<HubTocEntry>
    {
        const float k_IndentSize = 12f;

        public HubPage page = null;
        public string tocName = string.Empty;
        public string tocLower = string.Empty;
        public int order = 0;
        public List<HubTocEntry> children = new List<HubTocEntry>();
        public ExpandMode expand = ExpandMode.Automatic;

        public enum ExpandMode
        {
            Automatic,
            ForceExpand,
            ForceContract
        }

        public bool isExpanded
        {
            get
            {
                return expand == ExpandMode.ForceExpand ||
                    (expand == ExpandMode.Automatic && ContainsPage(NeoFpsHubEditor.instance.currentPage));
            }
            set
            {
                if (value)
                    expand = ExpandMode.ForceExpand;
                else
                    expand = ExpandMode.ForceContract;
            }
        }

        private static GUIStyle m_ErrorIcon = null;
        public static GUIStyle errorIcon
        {
            get
            {
                if (m_ErrorIcon == null)
                {
                    m_ErrorIcon = new GUIStyle();
                    try
                    {
                        m_ErrorIcon.normal.background = EditorGUIUtility.Load("icons/console.erroricon.sml.png") as Texture2D;
                    }
                    catch { }
                }
                return m_ErrorIcon;
            }
        }

        private static GUIStyle m_WarningIcon = null;
        public static GUIStyle warningIcon
        {
            get
            {
                if (m_WarningIcon == null)
                {
                    m_WarningIcon = new GUIStyle();
                    try
                    {
                        m_WarningIcon.normal.background = EditorGUIUtility.Load("icons/console.warnicon.sml.png") as Texture2D;
                    }
                    catch { }
                }
                return m_WarningIcon;
            }
        }

        private static GUIStyle m_InfoIcon = null;
        public static GUIStyle infoIcon
        {
            get
            {
                if (m_InfoIcon == null)
                {
                    m_InfoIcon = new GUIStyle();
                    try
                    {
                        m_InfoIcon.normal.background = EditorGUIUtility.Load("icons/console.infoicon.sml.png") as Texture2D;
                    }
                    catch { }
                }
                return m_InfoIcon;
            }
        }

        public HubTocEntry(string name, HubPage p, int o = 0)
        {
            page = p;
            tocName = name;
            tocLower = tocName.ToLower();
            order = o;
        }

        public HubTocEntry AddChild(string name, HubPage p)
        {
            var result = new HubTocEntry(name, p);
            children.Add(result);
            return result;
        }

        public bool RemoveChild(HubPage p)
        {
            if (page == p)
            {
                Debug.LogError("Calling RemoveChild on the entry to remove");
                return false;
            }

            for (int i = 0; i < children.Count; ++i)
            {
                if (children[i].page == p)
                {
                    children.RemoveAt(i);
                    return true;
                }
                else
                {
                    if (children[i].RemoveChild(p))
                        return true;
                }
            }

            return false;
        }

        public void SortChildren()
        {
            children.Sort();
            for (int i = 0; i < children.Count; ++i)
                children[i].SortChildren();
        }

        public bool ContainsPage(HubPage p)
        {
            if (page == p)
                return true;

            // Check children
            for (int i = 0; i < children.Count; ++i)
            {
                if (children[i].ContainsPage(p))
                    return true;
            }

            return false;
        }

        public void ResetExpandedState()
        {
            expand = ExpandMode.Automatic;
            for (int i = 0; i < children.Count; ++i)
                children[i].ResetExpandedState();
        }

        public void Awake()
        {
            if (page != null)
                page.Awake();
            for (int i = 0; i < children.Count; ++i)
                children[i].Awake();
        }

        public void Destroy()
        {
            if (page != null)
                page.OnDestroy();
            for (int i = 0; i < children.Count; ++i)
                children[i].Destroy();
        }

        MessageType GetNotification()
        {
            if (page != null)
                return page.notification;

            MessageType result = MessageType.None;

            // Check children for highest notification
            foreach (var c in children)
            {
                var notification = c.GetNotification();
                if (notification > result)
                    result = notification;
            }

            return result;
        }

        public void Draw(float width, int indent)
        {
            bool isCurrent = (NeoFpsHubEditor.instance.currentPage == page);

            float lineHeight = EditorGUIUtility.singleLineHeight;

            var rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(lineHeight));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            rect.width = width;

            // Check notification
            MessageType notification = GetNotification();

            // Show notification
            Color labelColor = Color.white;
            switch (notification)
            {
                case MessageType.Error:
                    labelColor = NeoFpsEditorGUI.errorRed;
                    break;
                case MessageType.Warning:
                    labelColor = NeoFpsEditorGUI.warningYellow;
                    break;
            }

            // Draw indent
            if (indent > 0)
            {
                float indentWidth = k_IndentSize * indent;
                rect.x += indentWidth;
                rect.width -= indentWidth;
            }

            if (children.Count == 0)
            {
                GUI.color = labelColor;
                if (isCurrent)
                {
                    // Print the current page in bold
                    EditorGUI.LabelField(rect, tocName, EditorStyles.boldLabel);
                }
                else
                {
                    // If not the current page, allow it to be selected
                    if (GUI.Button(rect, tocName, EditorStyles.label) && page != null)
                        NeoFpsHubEditor.instance.currentPage = page;
                }
                GUI.color = Color.white;
            }
            else
            {
                // It's a folder (can be a page at the same time)

                // Draw the foldout
                var foldoutRect = rect;
                foldoutRect.width = k_IndentSize;
                isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, "");

                rect.x += k_IndentSize;
                rect.width -= k_IndentSize;

                GUI.color = labelColor;
                if (isCurrent)
                {
                    // If it's also the current page, print it in bold
                    EditorGUI.LabelField(rect, tocName, EditorStyles.boldLabel);
                }
                else
                {
                    // If not the current page, either allow selection or toggle expanded
                    // based on whether it has a page or not
                    if (GUI.Button(rect, tocName, EditorStyles.label))
                    {
                        if (page != null)
                            NeoFpsHubEditor.instance.currentPage = page;
                        else
                            isExpanded = !isExpanded;
                    }
                }
                GUI.color = Color.white;

                // Draw child entries
                if (isExpanded)
                {
                    for (int i = 0; i < children.Count; ++i)
                        children[i].Draw(width, indent + 1);
                }
            }

            // Show notification
            if (notification != MessageType.None)
            {
                rect.x += rect.width - lineHeight;
                rect.width = lineHeight;
                switch (notification)
                {
                    case MessageType.Error:
                        if (errorIcon != null)
                            GUI.Box(rect, GUIContent.none, errorIcon);
                        break;
                    case MessageType.Warning:
                        if (warningIcon != null)
                            GUI.Box(rect, GUIContent.none, warningIcon);
                        break;
                    case MessageType.Info:
                        if (infoIcon != null)
                            GUI.Box(rect, GUIContent.none, infoIcon);
                        break;
                }
            }
        }

        public int CompareTo(HubTocEntry other)
        {
            return order.CompareTo(other.order);
        }
    }
}