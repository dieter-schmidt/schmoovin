using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using NeoFPS.CharacterMotion;

namespace NeoFPSEditor.CharacterMotion
{
    public class MotionGraphEditorViewport
    {
        const EventModifiers k_EventModifiersMask = EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift;
        const float k_DoubleClickTimeout = 0.25f;
        const float k_ViewportOffsetClamp = 2000f;
        const float k_ConnectionClickRange = 8f;
        readonly Vector2 k_NodeCenterOffset = new Vector2(100f, 25f);
        readonly Vector2 k_ArrowOffset = new Vector2(8f, 8f);
        readonly Quaternion k_SidewaysRotation = Quaternion.Euler(0f, 0f, 90f);

        Rect m_NodeRect = new Rect(0f, 0f, 200f, 50f);
        Rect m_ArrowRect = new Rect(0f, 0f, 16f, 16f);
        Rect m_ViewportRect = Rect.zero;
        Rect m_MenuBarRect = Rect.zero;
        Rect m_SelectionRect = Rect.zero;
        Vector2 m_SelectionRectStart = Vector2.zero;
        Vector2 m_ViewportOffset = Vector2.zero;
        Vector2 m_ViewportCenterOffset = Vector2.zero;
        Vector2 m_CursorPosition = Vector2.zero;
        bool m_DraggingViewport = false;
        bool m_DraggingElements = false;
        bool m_DragSelecting = false;
        float m_LastClickTime = 0f;
        MotionGraphConnectable m_TransitionStart = null;
        string m_CurrentContextString = "Root";
        StringBuilder m_CurrentContextStringBuilder = new StringBuilder(256);

        public MotionGraphEditor editor { get; private set; }

        public MotionGraphEditorViewport (MotionGraphEditor e)
        {
            editor = e;
            editor.onContextChanged += OnContextChanged;
        }

        public void Reset ()
        {
            m_LastClickTime = 0f;
            m_TransitionStart = null;
            m_DragSelecting = false;
            m_DraggingElements = false;
            m_DraggingViewport = false;
            m_SelectionRect = Rect.zero;
            m_ViewportOffset = Vector2.zero;
        }

        public void Draw (Rect rect)
        {
            // Set the relevant rects
            m_MenuBarRect = rect;
            m_MenuBarRect.height = 20f;
            m_ViewportRect = rect;
            m_ViewportRect.position += new Vector2(0f, 18f);
            m_ViewportRect.height -= 18f;

            m_ViewportCenterOffset = m_ViewportRect.center;

            // Draw the background grid
            DrawGrid();

            if (editor.currentGraph == null)
            {
                EditorGUI.HelpBox(new Rect(
                    rect.center - new Vector2(150f, 80f),
                    new Vector2(300f, 80f)),
                    "\nNo motion graph selected for editing.\n\nPlease select one in the project view or using the field at the top of this window.\n",
                    MessageType.Info
                );
            }
            else
            {
                // Draw the selection rect if in use
                if (m_SelectionRect.height > 4f && m_SelectionRect.width > 4f)
                    GUI.Box(m_SelectionRect, "");

                // Draw any pending connection
                DrawPendingConnection(Event.current);

                // Draw graph elements
                DrawConnections();
                DrawStates();
                DrawGraphs();
            }
            // Draw the top bar
            DrawTopBar();

            // Draw help URL button
            if (GUI.Button(new Rect (rect.xMax - 24, rect.yMax - 24, 20, 20), "", MotionGraphEditorStyles.helpButton))
                Application.OpenURL("http://docs.neofps.com/manual/motiongraph-editor.html");
        }

        #region DRAWING

        void DrawTopBar()
        {
            GUILayout.BeginArea(m_MenuBarRect, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.Label(m_CurrentContextString);
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void DrawGrid()
        {
            GUILayout.BeginArea(m_ViewportRect, MotionGraphEditorStyles.viewport);
            GUILayout.EndArea();

            Vector2 offset = m_ViewportOffset + m_ViewportCenterOffset;

            float spacing = 20f;
            Color lineColour = MotionGraphEditorStyles.gridLineColor;

            float lineOffsetX = m_ViewportOffset.x % spacing;
            float lineOffsetY = m_ViewportOffset.y % spacing;
            int intOffsetX = Mathf.CeilToInt(Mathf.Abs(offset.x / spacing)) * -(int)Mathf.Sign(offset.x);
            int intOffsetY = Mathf.CeilToInt(Mathf.Abs(offset.y / spacing)) * -(int)Mathf.Sign(offset.y);

            Handles.BeginGUI();

            // Draw verticals
            for (; lineOffsetX < m_ViewportRect.width; lineOffsetX += spacing)
            {
                if (intOffsetX++ % 5 == 0)
                    lineColour.a = 0.5f;
                else
                    lineColour.a = 0.2f;
                Handles.color = lineColour;
                Handles.DrawLine(
                    new Vector3(m_ViewportRect.x + lineOffsetX, m_ViewportRect.yMin, 0f),
                    new Vector3(m_ViewportRect.x + lineOffsetX, m_ViewportRect.yMax, 0f)
                );
            }

            // Draw horizontals
            for (; lineOffsetY < m_ViewportRect.height; lineOffsetY += spacing)
            {
                if (intOffsetY++ % 5 == 0)
                    lineColour.a = 0.5f;
                else
                    lineColour.a = 0.2f;
                Handles.color = lineColour;
                Handles.DrawLine(
                    new Vector3(m_ViewportRect.xMin, m_ViewportRect.y + lineOffsetY, 0f), 
                    new Vector3(m_ViewportRect.xMax, m_ViewportRect.y + lineOffsetY, 0f)
                );
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        void DrawStates()
        {
            Vector2 offset = m_ViewportOffset + m_ViewportCenterOffset;
            foreach (MotionGraphState state in editor.currentContext.states)
                DrawState(state, offset);
        }

        void DrawGraphs()
        {
            Vector2 offset = m_ViewportOffset + m_ViewportCenterOffset;
            DrawGraph(editor.currentContext, offset, true);
            foreach (MotionGraph graph in editor.currentContext.subGraphs)
                DrawGraph(graph, offset, false);
        }

        void DrawConnections()
        {
            Vector2 offset = m_ViewportOffset + m_ViewportCenterOffset;

            // Draw connection to default
            if (editor.currentContext.defaultEntry != null)
            {
                Vector2 start = editor.currentContext.internalUiPosition + offset;
                Vector2 end = editor.currentContext.defaultEntry.uiPosition + offset;
                Handles.DrawLine(start, end);
            }

            foreach (var connection in editor.currentContext.connections)
                DrawConnection(connection, offset);
            foreach (var state in editor.currentContext.states)
            {
                if (state != null)
                {
                    foreach (var connection in state.connections)
                        DrawConnection(connection, offset);
                }
            }
            foreach (var graph in editor.currentContext.subGraphs)
            {
                if (graph != null)
                {
                    foreach (var connection in graph.connections)
                        DrawConnection(connection, offset);
                }
            }
        }

        void DrawState(MotionGraphState state, Vector2 offset)
        {
            if (state == null)
                return;

            m_NodeRect.position = state.uiPosition + offset - k_NodeCenterOffset;

            bool selected = Selection.Contains(state);
            if (editor.currentController != null && editor.currentController.currentState == state)
            {
                if (selected)
                    GUI.Box(m_NodeRect, state.name, MotionGraphEditorStyles.nodeActiveSelected);
                else
                    GUI.Box(m_NodeRect, state.name, MotionGraphEditorStyles.nodeActive);
            }
            else
            {
                if (editor.currentContext.defaultEntry == state)
                {
                    if (selected)
                        GUI.Box(m_NodeRect, state.name, MotionGraphEditorStyles.nodeDefaultSelected);
                    else
                        GUI.Box(m_NodeRect, state.name, MotionGraphEditorStyles.nodeDefault);
                }
                else
                {
                    if (selected)
                        GUI.Box(m_NodeRect, state.name, MotionGraphEditorStyles.nodeSelected);
                    else
                        GUI.Box(m_NodeRect, state.name, MotionGraphEditorStyles.node);
                }
            }
        }

        void DrawGraph(MotionGraph graph, Vector2 offset, bool isContext)
        {
            if (graph == null)
                return;
            
            bool selected = Selection.Contains(graph);
            if (isContext)
            {
                m_NodeRect.position = graph.internalUiPosition + offset - k_NodeCenterOffset;
                if (selected)
                    GUI.Box(m_NodeRect, graph.name, MotionGraphEditorStyles.subGraphParentSelected);
                else
                    GUI.Box(m_NodeRect, graph.name, MotionGraphEditorStyles.subGraphParent);
            }
            else
            {
                m_NodeRect.position = graph.uiPosition + offset - k_NodeCenterOffset;
                if (editor.currentContext.defaultEntry == graph)
                {
                    if (selected)
                        GUI.Box(m_NodeRect, graph.name, MotionGraphEditorStyles.subGraphDefaultSelected);
                    else
                        GUI.Box(m_NodeRect, graph.name, MotionGraphEditorStyles.subGraphDefault);
                }
                else
                {
                    if (selected)
                        GUI.Box(m_NodeRect, graph.name, MotionGraphEditorStyles.subGraphSelected);
                    else
                        GUI.Box(m_NodeRect, graph.name, MotionGraphEditorStyles.subGraph);
                }
            }
        }

        void DrawConnection(MotionGraphConnection connection, Vector2 offset)
        {
            if (connection == null)
                return;
            if (connection.source == null || connection.destination == null)
                return;
            if (connection.destination.parent != editor.currentContext)
                return;

            // Get the start and end
            Vector2 start = connection.source.uiPosition;
            var sourceGraph = connection.source as MotionGraph;
            if (sourceGraph != null && sourceGraph == editor.currentContext)
                start = sourceGraph.internalUiPosition;
            start += offset;
            Vector2 end = connection.destination.uiPosition + offset;
            Vector2 direction = end - start;
            // Offset to one side based on direction
            Vector2 side = k_SidewaysRotation * direction;
            side.Normalize();
            side *= 6f;
            start += side;
            end += side;

            // Draw the line
            Handles.DrawLine(start, end);

            // Draw the center arrow
            Vector2 center = (start + end) * 0.5f;
            m_ArrowRect.position = center - k_ArrowOffset;

            float angle = Vector2.Angle(Vector2.down, direction);
            Vector3 cross = Vector3.Cross(Vector2.down, direction);
            if (cross.z < 0)
                angle = 360f - angle;

            GUIUtility.RotateAroundPivot(angle, center);
            GUI.Box(m_ArrowRect, "", MotionGraphEditorStyles.connection);
            GUIUtility.RotateAroundPivot(-angle, center);
        }

        void DrawPendingConnection(Event e)
        {
            if (m_TransitionStart != null)
            {
                Vector2 start = m_TransitionStart.uiPosition;
                var mg = m_TransitionStart as MotionGraph;
                if (mg != null && mg == editor.currentContext)
                    start = mg.internalUiPosition;
                Handles.DrawLine(
                    start + m_ViewportOffset + m_ViewportCenterOffset,
                    e.mousePosition
                );
                GUI.changed = true;
            }
        }

        #endregion

        #region EVENTS

        public void ProcessEvent(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    {
                        m_CursorPosition = e.mousePosition;
                        if (m_TransitionStart != null)
                        {
                            var end = GetConnectableUnderCursor(e.mousePosition);
                            if (end != null && end != m_TransitionStart)
                            {
                                bool duplicate = false;
                                foreach (MotionGraphConnection c in m_TransitionStart.connections)
                                {
                                    if (c.destination == end)
                                    {
                                        duplicate = true;
                                        break;
                                    }
                                }

                                // Create a transition from start to end
                                if (!duplicate)
                                {
                                    var connection = MotionGraphEditorFactory.CreateConnection(editor.currentGraph, new SerializedObject(m_TransitionStart), end);
                                    Selection.activeObject = connection;
                                }
                            }
                            m_TransitionStart = null;
                        }
                        else
                        {
                            if (m_ViewportRect.Contains(e.mousePosition))
                            {
                                switch (e.button)
                                {
                                    case 0: // Select
                                        {
                                            EventModifiers mods = GetEventModifiers(e);
                                            switch (mods)
                                            {
                                                case EventModifiers.None:
                                                    {
                                                        // Simple click / double click
                                                        // Check for double click
                                                        float clickTime = Time.realtimeSinceStartup;
                                                        if (clickTime - m_LastClickTime < k_DoubleClickTimeout)
                                                        {
                                                            m_LastClickTime = 0f;
                                                            // Double clicking a subgraph is the same as entering / exiting
                                                            var over = GetSubGraphUnderCursor(e.mousePosition);
                                                            if (over != null)
                                                            {
                                                                if (editor.currentContext == over)
                                                                {
                                                                    if (!over.isRoot)
                                                                        editor.currentContext = over.parent;
                                                                }
                                                                else
                                                                    editor.currentContext = over;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Record click time
                                                            m_LastClickTime = clickTime;

                                                            // Get element under cursor
                                                            var element = GetElementUnderCursor(e.mousePosition);
                                                            if (element != null)
                                                            {
                                                                var obj = element as Object;
                                                                if (!Selection.Contains(obj))
                                                                {
                                                                    // Select the element
                                                                    Selection.activeObject = obj;
                                                                }
                                                                m_DraggingElements = true;
                                                            }
                                                            else
                                                            {
                                                                Selection.activeObject = null;
                                                                m_SelectionRectStart = e.mousePosition;
                                                                m_DragSelecting = true;
                                                            }
                                                        }
                                                    }
                                                    break;
                                                case EventModifiers.Control:
                                                    {
                                                        // Toggle selection
                                                        var over = GetConnectableUnderCursor(e.mousePosition);
                                                        if (over != null)
                                                        {
                                                            List<Object> s = new List<Object>(Selection.objects);
                                                            if (Selection.Contains(over))
                                                                s.Remove(over);
                                                            else
                                                                s.Add(over);
                                                            Selection.objects = s.ToArray();
                                                        }
                                                        else
                                                        {
                                                            Selection.activeObject = null;
                                                            m_SelectionRectStart = e.mousePosition;
                                                            m_DragSelecting = true;
                                                        }
                                                    }
                                                    break;
                                                case EventModifiers.Alt:
                                                    {
                                                        // Remove selection
                                                        var over = GetConnectableUnderCursor(e.mousePosition);
                                                        if (over != null)
                                                        {
                                                            if (Selection.Contains(over))
                                                            {
                                                                List<Object> newSelection = new List<Object>(Selection.objects);
                                                                newSelection.Remove(over);
                                                                Selection.objects = newSelection.ToArray();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            m_LastClickTime = 0f;
                                                            m_DraggingViewport = true;
                                                        }
                                                    }
                                                    break;
                                                case EventModifiers.Shift:
                                                    {
                                                        // Toggle selection
                                                        var over = GetConnectableUnderCursor(e.mousePosition);
                                                        if (over != null)
                                                        {
                                                            List<Object> s = new List<Object>(Selection.objects);
                                                            if (Selection.Contains(over))
                                                                s.Remove(over);
                                                            else
                                                                s.Add(over);
                                                            Selection.objects = s.ToArray();
                                                        }
                                                        else
                                                        {
                                                            Selection.activeObject = null;
                                                            m_SelectionRectStart = e.mousePosition;
                                                            m_DragSelecting = true;
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                    case 1: // Show context menu
                                        m_LastClickTime = 0f;
                                        if (editor.currentGraph != null)
                                            ShowContextMenu(e.mousePosition);
                                        break;
                                    case 2: // Move camera
                                        m_LastClickTime = 0f;
                                        m_DraggingViewport = true;
                                        break;
                                }
                            }
                        }
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        switch (e.button)
                        {
                            case 0: // Move selection
                                {
                                    if (m_DraggingViewport)
                                    {
                                        m_ViewportOffset += e.delta;
                                        m_ViewportOffset.x = Mathf.Clamp(m_ViewportOffset.x, -k_ViewportOffsetClamp, k_ViewportOffsetClamp);
                                        m_ViewportOffset.y = Mathf.Clamp(m_ViewportOffset.y, -k_ViewportOffsetClamp, k_ViewportOffsetClamp);
                                    }
                                    else
                                    {
                                        if (m_DragSelecting)
                                        {
                                            m_SelectionRect = new Rect(
                                                Mathf.Min(m_SelectionRectStart.x, e.mousePosition.x),
                                                Mathf.Min(m_SelectionRectStart.y, e.mousePosition.y),
                                                Mathf.Abs(m_SelectionRectStart.x - e.mousePosition.x),
                                                Mathf.Abs(m_SelectionRectStart.y - e.mousePosition.y)
                                            );
                                        }
                                        if (m_DraggingElements)
                                        {
                                            // How to improve this?
                                            foreach (Object obj in Selection.objects)
                                            {
                                                var connectable = obj as MotionGraphConnectable;
                                                if (connectable == null)
                                                    continue;

                                                var so = new SerializedObject(connectable);
                                                SerializedProperty prop;
                                                var mg = connectable as MotionGraph;
                                                if (mg == null || mg != editor.currentContext)
                                                    prop = so.FindProperty("uiPosition");
                                                else
                                                    prop = so.FindProperty("internalUiPosition");
                                                prop.vector2Value += e.delta;
                                                so.ApplyModifiedProperties();
                                            }
                                        }
                                    }
                                }
                                break;
                            case 2: // Move camera
                                {
                                    if (m_DraggingViewport)
                                    {
                                        m_ViewportOffset += e.delta;
                                        m_ViewportOffset.x = Mathf.Clamp(m_ViewportOffset.x, -k_ViewportOffsetClamp, k_ViewportOffsetClamp);
                                        m_ViewportOffset.y = Mathf.Clamp(m_ViewportOffset.y, -k_ViewportOffsetClamp, k_ViewportOffsetClamp);
                                    }
                                }
                                break;
                        }
                        break;
                    }
                case EventType.MouseUp:
                    {
                        m_CursorPosition = e.mousePosition;
                        switch (e.button)
                        {
                            case 0:
                                if (m_DragSelecting)
                                {
                                    // Select elements
                                    var contained = GetConnectablesOverlappingRect(m_SelectionRect);
                                    List<Object> s = new List<Object>(Selection.objects);;
                                    switch (GetEventModifiers(e))
                                    {
                                        case EventModifiers.Control:
                                            foreach (var connectable in contained)
                                            {
                                                if (!s.Contains(connectable))
                                                    s.Add(connectable);
                                            }
                                            break;
                                        case EventModifiers.Alt:
                                            foreach (var connectable in contained)
                                                s.Remove(connectable);
                                            break;
                                        default:
                                            s.Clear();
                                            foreach (var connectable in contained)
                                                s.Add(connectable);
                                            break;
                                    }
                                    Selection.objects = s.ToArray();

                                    // Reset the selection rect
                                    m_SelectionRectStart = Vector2.zero;
                                    m_SelectionRect = Rect.zero;
                                }
                                break;
                            case 2: // Move camera
                                break;
                        }

                        m_DragSelecting = false;
                        m_DraggingElements = false;
                        m_DraggingViewport = false;

                        break;
                    }
                case EventType.KeyDown:
                    {
                        switch (e.keyCode)
                        {
                            case KeyCode.Delete:
                                {
                                    editor.DeleteSelection();
                                    break;
                                }
                        }
                        break;
                    }
            }

            editor.Repaint();
        }

        IMotionGraphElement GetElementUnderCursor(Vector2 position)
        {
            var result = GetConnectableUnderCursor(position);
            if (result != null)
                return result;
            return GetConnectionUnderCursor(position);
        }

        MotionGraphConnectable GetConnectableUnderCursor(Vector2 cursor)
        {
            var result = GetSubGraphUnderCursor(cursor);
            if (result != null)
                return result;
            return GetStateUnderCursor (cursor);
        }

        MotionGraphState GetStateUnderCursor(Vector2 cursor)
        {
            Vector2 offset = m_ViewportOffset + m_ViewportCenterOffset;
            foreach (MotionGraphState state in editor.currentContext.states)
            {
                m_NodeRect.position = state.uiPosition + offset - k_NodeCenterOffset;
                if (m_NodeRect.Contains(cursor))
                    return state;
            }
            return null;
        }

        MotionGraph GetSubGraphUnderCursor(Vector2 cursor)
        {
            Vector2 offset = m_ViewportOffset + m_ViewportCenterOffset;

            m_NodeRect.position = editor.currentContext.internalUiPosition + offset - k_NodeCenterOffset;
            if (m_NodeRect.Contains(cursor))
                return editor.currentContext;

            foreach (MotionGraph graph in editor.currentContext.subGraphs)
            {
                m_NodeRect.position = graph.uiPosition + offset - k_NodeCenterOffset;
                if (m_NodeRect.Contains(cursor))
                    return graph;
            }
            return null;
        }

        MotionGraphConnection GetConnectionUnderCursor(Vector2 cursor)
        {
            Vector2 offset = m_ViewportOffset + m_ViewportCenterOffset;

            MotionGraph context = editor.currentContext;
            foreach (MotionGraphConnection connection in context.connections)
            {
                if (CheckConnection(connection, offset, cursor))
                    return connection;
            }

            foreach (MotionGraph subGraph in context.subGraphs)
                foreach (MotionGraphConnection connection in subGraph.connections)
                {
                    if (CheckConnection(connection, offset, cursor))
                        return connection;
                }

            foreach (MotionGraphState state in context.states)
                foreach (MotionGraphConnection connection in state.connections)
                {
                    if (CheckConnection(connection, offset, cursor))
                        return connection;
                }

            return null;
        }

        bool CheckConnection (MotionGraphConnection connection, Vector2 offset, Vector2 cursor)
        {
            if (connection == null)
                return false;
            if (connection.source == null || connection.destination == null)
                return false;
            if (connection.destination.parent != editor.currentContext)
                return false;
            
            // Get the midpoint
            Vector2 sourcePoint = connection.source.uiPosition;
            if (connection.source == editor.currentContext)
                sourcePoint = editor.currentContext.internalUiPosition;
            Vector2 midPoint = Vector2.Lerp(sourcePoint, connection.destination.uiPosition, 0.5f);
            midPoint += offset;

            // Get the side offset
            Vector2 sideOffset = connection.destination.uiPosition - sourcePoint;
            sideOffset.Normalize();
            sideOffset = k_SidewaysRotation * sideOffset;
            sideOffset *= 6f;

            // add side offset
            midPoint += sideOffset;

            return (Vector2.Distance(midPoint, cursor) <= k_ConnectionClickRange);
        }

        List<MotionGraphConnectable> GetConnectablesOverlappingRect (Rect r)
        {
            List<MotionGraphConnectable> results = new List<MotionGraphConnectable>();

            Vector2 offset = m_ViewportOffset + m_ViewportCenterOffset;

            m_NodeRect.position = editor.currentContext.internalUiPosition + offset - k_NodeCenterOffset;
            if (m_NodeRect.Overlaps(r))
                results.Add(editor.currentContext);

            foreach (MotionGraph graph in editor.currentContext.subGraphs)
            {
                m_NodeRect.position = graph.uiPosition + offset - k_NodeCenterOffset;
                if (m_NodeRect.Overlaps(r))
                    results.Add(graph);
            }

            foreach (MotionGraphState state in editor.currentContext.states)
            {
                m_NodeRect.position = state.uiPosition + offset - k_NodeCenterOffset;
                if (m_NodeRect.Overlaps(r))
                    results.Add(state);
            }

            return results;
        }

        EventModifiers GetEventModifiers(Event e)
        {
            return e.modifiers & k_EventModifiersMask;
        }

        #endregion
        
        #region CONTEXT MENUS

        Vector2 m_ContextMenuPosition;

        void ShowContextMenu(Vector2 position)
        {
            m_ContextMenuPosition = position;

            // Check if over selected element
            IMotionGraphElement element = GetElementUnderCursor(position);
            if (element != null)
            {
                if (Selection.Contains(element as Object))
                {
                    if (GetNumSelectedElements() == 1)
                    {
                        var mg = element as MotionGraph;
                        if (mg != null)
                        {
                            ShowSubGraphContextMenu(mg);
                            return;
                        }

                        var mgs = element as MotionGraphState;
                        if (mgs != null)
                        {
                            ShowStateContextMenu(mgs);
                            return;
                        }

                        var mgc = element as MotionGraphConnection;
                        if (mgc != null)
                        {
                            ShowConnectionContextMenu(mgc);
                            return;
                        }
                    }
                    ShowSelectionContextMenu();
                    return;
                }
            }

            // Check if over node
            var state = GetStateUnderCursor(position);
            if (state != null)
            {
                ShowStateContextMenu(state);
                return;
            }

            // Check if over sub-graph
            var graph = GetSubGraphUnderCursor(position);
            if (graph != null)
            {
                ShowSubGraphContextMenu(graph);
                return;
            }

            // Check if over connection
            var connection = GetConnectionUnderCursor(position);
            if (connection != null)
            {
                ShowConnectionContextMenu(connection);
                return;
            }

            ShowViewportContextMenu();
        }

        int GetNumSelectedElements()
        {
            int result = 0;
            var selection = Selection.objects;
            foreach (var o in selection)
            {
                if (o is IMotionGraphElement)
                    ++result;
            }
            return result;
        }

        int GetNumSelectedConnectables()
        {
            int result = 0;
            var selection = Selection.objects;
            foreach (var o in selection)
            {
                if (o is MotionGraphConnectable)
                    ++result;
            }
            return result;
        }

        void ShowSelectionContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();

            // Copy / paste
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Cut Selection"), false, OnMenuCutSelection);
            genericMenu.AddItem(new GUIContent("Copy Selection"), false, OnMenuCopySelection);

            // Move to subgraph
            genericMenu.AddItem(new GUIContent("Move Selection To Sub-Graph"), false, OnMenuMoveSelectionToSubGraph);

            // Delete
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Delete Selection"), false, OnMenuDeleteSelection);

            genericMenu.ShowAsContext();
        }

        void ShowStateContextMenu(MotionGraphState state)
        {
            GenericMenu genericMenu = new GenericMenu();

            // Set default
            if (editor.currentContext.defaultEntry != state)
                genericMenu.AddItem(new GUIContent("Set Default"), false, OnMenuSetDefault, (object)state);

            // Transition
            genericMenu.AddItem(new GUIContent("Make Transition"), false, OnMenuMakeTransition, (object)state);

            // Copy / paste
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Cut State"), false, OnMenuCutConnectable, (object)state);
            genericMenu.AddItem(new GUIContent("Copy State"), false, OnMenuCopyConnectable, (object)state);

            // Move to subgraph
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Move To Sub-Graph"), false, OnMenuMoveElementToSubGraph, (object)state);

            // Delete
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Delete"), false, OnMenuDeleteState, (object)state);

            genericMenu.ShowAsContext();
        }

        void ShowSubGraphContextMenu(MotionGraph subGraph)
        {
            GenericMenu genericMenu = new GenericMenu();

            if (subGraph.isRoot)
                genericMenu.AddItem(new GUIContent("Make Transition"), false, OnMenuMakeTransition, (object)subGraph);
            else
            {
                if (subGraph == editor.currentContext)
                {
                    genericMenu.AddItem(new GUIContent("Show Parent Graph"), false, OnMenuPopSubGraphContext);
                    genericMenu.AddItem(new GUIContent("Make Transition"), false, OnMenuMakeTransition, (object)subGraph);
                }
                else
                {
                    // Show subgraph
                    genericMenu.AddItem(new GUIContent("Show Sub-Graph"), false, OnMenuPushSubGraphContext, (object)(subGraph));

                    // Set default
                    if (editor.currentContext.defaultEntry != subGraph)
                        genericMenu.AddItem(new GUIContent("Set Default"), false, OnMenuSetDefault, (object)subGraph);

                    // Transition
                    genericMenu.AddItem(new GUIContent("Make Transition"), false, OnMenuMakeTransition, (object)subGraph);

                    // Copy / paste
                    genericMenu.AddSeparator("");
                    genericMenu.AddItem(new GUIContent("Cut Sub-Graph"), false, OnMenuCutConnectable, (object)subGraph);
                    genericMenu.AddItem(new GUIContent("Copy Sub-Graph"), false, OnMenuCopyConnectable, (object)subGraph);

                    // Move to subgraph
                    genericMenu.AddSeparator("");
                    genericMenu.AddItem(new GUIContent("Move To Sub-Graph"), false, OnMenuMoveElementToSubGraph, (object)subGraph);

                    // Delete
                    genericMenu.AddSeparator("");
                    genericMenu.AddItem(new GUIContent("Delete"), false, OnMenuDeleteSubGraph, (object)subGraph);
                }
            }

            genericMenu.ShowAsContext();
        }

        void ShowConnectionContextMenu(MotionGraphConnection connection)
        {
            GenericMenu genericMenu = new GenericMenu();

            // Delete
            genericMenu.AddItem(new GUIContent("Delete"), false, OnMenuDeleteConnection, (object)connection);

            genericMenu.ShowAsContext();
        }

        void ShowViewportContextMenu()
        {
            // Get state menu
            var menu = MotionGraphEditorFactory.GetStateMenu(
                editor.currentGraph,
                new SerializedObject(editor.currentContext),
                m_CursorPosition - m_ViewportOffset - m_ViewportCenterOffset
                );

            // Add subgraph
            menu.AddItem(new GUIContent("Add Sub-Graph"), false, OnMenuCreateSubGraph);

            // Add parent graph
            if (!editor.currentContext.isRoot)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Show Parent Graph"), false, OnMenuPopSubGraphContext);
            }

            bool allowCopy = false;
            var selection = Selection.objects;
            foreach (var obj in selection)
            {
                if (obj is MotionGraphConnectable)
                {
                    allowCopy = true;
                    break;
                }
            }
            bool allowPaste = !editor.clipboard.empty;
            if (allowCopy || allowPaste)
            {
                menu.AddSeparator("");
                if (allowCopy)
                {
                    menu.AddItem(new GUIContent("Cut Selection"), false, OnMenuCutSelection);
                    menu.AddItem(new GUIContent("Copy Selection"), false, OnMenuCopySelection);
                }
                if (allowPaste)
                {
                    menu.AddItem(new GUIContent("Paste Elements"), false, OnMenuPasteElements);
                }
            }

            menu.ShowAsContext();
        }

        void OnMenuCutSelection()
        {
            editor.clipboard.CutSelection(m_ContextMenuPosition);
        }

        void OnMenuCopySelection()
        {
            editor.clipboard.CopySelection(m_ContextMenuPosition);
        }

        void OnMenuPasteElements()
        {
            editor.clipboard.Paste(m_ContextMenuPosition);
        }

        void OnMenuCutConnectable(object obj)
        {
            MotionGraphConnectable connectable = obj as MotionGraphConnectable;
            if (connectable != null)
                editor.clipboard.CutConnectable(connectable, m_ContextMenuPosition);
        }

        void OnMenuCopyConnectable(object obj)
        {
            MotionGraphConnectable connectable = obj as MotionGraphConnectable;
            if (connectable != null)
                editor.clipboard.CopyConnectable(connectable, m_ContextMenuPosition);
        }

        void OnMenuPopSubGraphContext()
        {
            if (editor.currentContext != null)
                editor.currentContext = editor.currentContext.parent;
        }

        void OnMenuPushSubGraphContext(object subGraph)
        {
            editor.currentContext = (MotionGraph)subGraph;
        }

        void OnMenuSetDefault(object obj)
        {
            SerializedObject so = new SerializedObject(editor.currentContext);
            so.FindProperty("m_DefaultEntry").objectReferenceValue = (MotionGraphConnectable)obj;
            so.ApplyModifiedProperties();
        }

        void OnMenuMakeTransition(object obj)
        {
            m_TransitionStart = (MotionGraphConnectable)obj;
        }

        void OnMenuDeleteState(object obj)
        {
            var state = (MotionGraphState)obj;
            // Check for if node is selected
            if (Selection.Contains(state))
                editor.DeleteSelection();
            else
                editor.DeleteState(state);
        }

        void OnMenuDeleteSubGraph(object obj)
        {
            var graph = (MotionGraph)obj;
            // Check for if node is selected
            if (Selection.Contains(graph))
                editor.DeleteSelection();
            else
                editor.DeleteSubGraph(graph);
        }

        void OnMenuDeleteConnection(object obj)
        {
            var connection = (MotionGraphConnection)obj;
            editor.DeleteConnection(connection);
        }

        void OnMenuDeleteSelection()
        {
            editor.DeleteSelection();
        }

        void OnMenuCreateSubGraph()
        {
            var graph = MotionGraphEditorFactory.CreateSubgraph(editor.currentGraph, new SerializedObject(editor.currentContext), m_ContextMenuPosition - m_ViewportOffset - m_ViewportCenterOffset);
            Selection.activeObject = graph;
        }

        void OnMenuMoveSelectionToSubGraph()
        {
            List<MotionGraphConnectable> connectables = new List<MotionGraphConnectable>();
            foreach (var o in Selection.objects)
            {
                var connectable = o as MotionGraphConnectable;
                if (connectable != null && connectable.parent == editor.currentContext)
                    connectables.Add(connectable);
            }

            if (connectables.Count == 0)
                return;

            MoveElementsToSubgraph(connectables);
        }

        void OnMenuMoveElementToSubGraph(object obj)
        {
            // Get element
            var connectable = obj as MotionGraphConnectable;
            if (connectable == null || connectable.parent != editor.currentContext)
                return;

            List<MotionGraphConnectable> connectables = new List<MotionGraphConnectable>(1);
            connectables.Add(connectable);
            MoveElementsToSubgraph(connectables);
        }

        void MoveElementsToSubgraph (List<MotionGraphConnectable> connectables)
        {
            // Create subgraph
            var contextSO = new SerializedObject(editor.currentContext);
            var newSubGraph = MotionGraphEditorFactory.CreateSubgraph(editor.currentGraph, contextSO, m_CursorPosition - m_ViewportOffset - m_ViewportCenterOffset);
            var newGraphSO = new SerializedObject(newSubGraph);

            foreach (var connectable in connectables)
            {
                var connectableSO = new SerializedObject(connectable);

                // Check if default entry
                if (editor.currentContext.defaultEntry == connectable)
                {
                    contextSO.FindProperty("m_DefaultEntry").objectReferenceValue = newSubGraph;
                    newGraphSO.FindProperty("m_DefaultEntry").objectReferenceValue = connectable;
                }

                // Set parent
                connectableSO.FindProperty("m_Parent").objectReferenceValue = newSubGraph;
                connectableSO.ApplyModifiedProperties();

                // Remove from context and add to new subgraph
                var mgs = connectable as MotionGraphState;
                if (mgs != null)
                {
                    var prop = contextSO.FindProperty("m_States");
                    SerializedArrayUtility.Remove(prop, connectable);
                    prop = newGraphSO.FindProperty("m_States");
                    SerializedArrayUtility.Add(prop, connectable);
                }
                else
                {
                    var prop = contextSO.FindProperty("m_SubGraphs");
                    SerializedArrayUtility.Remove(prop, connectable);
                    prop = newGraphSO.FindProperty("m_SubGraphs");
                    SerializedArrayUtility.Add(prop, connectable);
                }
            }

            newGraphSO.ApplyModifiedProperties();
            contextSO.ApplyModifiedProperties();

            // Get all connections
            List<MotionGraphConnection> connections = new List<MotionGraphConnection>();
            editor.currentGraph.rootNode.CollectConnections(connections);

            // Remove invaidated connections
            foreach (var connection in connections)
            {
                if ((connection.source.parent != connection.destination.parent) && (connection.destination.parent != connection.source))
                    editor.DeleteConnection(connection);
            }

            Selection.activeObject = newSubGraph;
        }

        #endregion

        void OnContextChanged (MotionGraph context)
        {
            if (context != null)
            {
                // Build the context string
                m_CurrentContextStringBuilder.Length = 0;
                m_CurrentContextStringBuilder.Append("Root");
                if (context != null)
                {
                    List<string> names = new List<string>();
                    MotionGraph itr = context;
                    while (itr != null && itr != editor.currentGraph.rootNode)
                    {
                        names.Add(itr.name);
                        itr = itr.parent;
                    }
                    for (int i = names.Count; i > 0; --i)
                        m_CurrentContextStringBuilder.AppendFormat("->\"{0}\"", names[i - 1]);
                }
                m_CurrentContextString = m_CurrentContextStringBuilder.ToString();
            }
            else
            {
                m_CurrentContextString = "None";
            }
        }
    }
}