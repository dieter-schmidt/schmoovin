using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Parameters/Integer")]
    public class IntCondition : MotionGraphCondition
    {
        [SerializeField] private IntParameter m_Property = null;
        [SerializeField] private int m_CompareValue = 0;
        [SerializeField] private ComparisonType m_ComparisonType = ComparisonType.EqualTo;

        public enum ComparisonType
        {
            EqualTo,
            NotEqualTo,
            GreaterThan,
            GreaterOrEqual,
            LessThan,
            LessOrEqual
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_Property != null)
            {
                switch (m_ComparisonType)
                {
                    case ComparisonType.EqualTo:
                        return m_Property.value == m_CompareValue;
                    case ComparisonType.NotEqualTo:
                        return m_Property.value != m_CompareValue;
                    case ComparisonType.GreaterThan:
                        return m_Property.value > m_CompareValue;
                    case ComparisonType.GreaterOrEqual:
                        return m_Property.value >= m_CompareValue;
                    case ComparisonType.LessThan:
                        return m_Property.value < m_CompareValue;
                    case ComparisonType.LessOrEqual:
                        return m_Property.value <= m_CompareValue;
                }
            }
            return false;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Property = map.Swap(m_Property);
        }
    }
}