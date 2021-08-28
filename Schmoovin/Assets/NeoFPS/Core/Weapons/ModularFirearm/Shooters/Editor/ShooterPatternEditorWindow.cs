using UnityEngine;
using UnityEditor;
using NeoFPSEditor.CharacterMotion;
using System;

namespace NeoFPSEditor.ModularFirearms
{
    public class ShooterPatternEditorWindow : EditorWindow
    {
        private const float k_PointSize = 16f;
        private const float k_WindowSize = 400f;
        private const float k_WindowBorder = 4f;

        private UnityEngine.Object m_Target = null;
        private SerializedObject m_SerializedObject = null;
        private Texture2D m_Template = null;
        private float m_BoxDimension = 0f;
        private float m_HalfBoxDimension = 0f;
        private float m_HalfWindowSize = 0f;
        private float m_HalfPointSize = 0f;
        private float m_GridStart = 0f;
        private float m_GridEnd = 0f;
        private int m_SelectedIndex = -1;
        private bool m_Dragging = false;
        private ToolState m_ToolState = ToolState.Move;
        private Vector2 m_SelectedPosition = Vector2.zero;
        GUIStyle m_MiniButtonLeftSelected = null;
        GUIStyle m_MiniButtonMidSelected = null;
        GUIStyle m_MiniButtonRightSelected = null;

        public enum ToolState
        {
            Add,
            Remove,
            Move
        }

        GUIStyle miniButtonLeftSelected
        {
            get
            {
                if (m_MiniButtonLeftSelected == null)
                {
                    m_MiniButtonLeftSelected = new GUIStyle(EditorStyles.miniButtonLeft);
                    m_MiniButtonLeftSelected.normal.background = m_MiniButtonLeftSelected.active.background;
                }
                return m_MiniButtonLeftSelected;
            }
        }
        GUIStyle miniButtonMidSelected
        {
            get
            {
                if (m_MiniButtonMidSelected == null)
                {
                    m_MiniButtonMidSelected = new GUIStyle(EditorStyles.miniButtonMid);
                    m_MiniButtonMidSelected.normal.background = m_MiniButtonMidSelected.active.background;
                }
                return m_MiniButtonMidSelected;
            }
        }
        GUIStyle miniButtonRightSelected
        {
            get
            {
                if (m_MiniButtonRightSelected == null)
                {
                    m_MiniButtonRightSelected = new GUIStyle(EditorStyles.miniButtonRight);
                    m_MiniButtonRightSelected.normal.background = m_MiniButtonRightSelected.active.background;
                }
                return m_MiniButtonRightSelected;
            }
        }

        private GUIContent m_PointIcon = null;
        public GUIContent pointIcon
        {
            get
            {
                if (m_PointIcon == null)
                    m_PointIcon = EditorGUIUtility.IconContent("d_greenLight");
                return m_PointIcon;
            }
        }

        private GUIContent m_SelectedIcon = null;
        public GUIContent selectedIcon
        {
            get
            {
                if (m_SelectedIcon == null)
                    m_SelectedIcon = EditorGUIUtility.IconContent("d_redLight");
                return m_SelectedIcon;
            }
        }

        private GUIStyle m_TemplateStyle = null;
        public GUIStyle templateStyle
        {
            get
            {
                if (m_TemplateStyle == null)
                {
                    m_TemplateStyle = new GUIStyle();
                    m_TemplateStyle.stretchHeight = true;
                    m_TemplateStyle.stretchWidth = true;
                }
                return m_TemplateStyle;
            }
        }

        public static void ShowWindow(UnityEngine.Object target)
        {
            var window = GetWindow<ShooterPatternEditorWindow>(true);
            window.titleContent = new GUIContent("Shooter Patter Editor");
            window.minSize = new Vector2(k_WindowSize, k_WindowSize + 98);
            window.maxSize = new Vector2(k_WindowSize, k_WindowSize + 98);
            window.m_Target = target;
            window.m_SerializedObject = new SerializedObject(target);
        }
        
        void Awake()
        {
            m_BoxDimension = k_WindowSize - k_WindowBorder * 2f;
            m_HalfBoxDimension = m_BoxDimension / 2f;
            m_HalfWindowSize = k_WindowSize / 2f;
            m_HalfPointSize = k_PointSize / 2f;
            m_GridStart = k_WindowBorder + 2f;
            m_GridEnd = k_WindowSize - k_WindowBorder - 2f;
        }

        private void OnDestroy()
        {
            m_Target = null;
            m_SerializedObject = null;
        }

        void ProcessInput(SerializedProperty pattern)
        {
            // Process input
            Event e = Event.current;
            if (e.type != EventType.Ignore && e.type != EventType.Used)
            {
                switch (m_ToolState)
                {
                    case ToolState.Add:
                        {
                            if (e.type == EventType.MouseDown)
                            {
                                ++pattern.arraySize;
                                pattern.GetArrayElementAtIndex(pattern.arraySize - 1).vector2Value = GetNormalisedPoint(e.mousePosition);
                            }
                        }
                        break;
                    case ToolState.Remove:
                        {
                            if (e.type == EventType.MouseDown)
                            {
                                SerializedArrayUtility.RemoveAt(pattern, GetPointUnderCursor(pattern, e.mousePosition));
                            }
                        }
                        break;
                    case ToolState.Move:
                        {
                            switch (e.type)
                            {
                                case EventType.MouseDown:
                                    m_SelectedIndex = GetPointUnderCursor(pattern, e.mousePosition);
                                    if (m_SelectedIndex != -1)
                                    {
                                        m_SelectedPosition = pattern.GetArrayElementAtIndex(m_SelectedIndex).vector2Value;
                                        m_Dragging = true;
                                    }
                                    break;
                                case EventType.MouseDrag:
                                    if (m_Dragging)
                                    {
                                        Vector2 delta = e.delta;
                                        delta /= m_HalfBoxDimension;
                                        delta.y = -delta.y;
                                        m_SelectedPosition += delta;
                                        m_SelectedPosition.x = Mathf.Clamp(m_SelectedPosition.x, -1f, 1f);
                                        m_SelectedPosition.y = Mathf.Clamp(m_SelectedPosition.y, -1f, 1f);
                                    }
                                    break;
                                case EventType.MouseUp:
                                    if (m_SelectedIndex != -1)
                                    {
                                        pattern.GetArrayElementAtIndex(m_SelectedIndex).vector2Value = m_SelectedPosition;
                                        m_Dragging = false;
                                    }
                                    break;
                            }
                        }
                        break;
                }

                Repaint();
            }
        }

        Vector2 GetNormalisedPoint(Vector2 point)
        {
            var result = new Vector2(point.x / m_HalfBoxDimension - 1.02f, 1.01f - point.y / m_HalfBoxDimension);
            return result;
        }

        void OnGUI()
        {
            if (m_Target == null)
            {
                m_SerializedObject = null;
                Close();
                return;
            }

            if (m_SerializedObject == null)
                m_SerializedObject = new SerializedObject(m_Target);
            else
                m_SerializedObject.UpdateIfRequiredOrScript();

            var pattern = m_SerializedObject.FindProperty("m_PatternPoints");
            
            // Draw grid
            DrawGrid();

            // Draw points
            for (int i = 0; i < pattern.arraySize; ++i)
            {
                if (m_Dragging && m_SelectedIndex == i)
                {
                    var rect = PointToRect(m_SelectedPosition);
                    GUI.Label(rect, selectedIcon);
                }
                else
                {
                    var point = pattern.GetArrayElementAtIndex(i);
                    var rect = PointToRect(point.vector2Value);

                    if (m_SelectedIndex == i)
                        GUI.Label(rect, selectedIcon);
                    else
                        GUI.Label(rect, pointIcon);
                }
            }

            // Draw controls
            DrawControls(pattern);

            // Process Input
            ProcessInput(pattern);

            pattern.serializedObject.ApplyModifiedProperties();
        }

        void DrawGrid()
        {
            // Draw background
            var boxRect = new Rect(k_WindowBorder, k_WindowBorder, m_BoxDimension, m_BoxDimension);
            GUI.Box(boxRect, GUIContent.none, MotionGraphEditorStyles.boxDark);

            // Draw template
            if (m_Template != null)
            {
                if (m_Template.width > m_Template.height)
                {
                    float newHeight = ((float)m_Template.height / (float)m_Template.width) * m_BoxDimension;
                    boxRect.height = newHeight;
                    boxRect.y += (m_BoxDimension - newHeight) * 0.5f;
                }
                else if (m_Template.width < m_Template.height)
                {
                    float newWidth = ((float)m_Template.width / (float)m_Template.height) * m_BoxDimension;
                    boxRect.width = newWidth;
                    boxRect.x += (m_BoxDimension - newWidth) * 0.5f;
                }
                GUI.DrawTexture(boxRect, m_Template);
            }

            // Draw grids
            Color lineColour = MotionGraphEditorStyles.gridLineColor;

            Handles.BeginGUI();
            Handles.color = lineColour;

            Handles.DrawLine(
                new Vector3(m_HalfWindowSize, m_GridStart, 0f),
                new Vector3(m_HalfWindowSize, m_GridEnd, 0f)
            );

            Handles.DrawLine(
                new Vector3(m_GridStart, m_HalfWindowSize, 0f),
                new Vector3(m_GridEnd, m_HalfWindowSize, 0f)
            );

            Handles.color = Color.white;
            Handles.EndGUI();

            // Draw axis labels
            GUI.Label(new Rect(m_HalfWindowSize, k_WindowBorder, 24, 18), "1.0", EditorStyles.centeredGreyMiniLabel);
            GUI.Label(new Rect(k_WindowBorder, m_HalfWindowSize, 24, 18), "0.0", EditorStyles.centeredGreyMiniLabel);
            GUI.Label(new Rect(m_HalfWindowSize, k_WindowSize - k_WindowBorder - 18, 24, 18), "0.0", EditorStyles.centeredGreyMiniLabel);
            GUI.Label(new Rect(k_WindowSize - k_WindowBorder - 24, m_HalfWindowSize, 24, 18), "1.0", EditorStyles.centeredGreyMiniLabel);
        }

        [Serializable]
        private class ClipboardIntermediate
        {
            public Vector2[] points = new Vector2[0];

            public ClipboardIntermediate(SerializedProperty prop)
            {
                points = new Vector2[prop.arraySize];
                for (int i = 0; i < prop.arraySize; ++i)
                    points[i] = prop.GetArrayElementAtIndex(i).vector2Value;
            }

            public void Paste (SerializedProperty output)
            {
                output.arraySize = points.Length;
                for (int i = 0; i < points.Length; ++i)
                    output.GetArrayElementAtIndex(i).vector2Value = points[i];
            }
        }

        void DrawControls(SerializedProperty pattern)
        {
            Rect controlsLine = new Rect(k_WindowBorder, k_WindowSize - 2f, m_BoxDimension, EditorGUIUtility.singleLineHeight);

            // Modes
            Rect toolRect = controlsLine;
            toolRect.height += 6f;
            toolRect.width *= 0.333f;

            if (GUI.Button(toolRect, EditorGUIUtility.FindTexture("d_Toolbar Plus"), m_ToolState == ToolState.Add ? miniButtonLeftSelected : EditorStyles.miniButtonLeft))
            {
                m_ToolState = ToolState.Add;
            }

            toolRect.x += toolRect.width;
            if (GUI.Button(toolRect, EditorGUIUtility.FindTexture("d_Toolbar Minus"), m_ToolState == ToolState.Remove ? miniButtonMidSelected : EditorStyles.miniButtonMid))
            {
                m_ToolState = ToolState.Remove;
            }

            toolRect.x += toolRect.width;
            if (GUI.Button(toolRect, EditorGUIUtility.FindTexture("d_MoveTool"), m_ToolState == ToolState.Move ? miniButtonRightSelected : EditorStyles.miniButtonRight))
            {
                m_ToolState = ToolState.Move;
            }

            // Copy button
            controlsLine.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 8f;
            Rect halfRect = controlsLine;
            halfRect.width = halfRect.width / 2 - 1f;
            if (GUI.Button(halfRect, "Copy To JSON"))
            {
                var clipboard = new ClipboardIntermediate(pattern);
                var json = EditorJsonUtility.ToJson(clipboard, true);
                EditorGUIUtility.systemCopyBuffer = json;
            }

            // Paste button
            halfRect.x += halfRect.width + 2f;
            bool canPaste = !string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer);
            if (!canPaste)
                GUI.enabled = false;
            if (GUI.Button(halfRect, "Paste From JSON"))
            {
                var clipboard = JsonUtility.FromJson<ClipboardIntermediate>(EditorGUIUtility.systemCopyBuffer);
                if (clipboard != null)
                {
                    clipboard.Paste(pattern);
                    pattern.serializedObject.ApplyModifiedProperties();
                    m_SelectedIndex = -1;
                }
            }
            GUI.enabled = true;

            // Clear points
            controlsLine.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (GUI.Button(controlsLine, "Clear Points"))
            {
                pattern.arraySize = 0;
            }

            // Position vector controls
            controlsLine.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (m_SelectedIndex >= pattern.arraySize)
                m_SelectedIndex = -1;
            if (m_SelectedIndex == -1)
            {
                GUI.enabled = false;
                EditorGUI.Vector2Field(controlsLine, GUIContent.none, Vector2.zero);
                GUI.enabled = true;
            }
            else
            {
                var point = pattern.GetArrayElementAtIndex(m_SelectedIndex);
                point.vector2Value = EditorGUI.Vector2Field(controlsLine, GUIContent.none, point.vector2Value);
            }

            // Select background template
            controlsLine.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            m_Template = EditorGUI.ObjectField(controlsLine, "Template", m_Template, typeof(Texture2D), false) as Texture2D;
        }

        Rect PointToRect(Vector2 point)
        {
            return new Rect(
                m_HalfWindowSize + point.x * m_HalfBoxDimension - m_HalfPointSize,
                m_HalfWindowSize - point.y * m_HalfBoxDimension - m_HalfPointSize,
                k_PointSize,
                k_PointSize
                );
        }

        int GetPointUnderCursor(SerializedProperty pattern, Vector2 cursor)
        {
            for (int i = 0; i < pattern.arraySize; ++i)
            {
                var r = PointToRect(pattern.GetArrayElementAtIndex(i).vector2Value);
                if (r.Contains(cursor))
                    return i;
            }

            return -1;
        }
    }
}