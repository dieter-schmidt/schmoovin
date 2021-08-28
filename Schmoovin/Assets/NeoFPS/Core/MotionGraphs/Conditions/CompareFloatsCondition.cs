using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Parameters/Compare Floats")]
    public class CompareFloatsCondition : MotionGraphCondition
    {
        [SerializeField] private FloatParameter m_PropertyA = null;
        [SerializeField] private FloatParameter m_PropertyB = null;
        [SerializeField] private ComparisonType m_Comparison = ComparisonType.Equal;

        public enum ComparisonType
        {
            Equal,
            NotEqual,
            Greater,
            GreaterOrEqual,
            Less,
            LessOrEqual
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_PropertyA != null && m_PropertyB != null)
            {
                switch (m_Comparison)
                {
                    case ComparisonType.Equal:
                        return Mathf.Approximately(m_PropertyA.value, m_PropertyB.value);
                    case ComparisonType.NotEqual:
                        return !Mathf.Approximately(m_PropertyA.value, m_PropertyB.value);
                    case ComparisonType.Greater:
                        return m_PropertyA.value > m_PropertyB.value;
                    case ComparisonType.GreaterOrEqual:
                        return m_PropertyA.value >= m_PropertyB.value;
                    case ComparisonType.Less:
                        return m_PropertyA.value < m_PropertyB.value;
                    case ComparisonType.LessOrEqual:
                        return m_PropertyA.value <= m_PropertyB.value;
                }
            }
            return false;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_PropertyA = map.Swap(m_PropertyA);
            m_PropertyB = map.Swap(m_PropertyB);
        }
    }
}