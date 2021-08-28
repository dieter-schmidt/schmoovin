using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using NeoFPS.CharacterMotion;

namespace NeoFPSEditor.CharacterMotion
{
    public class MotionGraphEditor : EditorWindow
    {
        public static MotionGraphEditor instance { get; private set; }

        private float m_ParametersWidth = 300f;

        enum SelectionType
        {
            None,
            State,
            SubGraph,
            Connection
        }

        [MenuItem ("Tools/NeoFPS/Motion Graph Editor", priority = 10)]
        public static void CreateWindow ()
        {
            instance = GetWindow<MotionGraphEditor> ();
            instance.titleContent = new GUIContent ("Motion Graph Editor");
            instance.minSize = new Vector2 (800, 500);
        }

        private MotionGraphContainer m_CurrentGraph = null;
        public MotionGraphContainer currentGraph
        {
            get
            {
                if (m_CurrentController != null)
                    return m_CurrentController.motionGraph;
                else
                    return m_CurrentGraph;
            }
            set
            {
                if (value == null || m_CurrentGraph != value)
                {
                    m_CurrentGraph = value;
                    if (m_CurrentGraph != null)
                        currentController = null;
                    ResetEditor();
                }
            }
        }

        private IMotionController m_CurrentController = null;
        public IMotionController currentController
        {
            get { return m_CurrentController; }
            set 
            {
                if (value == null || m_CurrentController != value)
                {
                    if (m_CurrentController != null)
                        m_CurrentController.onCurrentStateChanged -= OnControllerStateChange;
            
                    m_CurrentController = value;

                    if (m_CurrentController != null)
                    {
                        currentGraph = null;
                        m_CurrentController.onCurrentStateChanged += OnControllerStateChange;
                        ResetEditor();
                    }
                }
            }
        }


        private List<int> m_ContextList = new List<int>();
        private MotionGraph m_CurrentContext = null;
        public MotionGraph currentContext
        {
            get
            {
                return m_CurrentContext;
            }
            set
            {
                if (value == null)
                {
                    if (currentGraph != null)
                        value = currentGraph.rootNode;
                    else
                        value = null;
                }
                m_CurrentContext = value;
                if (onContextChanged != null)
                    onContextChanged(m_CurrentContext);

                //Debug.Log("Setting context to: " + value, value);

                // Record context heirarchy for resets
                m_ContextList.Clear();
                var itr = m_CurrentContext;
                while (itr != null)
                {
                    //Debug.Log("Adding id: " + itr.GetInstanceID());
                    m_ContextList.Add(itr.GetInstanceID());
                    itr = itr.parent;
                }
            }
        }

        void CheckContext ()
        {
            if (currentGraph == null)
                return;

            // Check if current context is still valid
            // Can be invalidated by undoing creation of a graph whilst it is set as context
            MotionGraph best = currentGraph.rootNode;
            for (int i = m_ContextList.Count - 2; i >= 0; --i)
            {
                // Work up the context list, checking if subgraph with id in list exists
                MotionGraph found = null;
                foreach (var g in best.subGraphs)
                {
                    //Debug.Log("Checking: " + g.GetInstanceID() + " vs " + m_ContextList[i]);
                    if (g.GetInstanceID() == m_ContextList[i])
                    {
                        found = g;
                        break;
                    }
                }

                if (found == null)
                    break;

                best = found;
            }

            // Set the new context
            if (currentContext != best)
                currentContext = best;
        }

        public bool editingRuntimeGraph
        {
            get { return m_CurrentController != null && Application.isPlaying; }
        }

        public SerializedObject serializedObject
        {
            get;
            private set;
        }

        public event Action<MotionGraph> onContextChanged;

        public MotionGraphEditorInspectorView inspector { get; private set; }
        public MotionGraphEditorViewport viewport { get; private set; }
        public MotionGraphEditorClipboard clipboard { get; private set; }

        void OnDestroy ()
        {
            currentController = null;
            currentGraph = null;
            currentContext = null;

            clipboard.Clear();
            clipboard = null;
            inspector = null;
            viewport = null;
            instance = null;
        }

        void OnEnable ()
        {
            if (viewport == null)
                viewport = new MotionGraphEditorViewport (this);
            if (inspector == null)
                inspector = new MotionGraphEditorInspectorView (this);
            if (clipboard == null)
                clipboard = new MotionGraphEditorClipboard(this);
            
            // Register editor selection callback (to switch graphs)
            Selection.selectionChanged += OnUnityObjectSelectionChanged;
            OnUnityObjectSelectionChanged ();

            // Register undo/redo callback
            Undo.undoRedoPerformed += OnUndoRedo;

            // Register play mode callback
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        void OnDisable ()
        {
            Selection.selectionChanged -= OnUnityObjectSelectionChanged;
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        }

        void OnUndoRedo()
        {
            ResetEditorElements();
        }

        void OnUnityObjectSelectionChanged ()
        {
            var mg = Selection.activeObject as MotionGraphContainer;
            if (mg != null)
            {
                currentGraph = mg;
                return;
            }

            if (Selection.activeGameObject != null)
            {
                IMotionController mc = Selection.activeGameObject.GetComponentInChildren<IMotionController>(true);
                if (mc != null)
                {
                    currentController = mc;
                }
            }
        }

        void PlayModeStateChanged(PlayModeStateChange playState)
        {
            // Check if editor is editing runtime instance of a graph
            if (m_CurrentController != null)
                ResetEditor();
        }

        void Initialise ()
        {
            inspector = new MotionGraphEditorInspectorView(this);
            viewport = new MotionGraphEditorViewport(this);
        }

        public void ResetEditor ()
        {
            CheckGraphAsset ();
            viewport.Reset ();
            inspector.Reset ();
            clipboard.Clear();
            currentContext = null;
            Repaint ();
        }

        void ResetEditorElements ()
        {
            if (currentGraph == null)
            {
                serializedObject = null;
                return;
            }
            else
                serializedObject = new SerializedObject(currentGraph);
            
            viewport.Reset();
            inspector.Reset();

            Repaint();
        }

        void OnGUI ()
        {
            CheckContext();

            Rect viewportRect = new Rect(m_ParametersWidth, 0f, position.width - m_ParametersWidth, position.height);
            Rect inspectorRect = new Rect(0f, 0f, m_ParametersWidth, position.height);
            viewport.Draw(viewportRect);
            inspector.Draw(inspectorRect);

            if (currentGraph != null)
            {
                Event e = Event.current;
                if (e.type != EventType.Ignore && e.type != EventType.Used)
                {
                    inspector.ProcessEvent(e);
                    viewport.ProcessEvent(e);
                }
            }
        }

        void CheckGraphAsset()
        {
            if (currentGraph == null)
            {
                serializedObject = null;
                return;
            }
            else
                serializedObject = new SerializedObject(currentGraph);

            // Don't bother if this is a runtime graph
            if (!AssetDatabase.IsMainAsset(currentGraph))
                return;

            // Collect graph elements (also strips out null references)
            List<MotionGraph> collectedGraphs = new List<MotionGraph>();
            List<MotionGraphState> collectedStates = new List<MotionGraphState>();
            List<MotionGraphConnection> collectedConnections = new List<MotionGraphConnection>();
            List<MotionGraphCondition> collectedConditions = new List<MotionGraphCondition>();
            List<MotionGraphBehaviour> collectedBehaviours = new List<MotionGraphBehaviour>();
            currentGraph.rootNode.CollectGraphs(collectedGraphs);
            currentGraph.rootNode.CollectStates(collectedStates);
            currentGraph.rootNode.CollectConnections(collectedConnections);
            currentGraph.rootNode.CollectConditions(collectedConditions);
            currentGraph.rootNode.CollectBehaviours(collectedBehaviours);

            // Gather references
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(currentGraph));
            List<UnityEngine.Object> referenced = new List<UnityEngine.Object>(assets.Length);
            referenced.Add(currentGraph);
            foreach (MotionGraph g in collectedGraphs)
                referenced.Add(g);
            foreach (MotionGraphState s in collectedStates)
                referenced.Add(s);
            foreach (MotionGraphConnection c in collectedConnections)
                referenced.Add(c);
            foreach (MotionGraphCondition c in collectedConditions)
                referenced.Add(c);
            foreach (MotionGraphBehaviour b in collectedBehaviours)
                referenced.Add(b);
            var prop = serializedObject.FindProperty("m_Parameters");
            for (int i = prop.arraySize; i > 0; --i)
            {
                var obj = prop.GetArrayElementAtIndex(i - 1).objectReferenceValue;
                if (obj == null)
                    SerializedArrayUtility.RemoveAt(prop, i - 1);
                else
                    referenced.Add(obj);
            }
            prop = serializedObject.FindProperty("m_Data");
            for (int i = prop.arraySize; i > 0; --i)
            {
                var obj = prop.GetArrayElementAtIndex(i - 1).objectReferenceValue;
                if (obj == null)
                    SerializedArrayUtility.RemoveAt(prop, i - 1);
                else
                    referenced.Add(obj);
            }

            // Check for unreferenced assets
            for (int i = 0; i < assets.Length; ++i)
            {
                if (referenced.Contains(assets[i]) != true)
                {
                    DestroyImmediate(assets[i], true);
                    assets[i] = null;
                }
            }

            // Check for invalid connectables
            for (int i = collectedGraphs.Count; i > 0; --i)
            {
                MotionGraph g = collectedGraphs[i - 1];
                if (g != currentGraph.rootNode && g.parent == null)
                {
                    DestroyImmediate(g, true);
                    collectedGraphs.RemoveAt(i - 1);
                }
            }
            for (int i = collectedStates.Count; i > 0; --i)
            {
                MotionGraphState s = collectedStates[i - 1];
                if (s.parent == null)
                {
                    DestroyImmediate(s, true);
                    collectedStates.RemoveAt(i - 1);
                }
            }

            // Check for invalid connections
            for (int i = collectedConnections.Count; i > 0; --i)
            {
                MotionGraphConnection c = collectedConnections[i - 1];
                if (c.source == null)
                {
                    for (int j = 0; j < c.conditions.Count; ++j)
                    {
                        collectedConditions.Remove(c.conditions[i]);
                        DestroyImmediate(c.conditions[i]);
                    }
                    DestroyImmediate(c, true);
                    collectedConnections.RemoveAt(i - 1);
                }
                else
                {
                    if (c.destination == null)
                    {
                        for (int j = 0; j < c.conditions.Count; ++j)
                        {
                            collectedConditions.Remove(c.conditions[i]);
                            DestroyImmediate(c.conditions[i]);
                        }
                        c.source.connections.Remove(c);
                        DestroyImmediate(c);
                        collectedConnections.RemoveAt(i - 1);
                    }
                }
            }

            foreach (MotionGraph g in collectedGraphs)
            {
                g.hideFlags = HideFlags.HideInHierarchy;
                g.OnValidate();
            }
            foreach (MotionGraphState s in collectedStates)
            {
                s.hideFlags = HideFlags.HideInHierarchy;
                s.OnValidate();
            }
            foreach (MotionGraphConnection c in collectedConnections)
            {
                c.hideFlags = HideFlags.HideInHierarchy;
                c.OnValidate();
            }
            foreach (MotionGraphCondition c in collectedConditions)
            {
                c.hideFlags = HideFlags.HideInHierarchy;
                c.OnValidate();
            }
            foreach (MotionGraphBehaviour b in collectedBehaviours)
            {
                b.hideFlags = HideFlags.HideInHierarchy;
                b.OnValidate();
            }

            currentContext = null;
        }
        
        #region DELETION

        private bool m_RepaintOnDelete = true;
        
        public void DeleteState (MotionGraphState state)
        {
            if (state == null)
                return;
            
            // Delete connections
            for (int i = state.connections.Count; i > 0; --i)
                DeleteConnection (state.connections[i - 1]);
            
            // Get parent
            var parent = state.parent;
            var parentSO = new SerializedObject(parent);

            // Check default entry
            var prop = parentSO.FindProperty("m_DefaultEntry");
            if (prop.objectReferenceValue == state)
                prop.objectReferenceValue = null;

            // Remove state
            prop = parentSO.FindProperty("m_States");
            SerializedArrayUtility.Remove(prop, state);

            // Scan connectables for connections to state
            for (int i = prop.arraySize; i > 0; --i)
            {
                var connectable = SerializedArrayUtility.GetItemAtIndex<MotionGraphConnectable>(prop, i - 1);
                for (int j = connectable.connections.Count; j > 0; --j)
                {
                    if (connectable.connections[j - 1].destination == state)
                        DeleteConnection(connectable.connections[j - 1]);
                }
            }
            prop = parentSO.FindProperty("m_SubGraphs");
            for (int i = prop.arraySize; i > 0; --i)
            {
                var connectable = SerializedArrayUtility.GetItemAtIndex<MotionGraphConnectable>(prop, i - 1);
                for (int j = connectable.connections.Count; j > 0; --j)
                {
                    if (connectable.connections[j - 1].destination == state)
                        DeleteConnection(connectable.connections[j - 1]);
                }
            }

            // Apply changes
            parentSO.ApplyModifiedProperties();

            // Destroy the object
            Undo.DestroyObjectImmediate (state);

            if (m_RepaintOnDelete)
                Repaint();
        }

        public void DeleteSubGraph (MotionGraph graph)
        {
            if (graph == null || graph.parent == null)
                return;

            // Delete connections
            for (int i = graph.connections.Count; i > 0; --i)
                DeleteConnection(graph.connections[i - 1]);

            // Delete children
            for (int i = graph.subGraphs.Count; i > 0; --i)
                DeleteSubGraph(graph.subGraphs[i - 1]);
            for (int i = graph.states.Count; i > 0; --i)
                DeleteState(graph.states[i - 1]);

            // Get parent
            var parent = graph.parent;
            var parentSO = new SerializedObject(parent);

            // Check default entry
            var prop = parentSO.FindProperty("m_DefaultEntry");
            if (prop.objectReferenceValue == graph)
                prop.objectReferenceValue = null;

            // Remove state
            prop = parentSO.FindProperty("m_SubGraphs");
            SerializedArrayUtility.Remove(prop, graph);

            // Scan connectables for connections to subgraph
            if (prop == null)
                Debug.LogError("WTF");
            for (int i = prop.arraySize; i > 0; --i)
            {
                var connectable = SerializedArrayUtility.GetItemAtIndex<MotionGraphConnectable>(prop, i - 1);
                for (int j = connectable.connections.Count; j > 0; --j)
                {
                    if (connectable.connections[j - 1].destination == graph)
                        DeleteConnection(connectable.connections[j - 1]);
                }
            }
            prop = parentSO.FindProperty("m_States");
            for (int i = prop.arraySize; i > 0; --i)
            {
                var connectable = SerializedArrayUtility.GetItemAtIndex<MotionGraphConnectable>(prop, i - 1);
                for (int j = connectable.connections.Count; j > 0; --j)
                {
                    if (connectable.connections[j - 1].destination == graph)
                        DeleteConnection(connectable.connections[j - 1]);
                }
            }

            // Apply changes
            parentSO.ApplyModifiedProperties();

            // Destroy the object
            Undo.DestroyObjectImmediate(graph);

            if (m_RepaintOnDelete)
                Repaint();
        }

        public void DeleteConnection (MotionGraphConnection connection)
        {
            if (connection == null)
                return;

            // Disconnect
            var sourceSO = new SerializedObject(connection.source);
            var prop = sourceSO.FindProperty("m_Connections");
            SerializedArrayUtility.Remove(prop, connection);
            sourceSO.ApplyModifiedProperties();
            
            // Destroy conditions
            for (int i = connection.conditions.Count; i > 0; --i)
                Undo.DestroyObjectImmediate(connection.conditions[i-1]);

            // Destroy Connection
            Undo.DestroyObjectImmediate (connection);
            if (m_RepaintOnDelete)
                Repaint();
        }
        
        public void DeleteCondition (MotionGraphConnection connection, int index)
        {
            MotionGraphCondition condition = connection.conditions[index];
            if (condition != null)
            {
                var connectionSO = new SerializedObject(connection);
                var prop = connectionSO.FindProperty("m_Conditions");
                SerializedArrayUtility.RemoveAt(prop, index);
                connection.conditions.RemoveAt(index);
                connectionSO.ApplyModifiedProperties();
                Undo.DestroyObjectImmediate(condition);
            }
        }

        public void DeleteParameter (int index)
        {
            var prop = serializedObject.FindProperty("m_Parameters");
            var parameter = prop.GetArrayElementAtIndex(index).objectReferenceValue;

            SerializedArrayUtility.RemoveAt(prop, index);
            serializedObject.ApplyModifiedProperties();

            Undo.DestroyObjectImmediate(parameter);
        }

        public void DeleteParameter (MotionGraphParameter parameter)
        {
            var prop = serializedObject.FindProperty("m_Parameters");

            SerializedArrayUtility.Remove(prop, parameter);
            serializedObject.ApplyModifiedProperties();

            Undo.DestroyObjectImmediate(parameter);
        }

        public void DeleteMotionData(int index)
        {
            var prop = serializedObject.FindProperty("m_Data");
            var data = prop.GetArrayElementAtIndex(index).objectReferenceValue;

            SerializedArrayUtility.RemoveAt(prop, index);
            serializedObject.ApplyModifiedProperties();

            Undo.DestroyObjectImmediate(data);
        }

        public void DeleteMotionData(MotionGraphDataBase data)
        {
            var prop = serializedObject.FindProperty("m_Data");

            SerializedArrayUtility.Remove(prop, data);
            serializedObject.ApplyModifiedProperties();

            Undo.DestroyObjectImmediate(data);
        }

        public void DeleteBehaviour (MotionGraphConnectable connectable, MotionGraphBehaviour behaviour)
        {
            if (connectable == null)
                return;

            var connectableSO = new SerializedObject(connectable);
            var prop = connectableSO.FindProperty("m_Behaviours");
            SerializedArrayUtility.Remove(prop, behaviour);
            connectableSO.ApplyModifiedProperties();

            Undo.DestroyObjectImmediate(behaviour);
        }

        public void DeleteSelection()
        {
            m_RepaintOnDelete = false;

            var objects = Selection.objects;
            for (int i = objects.Length - 1; i >= 0; --i)
            {
                // Delete connections first (null ref otherwise)
                var connection = objects[i] as MotionGraphConnection;
                if (connection != null)
                {
                    DeleteConnection(connection);
                    objects[i] = null;
                }
            }

            for (int i = objects.Length - 1; i >= 0; --i)
            {
                // Delete state
                var state = objects[i] as MotionGraphState;
                if (state != null)
                {
                    DeleteState(state);
                    continue;
                }

                // Delete sub-graph
                var graph = objects[i] as MotionGraph;
                if (graph != null && !graph != currentContext)
                {
                    DeleteSubGraph(graph);
                }
            }

            Selection.activeObject = null;

            m_RepaintOnDelete = true;
            Repaint();
        }

        #endregion
        
        #region REALTIME VIEW

        void OnControllerStateChange ()
        {
            var s = currentController.currentState;
            if (currentContext != s.parent)
                currentContext = s.parent;
        }

        #endregion
    }
}