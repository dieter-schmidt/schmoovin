using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using NeoFPS;
using System;

namespace NeoFPSEditor
{
    public class ProjectHierarchyBrowser : EditorWindow
    {
        static void GetObject(string[] guids, UnityAction<UnityEngine.Object> onPicked, UnityAction onCancelled, GameObjectFilter filter)
        {
            ProjectHierarchyBrowser window = GetWindow<ProjectHierarchyBrowser>(true, "Project Browser", true);
            window.minSize = new Vector2(320, 480);
            window.m_OnPicked = onPicked;
            window.m_OnCancelled = onCancelled;
            window.InitialiseHierarchy(guids, filter);
            s_Instance = window;
        }

        public static void GetPrefab(UnityAction<GameObject> onPicked, UnityAction onCancelled)
        {
            GetPrefab(onPicked, onCancelled, null);
        }

        public static void GetPrefab(UnityAction<GameObject> onPicked, UnityAction onCancelled, GameObjectFilter filter)
        {
            GetObject(AssetDatabase.FindAssets("t:GameObject"),
                (obj) =>
                {
                    var prefab = obj as GameObject;
                    if (prefab != null && onPicked != null)
                        onPicked(prefab);
                },
                onCancelled, filter);
            s_Instance.m_BrowsingPrefabs = true;
        }

        public static void GetPrefabWithComponent<T>(UnityAction<T> onPicked, UnityAction onCancelled) where T : class
        {
            GetObject(AssetDatabase.FindAssets("t:GameObject"),
                (obj) =>
                {
                    var prefab = obj as GameObject;
                    if (prefab != null && onPicked != null)
                        onPicked(prefab.GetComponent<T>());
                },
                onCancelled, FilterComponent<T>);
            s_Instance.m_BrowsingPrefabs = true;
        }

        public static void GetPrefabWithComponent<T>(UnityAction<T> onPicked, UnityAction onCancelled, ComponentFilter<T> filter) where T : class
        {
            GetObject(AssetDatabase.FindAssets("t:GameObject"),
                (obj) =>
                {
                    var prefab = obj as GameObject;
                    if (prefab != null && onPicked != null)
                        onPicked(prefab.GetComponent<T>());
                },
                onCancelled,
                (obj) =>
                {
                    var component = obj.GetComponent<T>();
                    if (component == null)
                        return false;
                    if (filter != null && !filter(component))
                        return false;
                    return true;
                });
            s_Instance.m_BrowsingPrefabs = true;
        }

        public static void GetAsset(UnityAction<ScriptableObject> onPicked, Type t, UnityAction onCancelled)
        {
            GetObject(AssetDatabase.FindAssets("t:" + t.Name),
                (obj) =>
                {
                    var asset = obj as ScriptableObject;
                    if (asset != null && onPicked != null)
                        onPicked(asset);
                },
                onCancelled, null);
            s_Instance.m_BrowsingPrefabs = false;
        }

        public static void GetAsset<T>(UnityAction<T> onPicked, UnityAction onCancelled) where T : class
        {
            GetObject(AssetDatabase.FindAssets("t:" + typeof(T).Name),
                (obj) =>
                {
                    var asset = obj as T;
                    if (asset != null && onPicked != null)
                        onPicked(asset);
                },
                onCancelled, null);
            s_Instance.m_BrowsingPrefabs = false;
        }

        static bool FilterComponent<T>(GameObject obj)
        {
            return obj.GetComponent<T>() != null;
        }

        private static ProjectHierarchyBrowser s_Instance = null;
        
        private Vector2 m_ScrollPosition = Vector2.zero;
        private UnityEngine.Object m_Selection = null;
        private UnityAction<UnityEngine.Object> m_OnPicked = null;
        private UnityAction m_OnCancelled = null;
        private bool m_DoubleClicked = false;
        private bool m_Picked = false;
        private bool m_BrowsingPrefabs = false;
        private string m_NameFilter = string.Empty;
        private List<FolderNode> m_RootFolders = new List<FolderNode>();
        private List<ObjectNode> m_RootObjects = new List<ObjectNode>();
        private List<ObjectNode> m_SortedObjects = new List<ObjectNode>();

        private GUIStyle m_BackgroundStyleNormal = null;
        private GUIStyle m_BackgroundStyleSelected = null;
        private GUIStyle m_FoldoutStyleExpanded = null;
        private GUIStyle m_FoldoutStyleContracted = null;
        private GUIStyle m_NameStyleNormal = null;
        private GUIStyle m_NameStyleSelected = null;

        private GUIContent m_FolderLabelContent = null;
        private GUIContent m_PrefabLabelContent = null;
        private GUIContent m_AssetLabelContent = null;

        private GUIContent objectLabelContent
        {
            get
            {
                if (m_BrowsingPrefabs)
                    return m_PrefabLabelContent;
                else
                    return m_AssetLabelContent;
            }
        }


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
            m_FoldoutStyleExpanded.margin = new RectOffset(0, 0, 4, 0);
            //m_FoldoutStyleExpanded.overflow = new RectOffset(0, 0, 0, 4);
            m_FoldoutStyleContracted = new GUIStyle(m_FoldoutStyleExpanded);
            m_FoldoutStyleContracted.normal.background = EditorGUIUtility.FindTexture("d_Toolbar Plus");

            // Get label styles
            m_NameStyleNormal = new GUIStyle(EditorStyles.label);
            m_NameStyleNormal.margin.left -= 4;
            m_NameStyleSelected = new GUIStyle(m_NameStyleNormal);
            m_NameStyleSelected.normal.textColor = Color.white;
            
            m_FolderLabelContent = EditorGUIUtility.IconContent("Folder Icon");
            m_PrefabLabelContent = EditorGUIUtility.IconContent("d_Prefab Icon");
            m_AssetLabelContent = EditorGUIUtility.IconContent("ScriptableObject Icon");

            m_NameFilter = string.Empty;
        }

        abstract class HierarchyNode
        {
            public abstract void Draw(int indent);
        }

        class FolderNode : HierarchyNode
        {
            private string m_Name = string.Empty;
            private bool m_Expanded = true;

            private List<FolderNode> m_SubFolders = new List<FolderNode>();
            private List<ObjectNode> m_Objects = new List<ObjectNode>();

            public string name
            {
                get { return m_Name; }
            }

            public FolderNode(string name)
            {
                m_Name = name;
                m_Expanded = true;
            }

            public override void Draw(int indent)
            {
                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(s_Instance.position.width - 32)))
                {
                    // Indent
                    if (indent > 0)
                        GUILayout.Space(12 * indent);
                    // Foldout button
                    if (GUILayout.Button(GUIContent.none, m_Expanded ? s_Instance.m_FoldoutStyleExpanded : s_Instance.m_FoldoutStyleContracted))
                        m_Expanded = !m_Expanded;
                    
                    var content = new GUIContent(s_Instance.m_FolderLabelContent);
                    content.text = m_Name;
                    if (GUILayout.Button(content, s_Instance.m_NameStyleNormal, GUILayout.Height(20)))
                        m_Expanded = !m_Expanded;
                }

                if (m_Expanded)
                {
                    for (int i = 0; i < m_SubFolders.Count; ++i)
                        m_SubFolders[i].Draw(indent + 1);
                    for (int i = 0; i < m_Objects.Count; ++i)
                        m_Objects[i].Draw(indent + 1);
                }
            }

            public FolderNode AddSubFolder(string subFolder)
            {
                for (int i = 0; i < m_SubFolders.Count; ++i)
                {
                    if (m_SubFolders[i].m_Name == subFolder)
                        return m_SubFolders[i];
                }

                var result = new FolderNode(subFolder);
                m_SubFolders.Add(result);
                return result;
            }

            public ObjectNode AddObject(UnityEngine.Object obj)
            {
                for (int i = 0; i < m_Objects.Count; ++i)
                {
                    if (m_Objects[i].name == obj.name)
                        return m_Objects[i];
                }

                var result = new ObjectNode(obj);
                m_Objects.Add(result);
                return result;
            }

            public FolderNode GetSubFolder(string subFolder)
            {
                for (int i = 0; i < m_SubFolders.Count; ++i)
                {
                    if (m_SubFolders[i].m_Name == subFolder)
                        return m_SubFolders[i];
                }
                return null;
            }

            public ObjectNode GetObject(string objectName)
            {
                for (int i = 0; i < m_Objects.Count; ++i)
                {
                    if (m_Objects[i].name == objectName)
                        return m_Objects[i];
                }
                return null;
            }
        }

        class ObjectNode : HierarchyNode
        {
            private UnityEngine.Object m_Object = null;
            
            public string name
            {
                get { return m_Object.name; }
            }

            public ObjectNode(UnityEngine.Object obj)
            {
                m_Object = obj;
            }

            public override void Draw(int indent)
            {
                bool isSelected = s_Instance.m_Selection == m_Object;
                var backgroundStyle = isSelected ? s_Instance.m_BackgroundStyleSelected : s_Instance.m_BackgroundStyleNormal;
                var labelStyle = isSelected ? s_Instance.m_NameStyleSelected : s_Instance.m_NameStyleNormal;
                
                using (new EditorGUILayout.HorizontalScope(backgroundStyle, GUILayout.Width(s_Instance.position.width - 32)))
                {
                    // Indent
                    if (indent > 0)
                        GUILayout.Space(12 * indent + 12);
                        
                    var content = new GUIContent(s_Instance.objectLabelContent);
                    content.text = m_Object.name;
                    if (GUILayout.Button(content, labelStyle, GUILayout.Height(20)))
                    {
                        s_Instance.m_Selection = m_Object;
                        if (s_Instance.m_DoubleClicked)
                        {
                            s_Instance.m_Picked = true;
                            s_Instance.Close();
                            throw new ExitGUIException(); // Abort layout / GUI
                        }
                    }
                }
            }
        }

        void InitialiseHierarchy(string[] guids, GameObjectFilter filter)
        {
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadMainAssetAtPath(path);
                if (filter == null || filter(obj as GameObject))
                    AddObject(obj, path);
            }

            // Sort objects
            m_SortedObjects.Sort((lhs, rhs) => { return string.Compare(lhs.name, rhs.name); });
        }

        static readonly char[] k_PathSeparators = { '/', '\\' };

        void AddObject(UnityEngine.Object obj, string path)
        {
            ObjectNode node = null;

            string[] folders = path.Split(k_PathSeparators);
            if (folders.Length <= 2)
            {
                node = new ObjectNode(obj);
                m_RootObjects.Add(node);
            }
            else
            {
                FolderNode itr = null;

                // Get root folder
                for (int i = 0; i < m_RootFolders.Count; ++i)
                {
                    if (m_RootFolders[i].name == folders[1])
                    {
                        itr = m_RootFolders[i];
                        break;
                    }
                }

                // Not found, so add
                if (itr == null)
                {
                    itr = new FolderNode(folders[1]);
                    m_RootFolders.Add(itr);
                }

                // Add sub-folders (if required)
                for (int i = 2; i < folders.Length - 1; ++i)
                    itr = itr.AddSubFolder(folders[i]);

                // Add object
                node = itr.AddObject(obj);
            }

            // Add to sorted objects list
            m_SortedObjects.Add(node);
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
                        if (!string.IsNullOrWhiteSpace(m_NameFilter))
                        {
                            string lowerCaseNameFilter = m_NameFilter.ToLower();
                            for (int i = 0; i < m_SortedObjects.Count; ++i)
                            {
                                if (m_SortedObjects[i].name.ToLower().Contains(lowerCaseNameFilter))
                                    m_SortedObjects[i].Draw(0);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < m_RootFolders.Count; ++i)
                                m_RootFolders[i].Draw(0);
                            for (int i = 0; i < m_RootObjects.Count; ++i)
                                m_RootObjects[i].Draw(0);
                        }
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

            m_RootFolders.Clear();
            m_RootObjects.Clear();
            m_SortedObjects.Clear();
            s_Instance = null;
        }

        public static bool FilterByComponent<T>(GameObject obj) where T : Component
        {
            return (obj.GetComponent<T>() != null);
        }
    }
}