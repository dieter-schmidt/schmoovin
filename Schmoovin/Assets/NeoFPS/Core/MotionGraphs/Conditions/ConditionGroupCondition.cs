using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    //[MotionGraphElement("Graph/Condition Group")]
    public class ConditionGroupCondition : MotionGraphCondition
    {
        [SerializeField] private MotionGraphConnection m_Connection = null;
        [SerializeField] private int m_ID = 0;

        private int m_Index = -1;

        public int id
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        public override void Initialise(IMotionController c)
        {
            base.Initialise(c);
            if (m_ID != 0)
            {
                for (int i = 0; i < m_Connection.conditionGroups.Count; ++i)
                {
                    if (m_Connection.conditionGroups[i].id == m_ID)
                    {
                        m_Index = i;
                        break;
                    }
                }
            }
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_Index != -1)
                return (m_Connection.conditionGroups[m_Index].CheckConditions());
            else
                return false;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Connection = map.Swap(m_Connection);
            base.CheckReferences(map);
        }
    }
}