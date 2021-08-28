using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPSEditor.CharacterMotion.EditorClasses;

namespace NeoFPSEditor.CharacterMotion
{
    public class MotionGraphEditorClipboard : IMotionGraphMap
    {
        MotionGraphEditor m_Editor = null;

        private string m_BehaviourJson = string.Empty;
        private Type m_BehaviourType = null;

        private Vector2 m_CopyPostion = Vector2.zero;
        private Dictionary<int, ScriptableObject> m_Mappings = new Dictionary<int, ScriptableObject>(512);
        private List<IMotionGraphElement> m_Clones = new List<IMotionGraphElement>(512);
        private List<MotionGraphConnectable> m_RootConnectables = new List<MotionGraphConnectable>(64);

        public class ClipboardEntry
        {
            public Type type = null;
            public int uid = 0;
            public string json = string.Empty;

            public ClipboardEntry(Type t, int u, string j)
            {
                type = t;
                uid = u;
                json = j;
            }
        }

        public bool empty
        {
            get { return m_Clones.Count == 0; }
        }

        public MotionGraphEditorClipboard (MotionGraphEditor editor)
        {
            m_Editor = editor;
        }

        public void CopyBehaviourProperties (MotionGraphBehaviour source)
        {
            m_BehaviourType = source.GetType();
            m_BehaviourJson = JsonUtility.ToJson(source);
        }

        public bool CanPasteBehaviourProperties (MotionGraphBehaviour target)
        {
            if (target == null || m_BehaviourType == null || string.IsNullOrEmpty(m_BehaviourJson))
                return false;
            return target.GetType() == m_BehaviourType;
        }

        public void PasteBehaviourProperties (MotionGraphBehaviour target)
        {
            if (target == null)
                return;

            Undo.RecordObject(target, "Paste Behaviour Properties");
            JsonUtility.FromJsonOverwrite(m_BehaviourJson, target);
        }

        public void CutSelection (Vector2 mousePosition)
        {
            CopySelection(mousePosition);
            m_Editor.DeleteSelection();
        }

        public void CopySelection (Vector2 mousePosition)
        {
            // Get list of connectables from selection
            List<MotionGraphConnectable> connectables = new List<MotionGraphConnectable>();
            var selection = Selection.objects;
            for (int i = 0; i < selection.Length; ++i)
            {
                var connectable = selection[i] as MotionGraphConnectable;
                if (connectable != null && connectable.parent == m_Editor.currentContext)
                    connectables.Add(connectable);
            }

            // Copy connectables
            if (connectables.Count > 0)
                CopyInternal(connectables, mousePosition);
        }

        public void CutConnectable (MotionGraphConnectable connectable, Vector2 mousePosition)
        {
            // Copy the connectable
            CopyConnectable(connectable, mousePosition);
            // Delete it
            if (connectable is MotionGraphState)
                m_Editor.DeleteState(connectable as MotionGraphState);
            if (connectable is MotionGraph)
                m_Editor.DeleteSubGraph(connectable as MotionGraph);
        }

        public void CopyConnectable (MotionGraphConnectable connectable, Vector2 mousePosition)
        {
            List<MotionGraphConnectable> connectables = new List<MotionGraphConnectable>();
            connectables.Add(connectable);
            CopyInternal(connectables, mousePosition);
        }

        void CopyInternal (List<MotionGraphConnectable> connectables, Vector2 mousePosition)
        {
            m_CopyPostion = mousePosition;

            Clear();

            int lastIndex = connectables.Count;

            // Expand connectables to include children
            for (int i = 0; i < lastIndex; ++i)
            {
                MotionGraph graph = connectables[i] as MotionGraph;
                if (graph != null)
                    GetChildConnectables(graph, connectables);
            }

            // Clone connectables and their attached elements
            for (int i = 0; i < connectables.Count; ++i)
            {
                // Clone connectable
                MotionGraphConnectable connectable = Clone(connectables[i]);

                // Set null parent if part of original selection
                if (i < lastIndex)
                {
                    SerializedObject so = new SerializedObject(connectable);
                    so.FindProperty("m_Parent").objectReferenceValue = null;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    m_RootConnectables.Add(connectable);
                }

                // Clone behaviours
                for (int j = 0; j < connectable.behaviours.Count; ++j)
                    Clone(connectable.behaviours[j]);

                // Clone connections
                for (int j = connectable.connections.Count - 1; j >= 0; --j)
                {
                    // Only clone it if it (connects to another in the clipboard)
                    if (connectables.Contains(connectable.connections[j].destination))
                    {
                        MotionGraphConnection connection = Clone(connectable.connections[j]);
                        for (int k = 0; k < connection.conditions.Count; ++k)
                            Clone(connection.conditions[k]);
                    }
                    else
                        connectable.connections.RemoveAt(j);
                }
            }
        }

        public void GetChildConnectables (MotionGraph graph, List<MotionGraphConnectable> connectables)
        {
            for (int i = 0; i < graph.states.Count; ++i)
                connectables.Add(graph.states[i]);
            for (int i = 0; i < graph.subGraphs.Count; ++i)
            {
                connectables.Add(graph.subGraphs[i]);
                GetChildConnectables(graph.subGraphs[i], connectables);
            }
        }

        public void Paste (Vector2 mousePosition)
        {
            // Get new context
            var context = m_Editor.currentContext;
            var contextSO = new SerializedObject(context);

            // New selection list
            List<MotionGraphConnectable> selection = new List<MotionGraphConnectable>();

            // Get position offset
            Vector2 offset = mousePosition - m_CopyPostion;

            // Add pasted elements as assets to graph
            MotionGraphContainer root = m_Editor.currentGraph;
            for (int i = 0; i < m_Clones.Count; ++i)
                AssetDatabase.AddObjectToAsset(m_Clones[i] as ScriptableObject, root);
            
            // Check references
            for (int i = 0; i < m_Clones.Count; ++i)
                m_Clones[i].CheckReferences(this);

            // Connect root connectables into new context
            for (int i = 0; i < m_RootConnectables.Count; ++i)
            {
                // Set parent
                var connectable = m_RootConnectables[i];

                // Set position
                connectable.uiPosition += offset;

                // Attach to context
                SerializedObject so = new SerializedObject(connectable);
                if (connectable is MotionGraphState)
                {
                    var mgs = connectable as MotionGraphState;
                    SerializedArrayUtility.Add(contextSO.FindProperty("m_States"), mgs);
                }
                if (connectable is MotionGraph)
                {
                    var mg = connectable as MotionGraph;
                    SerializedArrayUtility.Add(contextSO.FindProperty("m_SubGraphs"), mg);
                    so.FindProperty("m_Container").objectReferenceValue = m_Editor.currentGraph;
                }
                so.FindProperty("m_Parent").objectReferenceValue = context;
                so.ApplyModifiedPropertiesWithoutUndo();

                // Add to new selection
                selection.Add(connectable);
            }
            contextSO.ApplyModifiedProperties();

            // Select new elements (root)
            Selection.objects = selection.ToArray();

            // Clear the clipboard & copy new elements (allows repeated pasting)
            Clear(false);
            CopySelection(mousePosition);
        }
        
        public void Clear (bool destroy = true)
        {
            // Clear mappings
            m_Mappings.Clear();
            // Clear root connectables
            m_RootConnectables.Clear();
            // Clear clones
            if (destroy)
            {
                for (int i = 0; i < m_Clones.Count; ++i)
                    ScriptableObject.DestroyImmediate(m_Clones[i] as ScriptableObject);
            }
            m_Clones.Clear();
        }

        public T Swap<T> (T original) where T : ScriptableObject
        {
            ScriptableObject result = null;
            if (original != null && m_Mappings.TryGetValue(original.GetInstanceID(), out result))
            {
                T cast = result as T;
                if (cast != null)
                    return cast;
            }            
            return original;
        }

        T Clone<T> (T original) where T : ScriptableObject, IMotionGraphElement
        {
            T result = ScriptableObject.Instantiate(original);
            result.name = original.name;
            m_Clones.Add(result);
            m_Mappings.Add(original.GetInstanceID(), result);
            return result;
        }
    }
}