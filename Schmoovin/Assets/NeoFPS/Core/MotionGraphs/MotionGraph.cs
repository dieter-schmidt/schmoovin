using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion
{
    [HelpURL("https://docs.neofps.com/manual/motiongraph-index.html")]
    public class MotionGraph : MotionGraphConnectable
    {
#if UNITY_EDITOR
        public Vector2 internalUiPosition;
#endif

        [SerializeField] private MotionGraphContainer m_Container = null;
        [SerializeField] private List<MotionGraphState> m_States = new List<MotionGraphState> ();
        [SerializeField] private List<MotionGraph> m_SubGraphs = new List<MotionGraph> ();
        [SerializeField] private MotionGraphConnectable m_DefaultEntry = null;

        public MotionGraphContainer container
        {
            get { return m_Container; }
        }
        public List<MotionGraphState> states
        {
            get { return m_States; }
        }
        public List<MotionGraph> subGraphs
        {
            get { return m_SubGraphs; }
        }
        public MotionGraphConnectable defaultEntry
        {
            get { return m_DefaultEntry; }
        }

        public bool isRoot
        {
            get { return parent == null; }
        }

#if UNITY_EDITOR
        public override void OnValidate ()
        {
            base.OnValidate();
            for (int i = m_SubGraphs.Count; i > 0; --i)
            {
                if (subGraphs[i - 1] == null)
                    subGraphs.RemoveAt(i - 1);
            }
            for (int i = m_States.Count; i > 0; --i)
            {
                if (m_States[i - 1] == null)
                    m_States.RemoveAt(i - 1);
            }
        }
#endif

        public override void Initialise(IMotionController c)
        {
            base.Initialise(c);
            for (int i = 0; i < m_States.Count; ++i)
                m_States[i].Initialise(c);
            for (int i = 0; i < m_SubGraphs.Count; ++i)
                m_SubGraphs[i].Initialise(c);
        }

        public MotionGraph GetGraphRoot ()
        {
            if (parent == null)
                return this;
            else
                return parent.GetGraphRoot ();
        }

        public MotionGraphState GetStateFromKey(int key)
        {
            // Check this graph node's states
            for (int i = 0; i < m_States.Count; ++i)
            {
                if (m_States[i].serializationKey == key)
                    return m_States[i];
            }

            // Check subgraphs
            for (int i = 0; i < m_SubGraphs.Count; ++i)
            {
                var result = m_SubGraphs[i].GetStateFromKey(key);
                if (result != null)
                    return result;
            }

            // Not found
            return null;
        }

        public void CollectGraphs (List<MotionGraph> list)
        {
            list.Add(this);
            for (int i = 0; i < subGraphs.Count; ++i)
                subGraphs[i].CollectGraphs(list);
        }

        public void CollectStates (List<MotionGraphState> list)
        {
            list.AddRange(states);
            for (int i = 0; i < subGraphs.Count; ++i)
                subGraphs[i].CollectStates(list);
        }
        public void CollectConnections (List<MotionGraphConnection> list)
        {
            list.AddRange(connections);
            for (int i = 0; i < states.Count; ++i)
                list.AddRange(states[i].connections);
            for (int i = 0; i < subGraphs.Count; ++i)
                subGraphs[i].CollectConnections(list);
        }
        public void CollectConditions (List<MotionGraphCondition> list)
        {
            for (int i = 0; i < connections.Count; ++i)
            {
                list.AddRange(connections[i].conditions);
                for (int j = 0; j < connections[i].conditionGroups.Count; ++j)
                    list.AddRange(connections[i].conditionGroups[j].conditions);
            }
            for (int i = 0; i < states.Count; ++i)
            {
                for (int j = 0; j < states[i].connections.Count; ++j)
                {
                    list.AddRange(states[i].connections[j].conditions);
                    for (int k = 0; k < states[i].connections[j].conditionGroups.Count; ++k)
                        list.AddRange(states[i].connections[j].conditionGroups[k].conditions);
                }
            }
            for (int i = 0; i < subGraphs.Count; ++i)
                subGraphs[i].CollectConditions(list);
        }
        public void CollectBehaviours (List<MotionGraphBehaviour> list)
        {
            list.AddRange(behaviours);
            for (int i = 0; i < states.Count; ++i)
                list.AddRange(states[i].behaviours);
            for (int i = 0; i < subGraphs.Count; ++i)
                subGraphs[i].CollectBehaviours(list);
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            base.CheckReferences(map);
            for (int i = 0; i < m_SubGraphs.Count; ++i)
                m_SubGraphs[i] = map.Swap(m_SubGraphs[i]);
            for (int i = 0; i < m_States.Count; ++i)
                m_States[i] = map.Swap(m_States[i]);
            m_DefaultEntry = map.Swap(m_DefaultEntry);
            m_Container = map.Swap(m_Container);
        }
    }
}