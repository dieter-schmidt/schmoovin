using NeoFPS.CharacterMotion.Conditions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    [Serializable]
    public struct MotionGraphConditionGroup
    {
        [SerializeField] private string m_Name;
        [SerializeField] private MotionGraphConnection.ConditionRequirements m_TransitionOn;
        [SerializeField] private List<MotionGraphCondition> m_Conditions;

        [SerializeField, HideInInspector] private int m_ID;
        [SerializeField, HideInInspector] private MotionGraphConnection m_Connection;

#if UNITY_EDITOR
        [HideInInspector] public bool collapsed;
#endif

        private GroupState m_GroupState;

        public enum GroupState : byte
        {
            Unchecked,
            Checking,
            CheckedTrue,
            CheckedFalse
        }

        public int id
        {
            get { return m_ID; }
        }

        public string name
        {
            get { return m_Name; }
        }

        public MotionGraphConnection connection
        {
            get { return m_Connection; }
        }

        public List<MotionGraphCondition> conditions
        {
            get { return m_Conditions; }
        }

        public MotionGraphConditionGroup(string n, MotionGraphConnection c)
        {
            m_Name = n;
            m_TransitionOn = MotionGraphConnection.ConditionRequirements.AllTrue;
            m_Conditions = new List<MotionGraphCondition>();
            m_ID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            m_Connection = c;
            m_GroupState = GroupState.Unchecked;
#if UNITY_EDITOR
            collapsed = false;
#endif
        }

        public void Initialise (IMotionController c)
        {
            for (int i = 0; i < m_Conditions.Count; ++i)
                m_Conditions[i].Initialise(c);
        }

        public void ResetGroup()
        {
            m_GroupState = GroupState.Unchecked;
        }

        public bool CheckConditions()
        {
            switch (m_GroupState)
            {
                case GroupState.Unchecked:
                    // Set to checking state (will return false if evaluated again)
                    m_GroupState = GroupState.Checking;

                    // Check conditions
                    bool result = CheckConditionsInternal();

                    // Store result
                    if (result)
                        m_GroupState = GroupState.CheckedTrue;
                    else
                        m_GroupState = GroupState.CheckedFalse;

                    // Return
                    return result;
                case GroupState.CheckedTrue:
                    // Return stored result
                    return true; 
                default:
                    // Return stored result or it's still evaluating so this must be a cyclic check
                    return false; 
            }
        }

        bool CheckConditionsInternal()
        {
            if (m_Conditions.Count == 0)
                return false;

            for (int i = 0; i < m_Conditions.Count; ++i)
            {
                if (m_Conditions[i].CheckCondition(connection.source))
                {
                    if (m_TransitionOn == MotionGraphConnection.ConditionRequirements.AnyTrue)
                        return true;
                }
                else
                {
                    if (m_TransitionOn == MotionGraphConnection.ConditionRequirements.AllTrue)
                        return false;
                }
            }
            return m_TransitionOn == MotionGraphConnection.ConditionRequirements.AllTrue;
        }

        public void CheckReferences(IMotionGraphMap map)
        {
            m_Connection = map.Swap(m_Connection);
            for (int i = 0; i < m_Conditions.Count; ++i)
                m_Conditions[i] = map.Swap(m_Conditions[i]);
        }

        public void OnValidate ()
        {
            for (int i = m_Conditions.Count; i > 0; --i)
            {
                if (m_Conditions[i - 1] == null)
                    m_Conditions.RemoveAt(i - 1);
                else
                    m_Conditions[i - 1].OnValidate();
            }
        }        
    }
}