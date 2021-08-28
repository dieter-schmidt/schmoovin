using System;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.CharacterMotion
{
    public static class MotionGraphEditorStyles
    {
        public static GUIStyle inspector {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_Inspector;
            }
        }
        public static GUIStyle viewport
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_Viewport;
            }
        }
        public static GUIStyle box
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_Box;
            }
        }
        public static GUIStyle boxDark
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_BoxDark;
            }
        }
        public static GUIStyle separator
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_Separator;
            }
        }
        public static GUIStyle node
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_Node;
            }
        }
        public static GUIStyle nodeSelected
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_NodeSelected;
            }
        }
        public static GUIStyle nodeDefault
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_NodeDefault;
            }
        }
        public static GUIStyle nodeDefaultSelected
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_NodeDefaultSelected;
            }
        }
        public static GUIStyle nodeActive
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_NodeActive;
            }
        }
        public static GUIStyle nodeActiveSelected
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_NodeActiveSelected;
            }
        }
        public static GUIStyle subGraph
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_SubGraph;
            }
        }
        public static GUIStyle subGraphSelected
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_SubGraphSelected;
            }
        }
        public static GUIStyle subGraphDefault
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_SubGraphDefault;
            }
        }
        public static GUIStyle subGraphDefaultSelected
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_SubGraphDefaultSelected;
            }
        }
        public static GUIStyle subGraphParent
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_SubGraphParent;
            }
        }
        public static GUIStyle subGraphParentSelected
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_SubGraphParentSelected;
            }
        }
        public static GUIStyle connection
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_Connection;
            }
        }
        public static GUIStyle labelCentered
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_LabelCentered;
            }
        }
        public static GUIStyle search
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_Search;
            }
        }
        public static GUIStyle searchCancel
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_SearchCancel;
            }
        }
        public static GUIStyle inspectorHeader
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_InspectorHeader;
            }
        }
        public static GUIStyle optionsButton
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_OptionsButton;
            }
        }
        public static GUIStyle helpButton
        {
            get
            {
                if (!s_Initialised)
                    Initialize();
                return m_HelpButton;
            }
        }
        public static Color gridLineColor { get { return k_GridLineColor; } }
        public static Color nodeTextColor { get { return k_NodeTextColor; } }

        private static GUIStyle m_Inspector = null;
        private static GUIStyle m_Viewport = null;
        private static GUIStyle m_Box = null;
        private static GUIStyle m_BoxDark = null;
        private static GUIStyle m_Separator = null;
        private static GUIStyle m_Node = null;
        private static GUIStyle m_NodeSelected = null;
        private static GUIStyle m_NodeDefault = null;
        private static GUIStyle m_NodeDefaultSelected = null;
        private static GUIStyle m_NodeActive = null;
        private static GUIStyle m_NodeActiveSelected = null;
        private static GUIStyle m_SubGraph = null;
        private static GUIStyle m_SubGraphSelected = null;
        private static GUIStyle m_SubGraphDefault = null;
        private static GUIStyle m_SubGraphDefaultSelected = null;
        private static GUIStyle m_SubGraphParent = null;
        private static GUIStyle m_SubGraphParentSelected = null;
        private static GUIStyle m_Connection = null;
        private static GUIStyle m_LabelCentered = null;
        private static GUIStyle m_Search = null;
        private static GUIStyle m_SearchCancel = null;
        private static GUIStyle m_InspectorHeader = null;
        private static GUIStyle m_OptionsButton = null;
        private static GUIStyle m_HelpButton = null;

        private static bool s_Initialised = false;
        private static readonly Color k_GridLineColor = Color.gray;
        private static readonly Color k_NodeTextColor = new Color(0.9f, 0.9f, 0.9f);
                
        static void Initialize()
        {
            var font = LoadFont("OpenSans-Regular");

            m_Inspector = new GUIStyle ();
            m_Viewport = new GUIStyle();

            m_Box = new GUIStyle ();
            m_Box.border = new RectOffset(2, 2, 2, 2);
            m_Box.alignment = TextAnchor.MiddleCenter;
            m_Box.normal.textColor = Color.white;
            m_BoxDark = new GUIStyle();
            m_BoxDark.border = new RectOffset(2, 2, 2, 2);
            m_BoxDark.alignment = TextAnchor.MiddleCenter;
            m_BoxDark.normal.textColor = Color.white;

            m_Separator = new GUIStyle ();
            m_Separator.stretchWidth = true;
            m_Separator.fixedHeight = 1f;
            m_Separator.clipping = TextClipping.Clip;
            m_Separator.border = new RectOffset (2, 2, 2, 2);

            m_Node = new GUIStyle();
            m_Node.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            m_Node.alignment = TextAnchor.MiddleCenter;
            m_Node.contentOffset = new Vector2(-2f, -2f);
            if (font != null)
                m_Node.font = font;
            m_Node.fontSize = 12;

            m_NodeSelected = new GUIStyle(m_Node);
            m_NodeDefault = new GUIStyle(m_Node);
            m_NodeDefaultSelected = new GUIStyle(m_Node);
            m_NodeActive = new GUIStyle(m_Node);
            m_NodeActiveSelected = new GUIStyle(m_Node);

            m_SubGraph = new GUIStyle();
            m_SubGraph.normal.textColor = Color.white;
            m_SubGraph.alignment = TextAnchor.MiddleCenter;
            m_SubGraph.contentOffset = new Vector2(-2f, -2f);
            if (font != null)
                m_SubGraph.font = font;
            m_SubGraph.fontSize = 12;

            m_SubGraphSelected = new GUIStyle(m_SubGraph);
            m_SubGraphDefault = new GUIStyle(m_SubGraph);
            m_SubGraphDefaultSelected = new GUIStyle(m_SubGraph);
            m_SubGraphParent = new GUIStyle(m_SubGraph);
            m_SubGraphParentSelected = new GUIStyle(m_SubGraph);

            m_Connection = new GUIStyle();
            m_Connection.normal.background = LoadTexture("EditorImage_MGConnectionMarker");

            m_LabelCentered = new GUIStyle();
            m_LabelCentered.alignment = TextAnchor.MiddleCenter;

            m_Search = new GUIStyle();
            m_Search.border = new RectOffset(20, 0, 4, 4);
            m_Search.alignment = TextAnchor.MiddleLeft;
            m_Search.active.textColor = Color.white;
            m_Search.normal.textColor = Color.white;
            m_Search.padding = new RectOffset(2, 0, 0, 1);
            m_Search.contentOffset = new Vector2(20f, 0f);
            m_SearchCancel = new GUIStyle();
            m_SearchCancel.border = new RectOffset(0, 20, 4, 4);

            m_OptionsButton = new GUIStyle();
            m_OptionsButton.fixedHeight = 16f;
            m_OptionsButton.fixedWidth = 16f;
            m_OptionsButton.margin = new RectOffset(2, 2, 4, 2);
            m_HelpButton = new GUIStyle();
            m_HelpButton.normal.background = EditorGUIUtility.Load ("icons/_help.png") as Texture2D;
            m_HelpButton.fixedHeight = 16f;
            m_HelpButton.fixedWidth = 16f;
            m_HelpButton.margin = new RectOffset(2, 2, 2, 2);
            
            if (EditorGUIUtility.isProSkin)
            {
                m_Inspector.normal.background = EditorGUIUtility.Load ("builtin skins/darkskin/images/window back.png") as Texture2D;
                m_Box.normal.background = EditorGUIUtility.Load ("builtin skins/darkskin/images/box.png") as Texture2D;
                m_BoxDark.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/cn box.png") as Texture2D;
                m_Separator.normal.background = EditorGUIUtility.Load ("builtin skins/darkskin/images/pre toolbar.png") as Texture2D;
                m_OptionsButton.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/pane options.png") as Texture2D;

                m_Search.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/search.png") as Texture2D;
                m_SearchCancel.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/searchcancelbutton.png") as Texture2D;

                m_LabelCentered.normal.textColor = new Color(0.705f, 0.705f, 0.705f);
                m_Separator.border = new RectOffset(2, 2, 2, 2);
            }
            else
            {
                m_Inspector.normal.background = EditorGUIUtility.Load ("builtin skins/lightskin/images/window back.png") as Texture2D;
                m_Box.normal.background = EditorGUIUtility.Load ("builtin skins/lightskin/images/box.png") as Texture2D;
                m_BoxDark.normal.background = EditorGUIUtility.Load("builtin skins/lightskin/images/cn box.png") as Texture2D;
                m_Separator.normal.background = EditorGUIUtility.Load ("builtin skins/lightskin/images/pre toolbar.png") as Texture2D;
                m_OptionsButton.normal.background = EditorGUIUtility.Load("builtin skins/lightskin/images/pane options.png") as Texture2D;

                m_Search.normal.background = EditorGUIUtility.Load("builtin skins/lightskin/images/search.png") as Texture2D;
                m_SearchCancel.normal.background = EditorGUIUtility.Load("builtin skins/lightskin/images/searchcancelbutton.png") as Texture2D;

                m_Viewport.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/window back.png") as Texture2D;
                m_Separator.border = new RectOffset(2, 2, 1, 1);
            }

            m_Node.normal.background = LoadTexture("EditorImage_MGState_Blue");
            m_Node.normal.textColor = k_NodeTextColor;
            m_NodeSelected.normal.background = LoadTexture("EditorImage_MGStateSelected_Blue");
            m_NodeSelected.normal.textColor = k_NodeTextColor;
            m_NodeDefault.normal.background = LoadTexture("EditorImage_MGState_Orange");
            m_NodeDefault.normal.textColor = k_NodeTextColor;
            m_NodeDefaultSelected.normal.background = LoadTexture("EditorImage_MGStateSelected_Orange");
            m_NodeDefaultSelected.normal.textColor = k_NodeTextColor;
            m_NodeActive.normal.background = LoadTexture("EditorImage_MGState_Red");
            m_NodeActive.normal.textColor = k_NodeTextColor;
            m_NodeActiveSelected.normal.background = LoadTexture("EditorImage_MGStateSelected_Red");
            m_NodeActiveSelected.normal.textColor = k_NodeTextColor;

            m_SubGraph.normal.background = LoadTexture("EditorImage_MGGraph_Blue");
            m_SubGraph.normal.textColor = k_NodeTextColor;
            m_SubGraphSelected.normal.background = LoadTexture("EditorImage_MGGraphSelected_Blue");
            m_SubGraphSelected.normal.textColor = k_NodeTextColor;
            m_SubGraphDefault.normal.background = LoadTexture("EditorImage_MGGraph_Orange");
            m_SubGraphDefault.normal.textColor = k_NodeTextColor;
            m_SubGraphDefaultSelected.normal.background = LoadTexture("EditorImage_MGGraphSelected_Orange");
            m_SubGraphDefaultSelected.normal.textColor = k_NodeTextColor;
            m_SubGraphParent.normal.background = LoadTexture("EditorImage_MGGraph_Green");
            m_SubGraphParent.normal.textColor = k_NodeTextColor;
            m_SubGraphParentSelected.normal.background = LoadTexture("EditorImage_MGGraphSelected_Green");
            m_SubGraphParentSelected.normal.textColor = k_NodeTextColor;

            s_Initialised = true;
        }

        static Texture2D LoadTexture(string textureName)
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D " + textureName);
            if (guids != null && guids.Length > 0)
                return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
            else
            {
                Debug.LogError("Motion graph editor texture not found: " + textureName);
                return null;
            }
        }

        static Font LoadFont(string fontName)
        {
            var guids = AssetDatabase.FindAssets("t:Font " + fontName);
            if (guids != null && guids.Length > 0)
                return AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(guids[0]));
            else
                return null;
        }

        public static void DrawSeparator ()
        {
            GUILayout.Box("", separator);
        }
    }
}

