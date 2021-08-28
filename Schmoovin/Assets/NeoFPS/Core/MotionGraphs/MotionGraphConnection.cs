using NeoFPS.CharacterMotion.Conditions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    public class MotionGraphConnection : ScriptableObject, IMotionGraphElement
    {
        [SerializeField] private MotionGraphConnectable m_Source = null;
        [SerializeField] private MotionGraphConnectable m_Destination = null;
        [SerializeField] private ConditionRequirements m_TransitionOn = ConditionRequirements.AllTrue;
        [SerializeField] private List<MotionGraphCondition> m_Conditions = new List<MotionGraphCondition>();
        [SerializeField] private List<MotionGraphConditionGroup> m_ConditionGroups = new List<MotionGraphConditionGroup>();

        public enum ConditionRequirements
        {
            AllTrue,
            AnyTrue
        }

        public MotionGraphConnectable source
        {
            get { return m_Source; }
        }

        public MotionGraphConnectable destination
        {
            get { return m_Destination; }
        }

        public List<MotionGraphCondition> conditions
        {
            get { return m_Conditions; }
        }

        public List<MotionGraphConditionGroup> conditionGroups
        {
            get { return m_ConditionGroups; }
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

        public bool isValid
        {
            get
            {
                if (source == null || destination == null)
                    return false;
                if ((destination.parent != source.parent) && (destination.parent != source))
                    return false;
                return true;
            }
        }

        public bool CheckConditions ()
        {
            if (m_Conditions.Count == 0)
                return false;

            var destinationState = destination as MotionGraphState;
            if (destinationState != null && destinationState.active)
                return false;

            // Reset groups (results are tracked to prevent cyclic references)
            for (int i = 0; i < m_ConditionGroups.Count; ++i)
                m_ConditionGroups[i].ResetGroup();
            
            // Test conditions
            for (int i = 0; i < m_Conditions.Count; ++i)
            {
                if (m_Conditions[i].CheckCondition(source))
                {
                    if (m_TransitionOn == ConditionRequirements.AnyTrue)
                        return true;
                }
                else
                {
                    if (m_TransitionOn == ConditionRequirements.AllTrue)
                        return false;
                }
            }
            return m_TransitionOn == ConditionRequirements.AllTrue;
        }

        public void Initialise (IMotionController c)
        {
            for (int i = 0; i < m_Conditions.Count; ++i)
                m_Conditions[i].Initialise(c);
            for (int i = 0; i < m_ConditionGroups.Count; ++i)
                m_ConditionGroups[i].Initialise(c);
        }

        public void CheckReferences (IMotionGraphMap map)
        {
            m_Source = map.Swap(m_Source);
            m_Destination = map.Swap(m_Destination);
            for (int i = 0; i < m_Conditions.Count; ++i)
                m_Conditions[i] = map.Swap(m_Conditions[i]);
            for (int i = 0; i < m_ConditionGroups.Count; ++i)
                m_ConditionGroups[i].CheckReferences(map);
        }

#if UNITY_EDITOR
        public string GetUniqueConditionGroupName(string n, int index)
        {
            bool unique = false;
            string result = n;

            int suffix = 1;
            while (!unique)
            {
                unique = true;
                for (int i = 0; i < m_ConditionGroups.Count; ++i)
                {
                    if (i == index)
                        continue;

                    if (m_ConditionGroups[i].name == result)
                    {
                        unique = false;
                        result = n + suffix++;
                        break;
                    }
                }
            }

            return result;
        }
#endif
    }
}