using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Graph/Elapsed Time")]
    public class ElapsedTimeCondition : MotionGraphCondition 
    {
        [SerializeField] private FloatParameter m_TimeoutProperty = null;
        [SerializeField] private float m_TimeoutValue = 1f;

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_TimeoutProperty != null)
                return connectable.elapsedTime >= m_TimeoutProperty.value;
            else
                return connectable.elapsedTime >= m_TimeoutValue;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_TimeoutProperty = map.Swap(m_TimeoutProperty);
        }
    }
}