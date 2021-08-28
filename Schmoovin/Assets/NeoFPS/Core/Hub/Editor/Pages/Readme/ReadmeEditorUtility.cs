using UnityEngine;
using UnityEditor;
using NeoFPS.Hub;
using UnityEngine.Events;

namespace NeoFPSEditor.Hub
{
    public static class ReadmeEditorUtility
    {
        private const string k_BulletPoint = " • ";
        private const int k_BulletPointSize = 16;

        private static GUIStyle m_TitleStyle = null;
        public static GUIStyle titleStyle
        {
            get
            {
                if (m_TitleStyle == null)
                {
                    m_TitleStyle = new GUIStyle(EditorStyles.label);
                    m_TitleStyle.wordWrap = true;
                    m_TitleStyle.fontSize = 26;
                    m_TitleStyle.alignment = TextAnchor.MiddleLeft;
                }
                return m_TitleStyle;
            }
        }

        private static GUIStyle m_H2Style = null;
        public static GUIStyle h2Style
        {
            get
            {
                if (m_H2Style == null)
                {
                    m_H2Style = new GUIStyle(EditorStyles.label);
                    m_H2Style.wordWrap = true;
                    m_H2Style.fontSize = 22;
                }
                return m_H2Style;
            }
        }

        private static GUIStyle m_H3Style = null;
        public static GUIStyle h3Style
        {
            get
            {
                if (m_H3Style == null)
                {
                    m_H3Style = new GUIStyle(EditorStyles.label);
                    m_H3Style.wordWrap = true;
                    m_H3Style.fontSize = 18;
                }
                return m_H3Style;
            }
        }

        private static GUIStyle m_BodyStyle = null;
        public static GUIStyle bodyStyle
        {
            get
            {
                if (m_BodyStyle == null)
                {
                    m_BodyStyle = new GUIStyle(EditorStyles.label);
                    m_BodyStyle.wordWrap = true;
                    m_BodyStyle.fontSize = 14;
                    m_BodyStyle.padding.right = 8;
                }
                return m_BodyStyle;
            }
        }

        private static GUIStyle m_BodyStyleCenter = null;
        public static GUIStyle bodyStyleCenter
        {
            get
            {
                if (m_BodyStyleCenter == null)
                {
                    m_BodyStyleCenter = new GUIStyle(EditorStyles.label);
                    m_BodyStyleCenter.wordWrap = true;
                    m_BodyStyleCenter.fontSize = 14;
                    m_BodyStyleCenter.alignment = TextAnchor.UpperCenter;
                }
                return m_BodyStyleCenter;
            }
        }

        private static GUIStyle m_BulletStyle = null;
        public static GUIStyle bulletStyle
        {
            get
            {
                if (m_BulletStyle == null)
                {
                    m_BulletStyle = new GUIStyle(EditorStyles.label);
                    m_BulletStyle.fontSize = 14;
                }
                return m_BulletStyle;
            }
        }

        private static GUIStyle m_LinkStyle = null;
        public static GUIStyle linkStyle
        {
            get
            {
                if (m_LinkStyle == null)
                {
                    m_LinkStyle = new GUIStyle(EditorStyles.label);
                    m_LinkStyle.wordWrap = false;
                    if (EditorGUIUtility.isProSkin)
                        m_LinkStyle.normal.textColor = new Color(0f, 0.55f, 1f, 1f);
                    else
                        m_LinkStyle.normal.textColor = new Color(0f, 0.47f, 0.855f, 1f);
                    m_LinkStyle.stretchWidth = false;
                    m_LinkStyle.fontSize = 14;
                }
                return m_LinkStyle;
            }
        }

        private static bool m_EditMode = false;
        public static bool editMode
        {
            get { return m_EditMode; }
            set { m_EditMode = value; }
        }

        public static void DrawWebLink(string text, string url)
        {
            // Get the link GUIContent
            var linkContent = new GUIContent(text);

            // Get the button position rect
            var position = GUILayoutUtility.GetRect(linkContent, linkStyle);

            // Draw underline under link text
            Handles.BeginGUI();
            Handles.color = linkStyle.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            // Set hover over cursor
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

            // Draw button using content defined above
            if (GUI.Button(position, linkContent, linkStyle))
                Application.OpenURL(url);
        }

        public static void DrawObjectLink(string label, string text, Object obj)
        {
            EditorGUILayout.BeginHorizontal();

            // Draw the prefix label
            if (!string.IsNullOrEmpty(label))
                EditorGUILayout.PrefixLabel(label);

            // Draw the button
            if (obj == null)
                GUILayout.Label("<Broken Connection>", EditorStyles.boldLabel);
            else
            {
                if (GUILayout.Button(text))
                    EditorGUIUtility.PingObject(obj);
            }

            EditorGUILayout.EndHorizontal();
        }

        public static void DrawActionLink(string label, string text, UnityEvent action)
        {
            EditorGUILayout.BeginHorizontal();

            // Draw the prefix label
            if (!string.IsNullOrEmpty(label))
                EditorGUILayout.PrefixLabel(label);

            // Draw the button
            if (GUILayout.Button(text))
                action.Invoke();

            EditorGUILayout.EndHorizontal();
        }

        public static void EditReadmeHeader(SerializedProperty header)
        {
            EditorGUILayout.LabelField("Header", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(header.FindPropertyRelative("image"));
            EditorGUILayout.PropertyField(header.FindPropertyRelative("darkSkinImage"));
            EditorGUILayout.PropertyField(header.FindPropertyRelative("title"));
            EditorGUILayout.EndVertical();
        }

        public static void EditReadmeSections(SerializedProperty sectionArray)
        {
            EditorGUILayout.LabelField("Sections", EditorStyles.boldLabel);

            // Add section button
            if (GUILayout.Button("Add Section"))
                ++sectionArray.arraySize;
            GUILayout.Space(2);

            for (int i = 0; i < sectionArray.arraySize; ++i)
            {
                var section = sectionArray.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.PropertyField(section.FindPropertyRelative("image"));
                EditorGUILayout.PropertyField(section.FindPropertyRelative("h2Heading"));
                EditorGUILayout.PropertyField(section.FindPropertyRelative("h3Heading"));
                EditorGUILayout.PropertyField(section.FindPropertyRelative("text"));

                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(section.FindPropertyRelative("bulletPoints"), true);
                EditorGUILayout.PropertyField(section.FindPropertyRelative("links"), true);
                EditorGUILayout.PropertyField(section.FindPropertyRelative("highlightObjects"), true);
                EditorGUILayout.PropertyField(section.FindPropertyRelative("actions"), true);
                --EditorGUI.indentLevel;

                EditorGUILayout.BeginHorizontal();

                // Remove
                if (GUILayout.Button("Remove"))
                    SerializedArrayUtility.RemoveAt(sectionArray, i);

                // Move up (if possible)
                if (i <= 0)
                    GUI.enabled = false;
                if (GUILayout.Button("Move Up"))
                    SerializedArrayUtility.Move(sectionArray, i, i - 1);
                GUI.enabled = true;

                // Move down (if possible)
                if (i >= sectionArray.arraySize - 1)
                    GUI.enabled = false;
                if (GUILayout.Button("Move Down"))
                    SerializedArrayUtility.Move(sectionArray, i, i + 1);
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            // Add section button
            GUILayout.Space(2);
            if (GUILayout.Button("Add Section"))
                ++sectionArray.arraySize;
            EditorGUILayout.Space();
        }

        public static void DrawReadmeHeader(ReadmeHeader header, bool inline)
        {
            string title = header.title;
            if (string.IsNullOrEmpty(title))
                title = "<Unnamed>";

            // Draw background & layout
            if (inline)
            {
                EditorGUILayout.Space();
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
            }
            else
                GUILayout.BeginHorizontal("In BigTitle");

            // Draw icon / image
            Texture2D image = (EditorGUIUtility.isProSkin && header.darkSkinImage != null) ? header.darkSkinImage : header.image;
            if (image != null)
            {

                float iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f, image.width);
                float iconHeight = image.height * iconWidth / image.width;
                GUILayout.Label(image, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight));

                // Draw header
                EditorGUILayout.LabelField(title, titleStyle);
                // GUILayout.MinHeight effing sucks, so I can't vertically center align this
                // GUILayout.Label(titleContent, titleStyle, GUILayout.MinHeight(iconHeight - 4));
            }
            else
            {
                // Draw header
                GUILayout.Label(title, titleStyle);
            }

            // End layout
            GUILayout.EndHorizontal();
        }

        public static void DrawReadmeSection(ReadmeSection section, float width)
        {
            // Draw H2 Heading
            if (!string.IsNullOrEmpty(section.h2Heading))
                GUILayout.Label(section.h2Heading, h2Style);

            // Draw H3 Heading
            if (!string.IsNullOrEmpty(section.h3Heading))
            {
                GUILayout.Label(section.h3Heading, h3Style);
                GUILayout.Space(4);
            }

            // Draw image
            if (section.image != null)
            {
                float imageWidth = Mathf.Min(width - 34f, section.image.width);
                float imageHeight = section.image.height * imageWidth / section.image.width;
                GUILayout.Label(section.image, GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));
                GUILayout.Space(4);
            }

            // Draw body text
            if (!string.IsNullOrEmpty(section.text))
                GUILayout.Label(section.text, bodyStyle);

            // Draw bullet points
            foreach (var bp in section.bulletPoints)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(k_BulletPoint, bulletStyle, GUILayout.Width(k_BulletPointSize));
                GUILayout.Label(bp, bodyStyle);
                EditorGUILayout.EndHorizontal();
            }

            // Draw links
            if (section.highlightObjects.Length > 0 || section.actions.Length > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                foreach (var link in section.highlightObjects)
                    DrawObjectLink("Highlight: ", link.text, link.gameObject);
                foreach (var link in section.actions)
                    DrawActionLink("Action: ", link.text, link.action);
                EditorGUILayout.EndVertical();
            }
            foreach (var link in section.links)
                DrawWebLink(link.text, link.url);
            
            GUILayout.Space(10);
        }

        public static bool DrawEditModeCheck(IReadme readme)
        {
#if NEOFPS_INTERNAL
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Edit Mode", EditorStyles.boldLabel);
            m_EditMode = EditorGUILayout.Toggle("Enabled", m_EditMode);

            if (m_EditMode && readme != null)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy To Json"))
                {
                    var json = EditorJsonUtility.ToJson(readme, true);
                    EditorGUIUtility.systemCopyBuffer = json;
                }
                if (GUILayout.Button("Paste From Json"))
                {
                    string json = EditorGUIUtility.systemCopyBuffer;
                    if (!string.IsNullOrEmpty(json))
                        EditorJsonUtility.FromJsonOverwrite(json, readme);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            return m_EditMode;
#else
            return false;
#endif
        }
    }
}
