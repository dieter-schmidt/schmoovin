using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Parameters/Compare Time")]
    public class CompareTimeCondition : MotionGraphCondition
    {
        [SerializeField] private FloatParameter m_TimeValue = null;
        [SerializeField] private float m_CompareValue = 0f;
        [SerializeField] private ComparisonType m_ComparisonType = ComparisonType.Greater;

        public enum ComparisonType
        {
            Greater,
            GreaterOrEqual,
            Less,
            LessOrEqual
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_TimeValue != null)
            {
                float diff = Time.time - m_TimeValue.value;
                switch (m_ComparisonType)
                {
                    case ComparisonType.Greater:
                        return diff > m_CompareValue;
                    case ComparisonType.GreaterOrEqual:
                        return diff >= m_CompareValue;
                    case ComparisonType.Less:
                        return diff < m_CompareValue;
                    case ComparisonType.LessOrEqual:
                        return diff <= m_CompareValue;
                }
            }

            return false; 
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_TimeValue = map.Swap(m_TimeValue);
        }
    }
}