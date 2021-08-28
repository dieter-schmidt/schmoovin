using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;
using NeoFPS.CharacterMotion;
using NeoFPS.SinglePlayer;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace NeoFPSEditor.CharacterMotion.Debugger
{
    public class MotionControllerDebugger : EditorWindow
    {
        private static MotionControllerDebugger s_Instance = null;

        [MenuItem("Tools/NeoFPS/Motion Debugger", priority = 11)]
        public static void CreateWindow()
        {
            CreateWindow(null);
        }

        public static MotionControllerDebugger CreateWindow(MotionController mc)
        {
            var instance = GetWindow<MotionControllerDebugger>();
            instance.titleContent = new GUIContent("Motion Debug");
            instance.minSize = new Vector2(400, 200);
            instance.motionController = mc;
            return instance;
        }

        private readonly GUIContent k_AttachAutomaticallyLabel = new GUIContent("Attach Automatically", "Automatically attaches to the player character controller at runtime. Warning: this will clear the buffer on respawn, etc");


        private SerializedObject m_SerializedObject;
        public SerializedObject serializedObject
        {
            get
            {
                if (m_SerializedObject == null)
                    m_SerializedObject = new SerializedObject(this);
                return m_SerializedObject;
            }
        }

        private void OnEnable()
        {
            CheckBuffer();
            m_Buffer.onTicked += OnTickBuffer;
            ResetCharacterEventHandlers();
            ResetStyles();
            s_Instance = this;
        }

        private void OnDisable()
        {
            s_Instance = null;
        }

        private void OnDestroy()
        {
            DestroyImmediate(m_Buffer);
            m_Buffer = null;
            DestroyStyles();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            attachAutomatically = EditorGUILayout.Toggle(k_AttachAutomaticallyLabel, attachAutomatically);

            string error = null;
            if (m_Buffer == null)
                error = "Buffer has not been allocated. Please attach to a character in the scene.";
            else if (motionController == null && EditorApplication.isPlaying && !EditorApplication.isPaused)
                error = "No character attached. Please attach one via the character's motion controller component or using the \"Attach Automatically\" setting above.";
            else if (m_Buffer.count == 0)
                error = "Debug info will be displayed once the character starts moving.";

            LayoutGraph();

            EditorGUILayout.Space();
            m_DetailsFilter = (MotionControllerDebugFilter)EditorGUILayout.EnumFlagsField("Show Details", m_DetailsFilter);
            EditorGUILayout.Space();

            if (error != null)
                EditorGUILayout.HelpBox(error, MessageType.Info);
            else
                InspectFrameDetails(m_InspectingIndex);

            GUILayout.FlexibleSpace();

            serializedObject.ApplyModifiedProperties();
        }

        #region ATTACHED MOTION CONTROLLER

        private MotionController m_MotionController = null;
        public MotionController motionController
        {
            get { return m_MotionController; }
            set
            {
                if (m_MotionController != value)
                {
                    CheckBuffer();

                    // Detach buffer from previous
                    if (m_MotionController != null)
                    {
                        m_MotionController.onTick -= m_Buffer.GetSnapshotFromController;
                        m_MotionController.onDestroy -= OnDestroyMotionController;
                    }

                    // Set value
                    m_MotionController = value;

                    // Attach buffer to new value
                    if (m_MotionController != null)
                    {
                        // Add event handlers
                        m_MotionController.onTick += m_Buffer.GetSnapshotFromController;
                        m_MotionController.onDestroy += OnDestroyMotionController;

                        // Only reset the buffer if switching to a new controller
                        m_Buffer.ResetBuffer();
                        m_Buffer.GetParametersFromController(m_MotionController);
                    }
                }
            }
        }

        void OnDestroyMotionController(MotionController mc)
        {
            if (m_MotionController == mc)
                m_MotionController = null;
        }

        #endregion

        #region BUFFER

        [SerializeField] private MotionControllerDebugBuffer m_Buffer = null;

        void CheckBuffer()
        {
            // Reset data contents
            if (m_Buffer == null)
            {
                m_Buffer = CreateInstance<MotionControllerDebugBuffer>();
                m_Buffer.ResetBuffer();
                EditorUtility.SetDirty(m_Buffer);
            }
        }

        void OnTickBuffer()
        {
            Repaint();
        }

        public int bufferSize
        {
            get { return m_Buffer.bufferSize; }
            set { m_Buffer.bufferSize = value; }
        }

        #endregion

        #region STYLES

        [SerializeField] private GUIStyle m_GraphBackground = null;
        [SerializeField] private GUIStyle m_GraphFrameLabel = null;
        [SerializeField] private GUIStyle m_GraphLimitsLabel = null;
        [SerializeField] private GUIStyle m_GraphCurrentLabel = null;
        [SerializeField] private GUIStyle m_ReadoutHeader = null;
        [SerializeField] private GUIStyle m_ReadoutRowOdd = null;
        [SerializeField] private GUIStyle m_ReadoutRowEven = null;

        void ResetStyles()
        {
            if (m_GraphBackground != null && m_GraphBackground.normal.background != null)
                return;

            m_GraphFrameLabel = new GUIStyle();
            m_GraphFrameLabel.fontSize = 10;
            m_GraphFrameLabel.normal.textColor = Color.white;
            m_GraphFrameLabel.padding = new RectOffset(3, 3, 2, 2);
            m_GraphFrameLabel.clipping = TextClipping.Overflow;
            m_GraphFrameLabel.alignment = TextAnchor.MiddleCenter;
            m_GraphLimitsLabel = new GUIStyle(m_GraphFrameLabel);
            m_GraphCurrentLabel = new GUIStyle(m_GraphFrameLabel);

            m_ReadoutHeader = new GUIStyle();
            m_ReadoutHeader.fontStyle = FontStyle.Bold;
            m_ReadoutHeader.alignment = TextAnchor.MiddleCenter;

            m_ReadoutRowOdd = new GUIStyle();
            m_ReadoutRowOdd.padding = new RectOffset(2, 2, 2, 2);
            m_ReadoutRowEven = new GUIStyle(m_ReadoutRowOdd);

            if (EditorGUIUtility.isProSkin)
            {
                m_GraphBackground = new GUIStyle();
                m_GraphBackground.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.4f, 0.4f, 0.4f));
                m_ReadoutHeader.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
                m_GraphFrameLabel.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.6f, 0.6f, 0.6f));
                m_GraphLimitsLabel.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.4f, 0.7f, 0.7f));
                m_GraphCurrentLabel.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.4f, 0.7f, 0.4f));
                m_ReadoutRowOdd.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.6f, 0.6f, 0.6f));
                m_ReadoutRowEven.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.55f, 0.55f, 0.55f));
            }
            else
            {
                m_GraphBackground = new GUIStyle();
                m_GraphBackground.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.6f, 0.6f, 0.6f));
                m_ReadoutHeader.normal.textColor = new Color(0.1f, 0.1f, 0.1f);
                m_GraphFrameLabel.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.5f, 0.5f, 0.5f));
                m_GraphLimitsLabel.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.2f, 0.5f, 0.5f));
                m_GraphCurrentLabel.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.2f, 0.5f, 0.2f));
                m_ReadoutRowOdd.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.7f, 0.7f, 0.7f));
                m_ReadoutRowEven.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.65f, 0.65f, 0.65f));
            }
        }

        void DestroyStyles()
        {
            DestroyImmediate(m_GraphBackground.normal.background);
            DestroyImmediate(m_GraphFrameLabel.normal.background);
            DestroyImmediate(m_GraphLimitsLabel.normal.background);
            DestroyImmediate(m_GraphCurrentLabel.normal.background);
            DestroyImmediate(m_ReadoutRowOdd.normal.background);
            DestroyImmediate(m_ReadoutRowEven.normal.background);
        }

        #endregion

        #region GRAPH

        // Add in graph scroll
        //[SerializeField] private Vector2 m_GraphScroll = Vector2.zero;
        [SerializeField] private GraphContents m_GraphReadout = GraphContents.Speed;

        const float k_GraphHeight = 256;

        delegate float GraphValueGetter(int index);

        private int[] m_IntValues = null;
        private float[] m_FloatValues = null;
        private bool[] m_BoolValues = null;
        private string[] m_StringValues = null;
        private Rect m_GraphRect = Rect.zero;
        private int m_InspectingIndex = 0;
        private bool m_Dragging = false;
        private float m_MinValue = -1f;
        private float m_MaxValue = 5f;
        private float m_GraphMinBound = -2f;
        private float m_GraphMaxBound = 6f;

        public Material graphLineMaterial
        {
            get;
            private set;
        }

        bool labelsOnLeft
        {
            get { return m_InspectingIndex < (m_GraphRect.width * 0.25f); }
        }

        void InitialiseGraph()
        {
            bool resetRequired = false;

            if (graphLineMaterial == null)
            {
                var guids = AssetDatabase.FindAssets("MotionControllerDebuggerGraphShader t:Shader");
                if (guids != null && guids.Length > 0)
                {
                    graphLineMaterial = new Material(
                        AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(guids[0]))
                        );
                }
                resetRequired = true;
            }

            if (resetRequired || (m_FloatValues == null && m_IntValues == null && m_BoolValues == null && m_StringValues == null) ||
                (m_FloatValues != null && m_FloatValues.Length != m_Buffer.bufferSize) ||
                (m_IntValues != null && m_IntValues.Length != m_Buffer.bufferSize) ||
                (m_BoolValues != null && m_BoolValues.Length != m_Buffer.bufferSize) ||
                (m_StringValues != null && m_StringValues.Length != m_Buffer.bufferSize))
                ResetGraph();
        }

        void ResetGraph()
        {
            // Release existing array
            m_IntValues = null;
            m_FloatValues = null;
            m_BoolValues = null;
            m_StringValues = null;

            switch (m_GraphReadout)
            {
                case GraphContents.IsGrounded:
                    m_BoolValues = new bool[m_Buffer.bufferSize];
                    break;
                //case GraphContents.State:
                //    m_StringValues = new string[m_Buffer.bufferSize];
                //    break;
                case GraphContents.Depenetrations:
                    m_IntValues = new int[m_Buffer.bufferSize];
                    break;
                case GraphContents.MoveIterations:
                    m_IntValues = new int[m_Buffer.bufferSize];
                    break;
                default:
                    m_FloatValues = new float[m_Buffer.bufferSize];
                    break;
            }
        }

        void LayoutGraph()
        {
            // Show readout dropdown and reset buffers if changed
            var readoutProp = serializedObject.FindProperty("m_GraphReadout");
            int oldIndex = readoutProp.enumValueIndex;
            EditorGUILayout.PropertyField(readoutProp);
            if (readoutProp.enumValueIndex != oldIndex)
            {
                serializedObject.ApplyModifiedProperties();
                ResetGraph();
            }

            EditorGUILayout.Space();

            InitialiseGraph();

            float height = 256 + EditorGUIUtility.singleLineHeight;

            m_GraphRect = EditorGUILayout.BeginVertical(m_GraphBackground, GUILayout.Height(height), GUILayout.MinHeight(height));
            GUILayout.FlexibleSpace();
            GUILayout.HorizontalScrollbar(0f, 10f, 0f, 10f);
            EditorGUILayout.EndVertical();

            // Reset the inspecting index (vertical marker) in play mode
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
                m_InspectingIndex = 0;

            // Plot the graph of points
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    {
                        if ((!EditorApplication.isPlaying || EditorApplication.isPaused) && Event.current.button == 0 && m_Buffer != null && m_Buffer.count > 0)
                        {
                            var mp = Event.current.mousePosition;
                            if (mp.y > m_GraphRect.y && mp.y < (m_GraphRect.y + 256))
                            {
                                int xIndex = Mathf.FloorToInt((m_GraphRect.width - mp.x) / 2);
                                m_InspectingIndex = xIndex;
                                m_Dragging = true;
                                SceneView.RepaintAll();
                            }
                        }
                    }
                    break;
                case EventType.MouseDrag:
                    {
                        if (m_Dragging)
                        {
                            var mp = Event.current.mousePosition;
                            int xIndex = Mathf.FloorToInt((m_GraphRect.width - mp.x) / 2);
                            m_InspectingIndex = Mathf.Clamp(xIndex, 0, m_Buffer.count - 1);
                            Repaint();
                            SceneView.RepaintAll();
                        }
                    }
                    break;
                case EventType.MouseUp:
                    m_Dragging = false;
                    break;
                case EventType.Repaint:
                    {
                        int count = 0;
                        switch (m_GraphReadout)
                        {
                            case GraphContents.Speed:
                                count = m_Buffer.GetValues(GraphContents.Speed, m_FloatValues);
                                DrawFloatGraph(count);
                                break;
                            case GraphContents.RawSpeed:
                                count = m_Buffer.GetValues(GraphContents.RawSpeed, m_FloatValues);
                                DrawFloatGraph(count);
                                break;
                            case GraphContents.HorizontalSpeed:
                                count = m_Buffer.GetValues(GraphContents.HorizontalSpeed, m_FloatValues);
                                DrawFloatGraph(count);
                                break;
                            case GraphContents.RawHorizontalSpeed:
                                count = m_Buffer.GetValues(GraphContents.RawHorizontalSpeed, m_FloatValues);
                                DrawFloatGraph(count);
                                break;
                            case GraphContents.UpVelocity:
                                count = m_Buffer.GetValues(GraphContents.UpVelocity, m_FloatValues);
                                DrawFloatGraph(count);
                                break;
                            case GraphContents.RawUpVelocity:
                                count = m_Buffer.GetValues(GraphContents.RawUpVelocity, m_FloatValues);
                                DrawFloatGraph(count);
                                break;
                            case GraphContents.WorldHeight:
                                count = m_Buffer.GetValues(GraphContents.WorldHeight, m_FloatValues);
                                DrawFloatGraph(count);
                                break;
                            case GraphContents.IsGrounded:
                                count = m_Buffer.GetValues(GraphContents.IsGrounded, m_BoolValues);
                                DrawBoolGraph(count);
                                break;
                            //case GraphContents.State:
                            //    count = m_Buffer.GetValues(GraphContents.State, m_StringValues);
                            //    DrawStringGraph(count);
                            //    break;
                            case GraphContents.GroundSlope:
                                count = m_Buffer.GetValues(GraphContents.GroundSlope, m_FloatValues);
                                DrawFloatGraph(count);
                                break;
                            case GraphContents.GroundSurfaceSlope:
                                count = m_Buffer.GetValues(GraphContents.GroundSurfaceSlope, m_FloatValues);
                                DrawFloatGraph(count);
                                break;
                            case GraphContents.InputScale:
                                count = m_Buffer.GetValues(GraphContents.InputScale, m_FloatValues);
                                DrawFloatGraph(count);
                                break;
                            case GraphContents.ExternalForceMagnitude:
                                count = m_Buffer.GetValues(GraphContents.ExternalForceMagnitude, m_FloatValues);
                                DrawFloatGraph(count);
                                break;
                            case GraphContents.Depenetrations:
                                count = m_Buffer.GetValues(GraphContents.Depenetrations, m_IntValues);
                                DrawIntGraph(count);
                                break;
                            case GraphContents.MoveIterations:
                                count = m_Buffer.GetValues(GraphContents.MoveIterations, m_IntValues);
                                DrawIntGraph(count);
                                break;
                        }
                    }
                    break;
            }
        }

        void DrawHorizontalLine(float yValue, Color c)
        {
            GL.Begin(GL.LINE_STRIP);
            GL.Color(new Color(0.4f, 0.4f, 0.4f));
            var y = (yValue - m_GraphMinBound) / (m_GraphMaxBound - m_GraphMinBound);
            y = m_GraphRect.y + k_GraphHeight - (y * k_GraphHeight);
            GL.Vertex3(m_GraphRect.width - 1f, y, 0);
            GL.Vertex3(1f, y, 0);
            GL.End();
        }

        void DrawKeyValue(string value, float yValue, bool left)
        {
            float y = (yValue - m_GraphMinBound) / (m_GraphMaxBound - m_GraphMinBound);
            y = m_GraphRect.y + k_GraphHeight - (y * k_GraphHeight);

            var content = new GUIContent(value);
            var size = m_GraphLimitsLabel.CalcSize(content);
            EditorGUI.LabelField(
                new Rect((left) ? 2f : m_GraphRect.width - 2f - size.x, y - size.y * 0.5f, size.x, size.y),
                content, m_GraphLimitsLabel
                );
        }

        void DrawFrameMarker(string value, float yValue, bool left)
        {
            float x = m_GraphRect.width - 1f - (2f * m_InspectingIndex);
            var content = new GUIContent("Frame: " + m_Buffer.GetFrameNumber(m_InspectingIndex));
            var size = m_GraphLimitsLabel.CalcSize(content);

            float clampedX = Mathf.Clamp(x - size.x * 0.5f, 0, m_GraphRect.width - size.x);
            EditorGUI.LabelField(
                new Rect(clampedX, m_GraphRect.y + 1, size.x, size.y),
                content, m_GraphFrameLabel
            );

            float y = (yValue - m_GraphMinBound) / (m_GraphMaxBound - m_GraphMinBound);
            y = m_GraphRect.y + k_GraphHeight - (y * k_GraphHeight);

            content = new GUIContent(value);
            size = m_GraphLimitsLabel.CalcSize(content);
            EditorGUI.LabelField(
                new Rect((left) ? x - 4 - size.x : x + 4, y - size.y * 0.5f, size.x, size.y),
                content, m_GraphLimitsLabel
                );
        }

        void DrawFrameMarkerLine()
        {
            // Draw the min line
            GL.Begin(GL.LINE_STRIP);
            GL.Color(Color.white);
            var x = m_GraphRect.width - 1 - (2f * m_InspectingIndex);
            GL.Vertex3(x, m_GraphRect.y + k_GraphHeight, 0);
            GL.Vertex3(x, m_GraphRect.y, 0);
            GL.End();
        }


        void DrawGraph(GraphValueGetter valueFunc)
        {
            GL.Begin(GL.LINE_STRIP);
            GL.Color(Color.green);
            try
            {
                for (int i = 0; i < m_Buffer.count; ++i)
                {
                    var x = (m_GraphRect.width - 1f) - (i * 2f);
                    var y = (valueFunc(i) - m_GraphMinBound) / (m_GraphMaxBound - m_GraphMinBound);
                    y = m_GraphRect.y + 256 - (y * k_GraphHeight);
                    GL.Vertex3(x, y, 0);
                }
            }
            catch { }
            GL.End();
        }
        
        void DrawFloatGraph(int count)
        {
            // Get min / max
            m_MinValue = 0f;
            m_MaxValue = 1f;
            for (int i = 0; i < count; ++i)
            {
                if (m_FloatValues[i] < m_MinValue)
                    m_MinValue = m_FloatValues[i];
                if (m_FloatValues[i] > m_MaxValue)
                    m_MaxValue = m_FloatValues[i];
            }

            // Add a bit of padding at top and bottom of graph
            float diff = m_MaxValue - m_MinValue;
            m_GraphMinBound = m_MinValue - diff * 0.1f;
            m_GraphMaxBound = m_MaxValue + diff * 0.1f;

            graphLineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadPixelMatrix();

            // draw the limits lines
            if (m_MinValue < 0f && m_MaxValue > 0f)
                DrawHorizontalLine(0f, new Color(0.4f, 0.4f, 0.4f));
            DrawHorizontalLine(m_MinValue, new Color(0.3f, 0.3f, 0.3f));
            DrawHorizontalLine(m_MaxValue, new Color(0.3f, 0.3f, 0.3f));

            // Draw the value graph
            DrawGraph((index) => { return m_FloatValues[index]; });

            // Draw the vertical index marker
            if (m_InspectingIndex != 0)
                DrawFrameMarkerLine();

            GL.PopMatrix();

            // Draw the limits labels
            if (m_MinValue < 0f && m_MaxValue > 0f)
                DrawKeyValue("Zero: 0.0", 0f, labelsOnLeft);
            DrawKeyValue("Min: " + m_MinValue.ToString("F3"), m_MinValue, labelsOnLeft);
            DrawKeyValue("Max: " + m_MaxValue.ToString("F3"), m_MaxValue, labelsOnLeft);

            // Draw the frame marker
            if (m_InspectingIndex != 0)
            {
                float value = m_FloatValues[m_InspectingIndex];
                DrawFrameMarker("Value: " + value.ToString("F2"), value, labelsOnLeft);
            }
        }

        void DrawIntGraph(int count)
        {
            // Get min / max
            m_MinValue = 0f;
            m_MaxValue = 1f;
            for (int i = 0; i < count; ++i)
            {
                if (m_IntValues[i] < m_MinValue)
                    m_MinValue = m_IntValues[i];
                if (m_IntValues[i] > m_MaxValue)
                    m_MaxValue = m_IntValues[i];
            }

            // Add a bit of padding at top and bottom of graph
            float diff = m_MaxValue - m_MinValue;
            m_GraphMinBound = m_MinValue - diff * 0.1f;
            m_GraphMaxBound = m_MaxValue + diff * 0.1f;

            graphLineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadPixelMatrix();

            // draw the limits lines
            if (m_MinValue < 0f && m_MaxValue > 0f)
                DrawHorizontalLine(0f, new Color(0.4f, 0.4f, 0.4f));
            DrawHorizontalLine(m_MinValue, new Color(0.3f, 0.3f, 0.3f));
            DrawHorizontalLine(m_MaxValue, new Color(0.3f, 0.3f, 0.3f));

            // Draw the value graph
            DrawGraph((index) => { return m_IntValues[index]; });

            // Draw the vertical index marker
            if (m_InspectingIndex != 0)
                DrawFrameMarkerLine();

            GL.PopMatrix();

            // Draw the limits labels
            if (m_MinValue < 0f && m_MaxValue > 0f)
                DrawKeyValue("Zero: 0", 0f, labelsOnLeft);
            DrawKeyValue("Min: " + m_MinValue.ToString(), m_MinValue, labelsOnLeft);
            DrawKeyValue("Max: " + m_MaxValue.ToString(), m_MaxValue, labelsOnLeft);

            // Draw the frame marker
            if (m_InspectingIndex != 0)
            {
                int value = m_IntValues[m_InspectingIndex];
                DrawFrameMarker("Value: " + value.ToString(), value, labelsOnLeft);
            }
        }

        void DrawBoolGraph(int count)
        {
            // Get min / max
            m_MinValue = 0f;
            m_MaxValue = 1f;

            // Add a bit of padding at top and bottom of graph
            m_GraphMinBound = -0.25f;
            m_GraphMaxBound = 1.25f;

            graphLineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadPixelMatrix();

            // draw the limits lines
            DrawHorizontalLine(m_MinValue, new Color(0.3f, 0.3f, 0.3f));
            DrawHorizontalLine(m_MaxValue, new Color(0.3f, 0.3f, 0.3f));

            // Draw the value graph
            DrawGraph((index) => { return m_BoolValues[index] ? 1 : 0; });

            // Draw the vertical index marker
            if (m_InspectingIndex != 0)
                DrawFrameMarkerLine();

            GL.PopMatrix();

            // Draw the limits labels
            DrawKeyValue("False", m_MinValue, labelsOnLeft);
            DrawKeyValue("True", m_MaxValue, labelsOnLeft);

            // Draw the frame marker
            if (m_InspectingIndex != 0)
            {
                bool value = m_BoolValues[m_InspectingIndex];
                DrawFrameMarker("Value: " + value.ToString(), value ? 1f : 0f, labelsOnLeft);
            }
        }

        void DrawStringGraph(int count)
        {
            // Get list of strings if required
        }
        
        #endregion

        #region FRAME DETAILS

        [SerializeField] private Vector2 m_DetailsScroll = Vector2.zero;
        [SerializeField] private MotionControllerDebugFilter m_DetailsFilter =
            MotionControllerDebugFilter.PreviousMove |
            MotionControllerDebugFilter.IsGrounded |
            MotionControllerDebugFilter.Position |
            MotionControllerDebugFilter.Speed |
            MotionControllerDebugFilter.SlopeAngles;

        private bool m_OddLine = true;
        
        bool CheckFilter(MotionControllerDebugFilter filter)
        {
            return (m_DetailsFilter & filter) != MotionControllerDebugFilter.None;
        }

        void InspectFrameDetails(int frameOffset)
        {
            m_DetailsScroll = EditorGUILayout.BeginScrollView(m_DetailsScroll);//, false, true);

            m_OddLine = true;
            if (frameOffset < m_Buffer.count - 1)
            {
                DrawMultiFrameDetails(frameOffset);
                DrawMultiFrameParameters(frameOffset);
            }
            else
            {
                DrawSingleFrameDetails(frameOffset);
                DrawSingleFrameParameters(frameOffset);
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawSingleFrameDetails(int frameOffset)
        {
            var snapshot = m_Buffer.GetSnapshot(frameOffset);

            LayoutDetailsElement("Frame", snapshot.frame.ToString(), null);

            LayoutDetailsHeader("Output");

            LayoutDetailsElement("Motion State", snapshot.state, null);
            LayoutDetailsElement("Motion State Type", snapshot.stateType, null);
            LayoutDetailsElement("Target Move", snapshot.targetMove.ToString("F3"), null);
            LayoutDetailsElement("Apply Gravity", snapshot.applyGravity.ToString());
            LayoutDetailsElement("Snap To Ground", snapshot.snapToGround.ToString());

            LayoutDetailsHeader("Input");

            if (CheckFilter(MotionControllerDebugFilter.PreviousMove))
                LayoutDetailsElement("Previous Move", snapshot.previousMove.ToString("F3"));
            if (CheckFilter(MotionControllerDebugFilter.IsGrounded))
                LayoutDetailsElement("Is Grounded", snapshot.isGrounded.ToString());
            if (CheckFilter(MotionControllerDebugFilter.Position))
                LayoutDetailsElement("Position", snapshot.position.ToString("F3"));
            if (CheckFilter(MotionControllerDebugFilter.Rotation))
                LayoutDetailsElement("Rotation", snapshot.rotation.ToString("F3"));
            if (CheckFilter(MotionControllerDebugFilter.Input))
            {
                LayoutDetailsElement("Input Direction", snapshot.inputDirection.ToString("F3"));
                LayoutDetailsElement("Input Scale", snapshot.inputScale.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.Velocity))
            {
                LayoutDetailsElement("Velocity", snapshot.velocity.ToString("F3"));
                LayoutDetailsElement("Velocity (Raw)", snapshot.rawVelocity.ToString("F3"));
                LayoutDetailsElement("Velocity (Target)", snapshot.targetVelocity.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.Speed))
            {
                LayoutDetailsElement("Speed", snapshot.velocity.magnitude.ToString("F3"));
                LayoutDetailsElement("Speed (Raw)", snapshot.rawVelocity.magnitude.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.HorizontalSpeed))
            {
                LayoutDetailsElement("Horizontal Speed", Vector3.ProjectOnPlane(snapshot.velocity, snapshot.upTarget).magnitude.ToString("F3"));
                LayoutDetailsElement("Horizontal Speed (Raw)", Vector3.ProjectOnPlane(snapshot.rawVelocity, snapshot.upTarget).magnitude.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.UpVelocity))
            {
                LayoutDetailsElement("Up Velocity", Vector3.Dot(snapshot.velocity, snapshot.upTarget).ToString("F3"));
                LayoutDetailsElement("Up Velocity (Raw)", Vector3.Dot(snapshot.rawVelocity, snapshot.upTarget).ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.CollisionFlags))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Collision Flags");
                EditorGUILayout.EnumFlagsField(snapshot.collisionFlags);
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
            }
            if (CheckFilter(MotionControllerDebugFilter.GroundNormals))
            {
                LayoutDetailsElement("Ground Normal", snapshot.groundNormal.ToString("F3"));
                LayoutDetailsElement("Ground Surface Normal", snapshot.groundSurfaceNormal.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.SlopeAngles))
            {
                if (snapshot.isGrounded)
                {
                    LayoutDetailsElement("Ground Slope", Vector3.Angle(snapshot.groundNormal, snapshot.upTarget).ToString("F3"));
                    LayoutDetailsElement("Ground Surface Slope", Vector3.Angle(snapshot.groundSurfaceNormal, snapshot.upTarget).ToString("F3"));
                }
                else
                {
                    LayoutDetailsElement("Ground Slope", "<Not Grounded>");
                    LayoutDetailsElement("Ground Surface Slope", "<Not Grounded>");
                }
            }
            if (CheckFilter(MotionControllerDebugFilter.Friction))
            {
                LayoutDetailsElement("Ledge Friction", snapshot.ledgeFriction.ToString("F3"));
                LayoutDetailsElement("Slope Friction", snapshot.slopeFriction.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.Dimensions))
            {
                LayoutDetailsElement("Radius", snapshot.radius.ToString("F3"));
                LayoutDetailsElement("Height", snapshot.height.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.Gravity))
            {
                LayoutDetailsElement("Gravity", snapshot.gravity.ToString("F3"));
                float mag = snapshot.gravity.magnitude;
                LayoutDetailsElement("Gravity Amount", mag.ToString("F3"));
                if (mag > 0.001f)
                    LayoutDetailsElement("Gravity Angle", Vector3.Angle(snapshot.upTarget, Vector3.up).ToString("F3"));
                else
                    LayoutDetailsElement("Gravity Angle", "<Magnitude Too Small>");
            }
            if (CheckFilter(MotionControllerDebugFilter.UpVector))
            {
                LayoutDetailsElement("Up Target", snapshot.upTarget.ToString("F3"));
                Vector3 currentUp = snapshot.rotation * Vector3.up;
                LayoutDetailsElement("Up Angle Offset", Vector3.Angle(snapshot.upTarget, currentUp).ToString("F3"));
                LayoutDetailsElement("World Up Offset", Vector3.Angle(snapshot.upTarget, Vector3.up).ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.GroundSnapping))
            {
                LayoutDetailsElement("Ground Snap Height", snapshot.groundSnapHeight.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.ExternalForces))
            {
                LayoutDetailsElement("Ignore External Forces", snapshot.ignoreExternalForces.ToString());
                LayoutDetailsElement("External Force Move", snapshot.externalForceMove.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.Depenetrations))
                LayoutDetailsElement("Depenetrations", snapshot.depenetrations.ToString());
            if (CheckFilter(MotionControllerDebugFilter.MoveIterations))
                LayoutDetailsElement("Move Iterations", snapshot.moveIterations.ToString());
            if (CheckFilter(MotionControllerDebugFilter.Platforms))
            {
                LayoutDetailsElement("Platform", snapshot.platform);
                LayoutDetailsElement("Ignore Platforms", snapshot.ignorePlatforms.ToString());
            }
        }

        void DrawMultiFrameDetails(int frameOffset)
        {
            var snapshot1 = m_Buffer.GetSnapshot(frameOffset);
            var snapshot2 = m_Buffer.GetSnapshot(frameOffset + 1);

            LayoutDetailsElement("Frame", snapshot1.frame.ToString(), snapshot2.frame.ToString());

            LayoutDetailsHeader("Output");

            LayoutDetailsElement("Motion State", snapshot1.state, snapshot2.state);
            LayoutDetailsElement("Motion State Type", snapshot1.stateType, snapshot2.stateType);
            LayoutDetailsElement("Target Move", snapshot1.targetMove.ToString("F3"), snapshot2.targetMove.ToString("F3"));
            LayoutDetailsElement("Apply Gravity", snapshot1.applyGravity.ToString(), snapshot2.applyGravity.ToString());
            LayoutDetailsElement("Snap To Ground", snapshot1.snapToGround.ToString(), snapshot2.snapToGround.ToString());

            LayoutDetailsHeader("Input");

            if (CheckFilter(MotionControllerDebugFilter.PreviousMove))
                LayoutDetailsElement("Previous Move", snapshot1.previousMove.ToString("F3"), snapshot2.previousMove.ToString("F3"));
            if (CheckFilter(MotionControllerDebugFilter.IsGrounded))
                LayoutDetailsElement("Is Grounded", snapshot1.isGrounded.ToString(), snapshot2.isGrounded.ToString());
            if (CheckFilter(MotionControllerDebugFilter.Position))
                LayoutDetailsElement("Position", snapshot1.position.ToString("F3"), snapshot2.position.ToString("F3"));
            if (CheckFilter(MotionControllerDebugFilter.Rotation))
                LayoutDetailsElement("Rotation", snapshot1.rotation.ToString("F3"), snapshot2.rotation.ToString("F3"));
            if (CheckFilter(MotionControllerDebugFilter.Input))
            {
                LayoutDetailsElement("Input Direction", snapshot1.inputDirection.ToString("F3"), snapshot2.inputDirection.ToString("F3"));
                LayoutDetailsElement("Input Scale", snapshot1.inputScale.ToString("F3"), snapshot2.inputScale.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.Velocity))
            {
                LayoutDetailsElement("Velocity", snapshot1.velocity.ToString("F3"), snapshot2.velocity.ToString("F3"));
                LayoutDetailsElement("Velocity (Raw)", snapshot1.rawVelocity.ToString("F3"), snapshot2.rawVelocity.ToString("F3"));
                LayoutDetailsElement("Velocity (Target)", snapshot1.targetVelocity.ToString("F3"), snapshot2.targetVelocity.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.Speed))
            {
                LayoutDetailsElement("Speed", snapshot1.velocity.magnitude.ToString("F3"), snapshot2.velocity.magnitude.ToString("F3"));
                LayoutDetailsElement("Speed (Raw)", snapshot1.rawVelocity.magnitude.ToString("F3"), snapshot2.rawVelocity.magnitude.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.HorizontalSpeed))
            {
                LayoutDetailsElement("Horizontal Speed",
                    Vector3.ProjectOnPlane(snapshot1.velocity, snapshot1.upTarget).magnitude.ToString("F3"),
                    Vector3.ProjectOnPlane(snapshot2.velocity, snapshot2.upTarget).magnitude.ToString("F3")
                    );
                LayoutDetailsElement("Horizontal Speed (Raw)",
                    Vector3.ProjectOnPlane(snapshot1.rawVelocity, snapshot1.upTarget).magnitude.ToString("F3"),
                    Vector3.ProjectOnPlane(snapshot2.rawVelocity, snapshot2.upTarget).magnitude.ToString("F3")
                    );
            }
            if (CheckFilter(MotionControllerDebugFilter.UpVelocity))
            {
                LayoutDetailsElement("Up Velocity",
                    Vector3.Dot(snapshot1.velocity, snapshot1.upTarget).ToString("F3"),
                    Vector3.Dot(snapshot2.velocity, snapshot2.upTarget).ToString("F3")
                    );
                LayoutDetailsElement("Up Velocity (Raw)",
                    Vector3.Dot(snapshot1.rawVelocity, snapshot1.upTarget).ToString("F3"),
                    Vector3.Dot(snapshot2.rawVelocity, snapshot2.upTarget).ToString("F3")
                    );
            }
            if (CheckFilter(MotionControllerDebugFilter.CollisionFlags))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Collision Flags");
                EditorGUILayout.EnumFlagsField(snapshot2.collisionFlags);
                EditorGUILayout.EnumFlagsField(snapshot1.collisionFlags);
                EditorGUILayout.EndHorizontal();
            }
            if (CheckFilter(MotionControllerDebugFilter.GroundNormals))
            {
                LayoutDetailsElement("Ground Normal", snapshot1.groundNormal.ToString("F3"), snapshot2.groundNormal.ToString("F3"));
                LayoutDetailsElement("Ground Surface Normal", snapshot1.groundSurfaceNormal.ToString("F3"), snapshot2.groundSurfaceNormal.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.SlopeAngles))
            {
                string value1 = "<Not Grounded>";
                string value2 = "<Not Grounded>";

                // Display ground slope
                if (snapshot1.isGrounded)
                    value1 = Vector3.Angle(snapshot1.groundNormal, snapshot1.upTarget).ToString("F3");
                if (snapshot2.isGrounded)
                    value2 = Vector3.Angle(snapshot2.groundNormal, snapshot2.upTarget).ToString("F3");
                LayoutDetailsElement("Ground Slope", value1, value2);

                // Display ground surface slope
                if (snapshot1.isGrounded)
                    value1 = Vector3.Angle(snapshot1.groundSurfaceNormal, snapshot1.upTarget).ToString("F3");
                if (snapshot2.isGrounded)
                    value2 = Vector3.Angle(snapshot2.groundSurfaceNormal, snapshot2.upTarget).ToString("F3");
                LayoutDetailsElement("Ground Surface Slope", value1, value2);
            }
            if (CheckFilter(MotionControllerDebugFilter.Friction))
            {
                LayoutDetailsElement("Ledge Friction", snapshot1.ledgeFriction.ToString("F3"), snapshot2.ledgeFriction.ToString("F3"));
                LayoutDetailsElement("Slope Friction", snapshot1.slopeFriction.ToString("F3"), snapshot2.slopeFriction.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.Dimensions))
            {
                LayoutDetailsElement("Radius", snapshot1.radius.ToString("F3"), snapshot2.radius.ToString("F3"));
                LayoutDetailsElement("Height", snapshot1.height.ToString("F3"), snapshot2.height.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.Gravity))
            {
                LayoutDetailsElement("Gravity", snapshot1.gravity.ToString("F3"), snapshot2.gravity.ToString("F3"));

                float mag1 = snapshot1.gravity.magnitude;
                float mag2 = snapshot2.gravity.magnitude;
                LayoutDetailsElement("Gravity Amount", mag1.ToString("F3"), mag2.ToString("F3"));

                LayoutDetailsElement("Gravity Angle",
                    (mag1 > 0.001f) ? Vector3.Angle(snapshot1.upTarget, Vector3.up).ToString("F3") : "<Magnitude Too Small>",
                    (mag2 > 0.001f) ? Vector3.Angle(snapshot2.upTarget, Vector3.up).ToString("F3") : "<Magnitude Too Small>"
                    );
            }
            if (CheckFilter(MotionControllerDebugFilter.UpVector))
            {
                LayoutDetailsElement("Up Target", snapshot1.upTarget.ToString("F3"), snapshot2.upTarget.ToString("F3"));
                Vector3 currentUp1 = snapshot1.rotation * Vector3.up;
                Vector3 currentUp2 = snapshot2.rotation * Vector3.up;
                LayoutDetailsElement("Up Angle Offset", Vector3.Angle(snapshot1.upTarget, currentUp1).ToString("F3"), Vector3.Angle(snapshot2.upTarget, currentUp2).ToString("F3"));
                LayoutDetailsElement("World Up Offset", Vector3.Angle(snapshot1.upTarget, Vector3.up).ToString("F3"), Vector3.Angle(snapshot2.upTarget, Vector3.up).ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.GroundSnapping))
            {
                LayoutDetailsElement("Ground Snap Height", snapshot1.groundSnapHeight.ToString("F3"), snapshot2.groundSnapHeight.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.ExternalForces))
            {
                LayoutDetailsElement("Ignore External Forces", snapshot1.ignoreExternalForces.ToString(), snapshot2.ignoreExternalForces.ToString());
                LayoutDetailsElement("External Force Move", snapshot1.externalForceMove.ToString("F3"), snapshot2.externalForceMove.ToString("F3"));
            }
            if (CheckFilter(MotionControllerDebugFilter.Depenetrations))
                LayoutDetailsElement("Depenetrations", snapshot1.depenetrations.ToString(), snapshot2.depenetrations.ToString());
            if (CheckFilter(MotionControllerDebugFilter.MoveIterations))
                LayoutDetailsElement("Move Iterations", snapshot1.moveIterations.ToString(), snapshot2.moveIterations.ToString());
            if (CheckFilter(MotionControllerDebugFilter.Platforms))
            {
                LayoutDetailsElement("Platform", snapshot1.platform, snapshot2.platform);
                LayoutDetailsElement("Ignore Platforms", snapshot1.ignorePlatforms.ToString(), snapshot2.ignorePlatforms.ToString());
            }
        }

        void DrawSingleFrameParameters(int frameOffset)
        {
            int index = m_Buffer.GetIndexFromOffset(frameOffset);

            LayoutDetailsHeader("Parameters");

            foreach (var p in m_Buffer.triggerParameters)
            {
                string value = p.buffer[index] ? "trigger set" : "trigger not set";
                LayoutDetailsElement(p.title, value);
            }
            foreach (var p in m_Buffer.switchParameters)
            {
                string value = p.buffer[index] ? "on" : "off";
                LayoutDetailsElement(p.title, value);
            }
            foreach (var p in m_Buffer.intParameters)
                LayoutDetailsElement(p.title, p.buffer[index].ToString());
            foreach (var p in m_Buffer.floatParameters)
                LayoutDetailsElement(p.title, p.buffer[index].ToString("F5"));
            foreach (var p in m_Buffer.vectorParameters)
                LayoutDetailsElement(p.title, p.buffer[index].ToString("F3"));
            foreach (var p in m_Buffer.transformParameters)
                LayoutDetailsElement(p.title, p.buffer[index]);
        }

        void DrawMultiFrameParameters(int frameOffset)
        {
            int index1 = m_Buffer.GetIndexFromOffset(frameOffset);
            int index2 = m_Buffer.GetIndexFromOffset(frameOffset + 1);

            LayoutDetailsHeader("Parameters");

            foreach (var p in m_Buffer.triggerParameters)
            {
                string value1 = p.buffer[index1] ? "trigger set" : "trigger not set";
                string value2 = p.buffer[index2] ? "trigger set" : "trigger not set";
                LayoutDetailsElement(p.title, value1, value2);
            }
            foreach (var p in m_Buffer.switchParameters)
            {
                string value1 = p.buffer[index1] ? "on" : "off";
                string value2 = p.buffer[index2] ? "on" : "off";
                LayoutDetailsElement(p.title, value1, value2);
            }
            foreach (var p in m_Buffer.intParameters)
                LayoutDetailsElement(p.title, p.buffer[index1].ToString(), p.buffer[index2].ToString());
            foreach (var p in m_Buffer.floatParameters)
                LayoutDetailsElement(p.title, p.buffer[index1].ToString("F5"), p.buffer[index2].ToString("F5"));
            foreach (var p in m_Buffer.vectorParameters)
                LayoutDetailsElement(p.title, p.buffer[index1].ToString("F3"), p.buffer[index2].ToString("F5"));
            foreach (var p in m_Buffer.transformParameters)
                LayoutDetailsElement(p.title, p.buffer[index1], p.buffer[index2]);
        }

        void LayoutDetailsElement(string label, string value1, string value2 = null)
        {
            // Get the background style for the row
            GUIStyle style = m_OddLine ? m_ReadoutRowOdd : m_ReadoutRowEven;
            m_OddLine = !m_OddLine;

            EditorGUILayout.BeginHorizontal(style);
            
            // Show title
            EditorGUILayout.LabelField(label);

            // Show older frame first
            if (value2 != null)
                EditorGUILayout.LabelField(value2);
            else
                EditorGUILayout.LabelField("-");

            // Show newer frame second
            EditorGUILayout.LabelField(value1);

            EditorGUILayout.EndHorizontal();
        }

        void LayoutDetailsHeader(string title)
        {
            // Get the background style for the row
            GUIStyle style = m_OddLine ? m_ReadoutRowOdd : m_ReadoutRowEven;
            m_OddLine = !m_OddLine;

            EditorGUILayout.BeginHorizontal(style);

            // Show title
            EditorGUILayout.LabelField(title, m_ReadoutHeader);

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region SCENE GIZMOS
        
        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawGizmoForMyScript(MotionController mc, GizmoType gizmoType)
        {
            if (s_Instance != null && s_Instance.motionController == mc)
            {
                s_Instance.DrawDebugGeo();
            }
        }

        void DrawDebugGeo()
        {
            if (m_InspectingIndex == 0)
                return;

            var snapshot = m_Buffer.GetSnapshot(m_InspectingIndex);

            float radius = snapshot.radius;
            float height = snapshot.height;
            Vector3 position = snapshot.position;
            Quaternion rotation = snapshot.rotation;
            Vector3 up = rotation * Vector3.up;

            // Draw spheres (approximates capsule)
            ExtendedGizmos.DrawSphereMarker(position + up * radius, radius, Color.white);
            if (height > 2f * radius + 0.05f)
                ExtendedGizmos.DrawSphereMarker(position + up * (height - radius), radius, Color.white);

            // Draw input arrow
            if (snapshot.inputScale > 0.01f)
            {
                float angle = Vector2.SignedAngle(snapshot.inputDirection, Vector2.up);
                ExtendedGizmos.DrawArrowMarkerFlat(
                    position + up * height * 0.5f,
                    rotation,
                    angle,
                    snapshot.inputScale,
                    Color.blue
                );
            }

            // Draw velocity arrow
            Vector3 velocity = snapshot.rawVelocity;
            if (velocity.sqrMagnitude > 0.001f)
            {
                ExtendedGizmos.DrawArrowMarker3D(
                    position + up * height * 0.5f,
                    velocity.normalized,
                    velocity.magnitude * 0.2f,
                    Color.cyan
                );
            }

            // Draw ground normals
            if (snapshot.isGrounded)
            {
                Vector3 normal = snapshot.groundNormal;
                Vector3 surfaceNormal = snapshot.groundSurfaceNormal;
                // Draw ground normal
                Vector3 contactPoint = position + (up * radius) - (normal * radius);
                ExtendedGizmos.DrawArrowMarker3D(
                    contactPoint,
                    normal,
                    radius,
                    Color.magenta
                );

                ExtendedGizmos.DrawRay(contactPoint, surfaceNormal, radius, Color.green);
            }
        }

        #endregion

        #region SINGLEPLAYER

        [SerializeField]
        private bool m_AttachAutomatically = false;
        public bool attachAutomatically
        {
            get { return m_AttachAutomatically; }
            set
            {
                if (m_AttachAutomatically != value)
                {
                    m_AttachAutomatically = value;
                    ResetCharacterEventHandlers();
                }
            }
        }

        void OnLocalPlayerCharacterChange(FpsSoloCharacter character)
        {
            if (character == null)
                motionController = null;
            else
                motionController = character.GetComponent<MotionController>();
        }

        void ResetCharacterEventHandlers()
        {
            if (m_AttachAutomatically)
            {
                FpsSoloCharacter.onLocalPlayerCharacterChange += OnLocalPlayerCharacterChange;
                OnLocalPlayerCharacterChange(FpsSoloCharacter.localPlayerCharacter);
            }
            else
            {
                FpsSoloCharacter.onLocalPlayerCharacterChange -= OnLocalPlayerCharacterChange;
            }
        }

        #endregion
    }
}