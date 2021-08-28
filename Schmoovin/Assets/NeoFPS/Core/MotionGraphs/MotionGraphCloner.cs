using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    public class MotionGraphCloner : IMotionGraphMap
    {
        private List<MotionGraph> m_CollectedGraphs = new List<MotionGraph>(64);
        private List<MotionGraphState> m_CollectedStates = new List<MotionGraphState>(256);
        private List<MotionGraphConnection> m_CollectedConnections = new List<MotionGraphConnection>(256);
        private List<MotionGraphCondition> m_CollectedConditions = new List<MotionGraphCondition>(512);
        private List<MotionGraphBehaviour> m_CollectedBehaviours = new List<MotionGraphBehaviour>(64);
        private List<MotionGraphParameter> m_CollectedParameters = new List<MotionGraphParameter>(32);
        private List<MotionGraphDataBase> m_CollectedData = new List<MotionGraphDataBase>(32);
        private Dictionary<ScriptableObject, ScriptableObject> m_Mappings = new Dictionary<ScriptableObject, ScriptableObject>(512);
        private List<IMotionGraphElement> m_Clones = new List<IMotionGraphElement>(512);

        public MotionGraphContainer CloneGraph(MotionGraphContainer graph)
        {
            var newContainer = Clone(graph, false);

            graph.rootNode.CollectGraphs(m_CollectedGraphs);
            graph.rootNode.CollectStates(m_CollectedStates);
            graph.rootNode.CollectConnections(m_CollectedConnections);
            graph.rootNode.CollectConditions(m_CollectedConditions);
            graph.rootNode.CollectBehaviours(m_CollectedBehaviours);
            graph.CollectParameters(m_CollectedParameters);
            graph.CollectData(m_CollectedData);

            for (int i = 0; i < m_CollectedGraphs.Count; ++i)
                Clone(m_CollectedGraphs[i]);
            for (int i = 0; i < m_CollectedStates.Count; ++i)
                Clone(m_CollectedStates[i]);
            for (int i = 0; i < m_CollectedConnections.Count; ++i)
                Clone(m_CollectedConnections[i]);
            for (int i = 0; i < m_CollectedConditions.Count; ++i)
                Clone(m_CollectedConditions[i]);
            for (int i = 0; i < m_CollectedBehaviours.Count; ++i)
                Clone(m_CollectedBehaviours[i]);
            for (int i = 0; i < m_CollectedParameters.Count; ++i)
                Clone(m_CollectedParameters[i]);
            for (int i = 0; i < m_CollectedData.Count; ++i)
                Clone(m_CollectedData[i]);

            newContainer.CheckReferences(this);
            for (int i = 0; i < m_Clones.Count; ++i)
                m_Clones[i].CheckReferences(this);
            
            return newContainer;
        }

        public void Clear ()
        {
            m_CollectedGraphs.Clear();
            m_CollectedStates.Clear();
            m_CollectedConnections.Clear();
            m_CollectedConditions.Clear();
            m_CollectedParameters.Clear();
            m_CollectedBehaviours.Clear();
            m_CollectedData.Clear();

            m_Clones.Clear();
            m_Mappings.Clear();
        }

        public T Swap<T> (T original) where T : ScriptableObject
        {
            if (original == null)
                return null;
            ScriptableObject result = null;
            if (m_Mappings.TryGetValue(original, out result))
                return result as T;
            else
                return original;
        }

        T Clone<T> (T original, bool rename = true) where T : ScriptableObject, IMotionGraphElement
        {
            T result = ScriptableObject.Instantiate(original);
            if (rename)
                result.name = original.name;
            m_Clones.Add(result);
            m_Mappings.Add(original, result);
            return result;
        }
    }
}