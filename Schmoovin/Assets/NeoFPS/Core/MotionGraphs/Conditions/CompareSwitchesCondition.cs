using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Parameters/Compare Switches")]
    public class CompareSwitchesCondition : MotionGraphCondition
    {
        [SerializeField] private SwitchParameter m_PropertyA = null;
        [SerializeField] private SwitchParameter m_PropertyB = null;
        [SerializeField] private ComparisonType m_Comparison = ComparisonType.Equal;

        public enum ComparisonType
        {
            Equal,
            NotEqual
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_PropertyA != null && m_PropertyB != null)
            {
                if (m_Comparison == ComparisonType.Equal)
                    return m_PropertyA.on == m_PropertyB.on;
                else
                    return m_PropertyA.on != m_PropertyB.on;
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