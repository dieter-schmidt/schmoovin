using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    public class ObjectHierarchyBrowser : EditorWindow
    {
        public static void GetChildObject(Transform root, bool allowRoot, OnPickedDelegate onPicked, OnCancelledDelegate onCancelled, GameObjectFilter filter = null)
        {
            if (root != null)
            {
                ObjectHierarchyBrowser window = GetWindow<ObjectHierarchyBrowser>(true, "Hierarchy Browser", true);
                window.minSize = new Vector2(320, 480);
                window.m_OnPicked = onPicked;
                window.m_OnCancelled = onCancelled;
                window.InitialiseHierarchy(root, allowRoot, filter);
                s_Instance = window;
            }
        }
        
        public static void GetChildComponent<T>(Transform root, bool allowRoot, OnPickedDelegate onPicked, OnCancelledDelegate onCancelled) where T : class
        {
            if (root != null)
            {
                ObjectHierarchyBrowser window = GetWindow<ObjectHierarchyBrowser>(true, "Hierarchy Browser", true);
                window.minSize = new Vector2(320, 480);
                window.m_OnPicked = onPicked;
                window.m_OnCancelled = onCancelled;
                window.InitialiseHierarchy(root, allowRoot, FilterByComponent<T>);
                s_Instance = window;
            }
        }

        public static void GetChildComponent<T>(Transform root, bool allowRoot, OnPickedDelegate onPicked, OnCancelledDelegate onCancelled, ComponentFilter<T> filter) where T : class
        {
            if (root != null)
            {
                ObjectHierarchyBrowser window = GetWindow<ObjectHierarchyBrowser>(true, "Hierarchy Browser", true);
                window.minSize = new Vector2(320, 480);
                window.m_OnPicked = onPicked;
                window.m_OnCancelled = onCancelled;
                window.InitialiseHierarchy(root, allowRoot,
                (obj) =>
                {
                    var component = obj.GetComponent<T>();
                    if (component == null)
                        return false;
                    if (filter != null && !filter(component))
                        return false;
                    return true;
                });
                s_Instance = window;
            }
        }

        public delegate void OnPickedDelegate(GameObject obj);
        public delegate void OnCancelledDelegate();

        private static ObjectHierarchyBrowser s_Instance = null;

        private Vector2 m_ScrollPosition = Vector2.zero;
        private GameObject m_Selection = null;
        private OnPickedDelegate m_OnPicked = null;
        private OnCancelledDelegate m_OnCancelled = null;
        private bool m_DoubleClicked = false;
        private bool m_Picked = false;
        private string m_NameFilter = string.Empty;
        private HierarchyNode rootNode = null;

        private GUIStyle m_BackgroundStyleNormal = null;
        private GUIStyle m_BackgroundStyleSelected = null;
        private GUIStyle m_FoldoutStyleExpanded = null;
        private GUIStyle m_FoldoutStyleContracted = null;
        private GUIStyle m_NameStyleNormal = null;
        private GUIStyle m_NameStyleSelected = null;

        private GUIContent m_LabelContent = null;

        private void Awake()
        {
            // Get background styles
            m_BackgroundStyleNormal = new GUIStyle();
            m_BackgroundStyleNormal.fixedHeight = 20;
            m_BackgroundStyleSelected = new GUIStyle(m_BackgroundStyleNormal);
            m_BackgroundStyleSelected.normal.background = new GUIStyle("OL SelectedRow").normal.background;

            // Get foldout styles
            m_FoldoutStyleExpanded = new GUIStyle();
            m_FoldoutStyleExpanded.fixedHeight = 12;
            m_FoldoutStyleExpanded.fixedWidth = 12;
            m_FoldoutStyleExpanded.normal.background = EditorGUIUtility.FindTexture("d_Toolbar Minus");
            m_FoldoutStyleExpanded.margin = new RectOffset(0, 0, 4, 4);
            m_FoldoutStyleContracted = new GUIStyle(m_FoldoutStyleExpanded);
            m_FoldoutStyleContracted.normal.background = EditorGUIUtility.FindTexture("d_Toolbar Plus");

            // Get label styles
            m_NameStyleNormal = new GUIStyle(EditorStyles.label);
            m_NameStyleNormal.margin.left -= 4;
            m_NameStyleSelected = new GUIStyle(m_NameStyleNormal);
            m_NameStyleSelected.normal.textColor = Color.white;
            
            m_LabelContent = EditorGUIUtility.IconContent("d_GameObject Icon");

            m_NameFilter = string.Empty;
        }

        class HierarchyNode
        {
            private GameObject m_GameObject = null;
            private bool m_Selectable = true;
            private bool m_Expanded = true;
            private int m_Indent = 0;
            
            public List<HierarchyNode> children { get; private set; }

            public HierarchyNode(GameObject obj, bool selectable, int indent)
            {
                m_GameObject = obj;
                m_Selectable = selectable;
                m_Indent = indent;
            }

            public void Draw(string filter)
            {
                bool isSelected = s_Instance.m_Selection == m_GameObject;
                var backgroundStyle = isSelected ? s_Instance.m_BackgroundStyleSelected : s_Instance.m_BackgroundStyleNormal;
                var labelStyle = isSelected ? s_Instance.m_NameStyleSelected : s_Instance.m_NameStyleNormal;

                if (filter == string.Empty || m_GameObject.name.ToLower().Contains(filter))
                {
                    using (new EditorGUILayout.HorizontalScope(backgroundStyle, GUILayout.Width(s_Instance.position.width - 32)))
                    {
                        if (filter == string.Empty)
                        {
                            // Expand/Collapse
                            if (children != null)
                            {
                                // Indent
                                if (m_Indent > 0)
                                    GUILayout.Space(12 * m_Indent);
                                // Foldout button
                                if (GUILayout.Button(GUIContent.none, m_Expanded ? s_Instance.m_FoldoutStyleExpanded : s_Instance.m_FoldoutStyleContracted))
                                    m_Expanded = !m_Expanded;
                            }
                            else
                            {
                                // Indent
                                if (m_Indent > 0)
                                    GUILayout.Space(12 * m_Indent + 12);
                            }
                        }

                        // Name
                        if (!m_Selectable)
                            GUI.enabled = false;

                        var content = new GUIContent(s_Instance.m_LabelContent);
                        content.text = m_GameObject.name;
                        if (GUILayout.Button(content, labelStyle, GUILayout.Height(20)))
                        {
                            s_Instance.m_Selection = m_GameObject;
                            if (s_Instance.m_DoubleClicked)
                            {
                                s_Instance.m_Picked = true;
                                s_Instance.Close();
                                throw new ExitGUIException(); // Abort layout / GUI
                            }
                        }

                        GUI.enabled = true;

                        // Selection
                    }
                }

                if ((m_Expanded || filter != string.Empty) && children != null)
                {
                    for (int i = 0; i < children.Count; ++i)
                        children[i].Draw(filter);
                }
            }

            public void InitialiseChildren(GameObjectFilter filter)
            {
                var t = m_GameObject.transform;
                if (t.childCount > 0)
                {
                    children = new List<HierarchyNode>(t.childCount);
                    for (int i = 0; i < t.childCount; ++i)
                    {
                        var child = t.GetChild(i).gameObject;
                        var childNode = new HierarchyNode(child, (filter != null) ? filter(child) : true, m_Indent + 1);
                        children.Add(childNode);
                        childNode.InitialiseChildren(filter);
                    }
                }
            }
        }

        void InitialiseHierarchy(Transform root, bool allowRoot, GameObjectFilter filter)
        {
            if (allowRoot && filter != null)
                allowRoot = filter(root.gameObject);
            rootNode = new HierarchyNode(root.gameObject, allowRoot, 0);
            rootNode.InitialiseChildren(filter);
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                m_DoubleClicked = (Event.current.clickCount > 1);
            }

            using (new EditorGUILayout.VerticalScope())
            {
                // Draw the filter
                GUILayout.Space(4);
                m_NameFilter = EditorGUILayout.TextField("Filter", m_NameFilter);

                // Draw the hierarchy list
                var gs = new GUIStyle();
                gs.stretchWidth = false;
                using (var scroller = new EditorGUILayout.ScrollViewScope(m_ScrollPosition, false, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box))
                {
                    m_ScrollPosition = scroller.scrollPosition;

                    using (new EditorGUILayout.VerticalScope())
                    {
                        rootNode.Draw(m_NameFilter.ToLower());
                    }
                }

                // Show select object button (only valid if there is a selection)
                if (m_Selection == null)
                {
                    GUI.enabled = false;
                    GUILayout.Button("Pick: <Nothing Selected>");
                    GUI.enabled = true;
                }
                else
                {
                    if (GUILayout.Button(string.Format("Pick: \"{0}\"", m_Selection.name)))
                    {
                        m_Picked = true;
                        Close();
                    }
                }

                // Show select none button
                if (GUILayout.Button("Pick None"))
                {
                    m_Picked = true;
                    m_Selection = null;
                    Close();
                }
            }
        }

        private void OnLostFocus()
        {
            Close();
        }
        
        private void OnDestroy()
        {
            if (m_Picked)
            {
                if (m_OnPicked != null)
                    m_OnPicked(m_Selection);
                m_Selection = null;
            }
            else
            {
                if (m_OnCancelled != null)
                    m_OnCancelled();
            }
            
            s_Instance = null;
        }

        public static bool FilterByComponent<T>(GameObject obj) where T : class
        {
            return (obj.GetComponent<T>() != null);
        }
    }
}