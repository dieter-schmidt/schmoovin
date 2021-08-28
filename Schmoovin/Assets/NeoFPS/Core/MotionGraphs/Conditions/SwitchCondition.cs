using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Parameters/Switch")]
    public class SwitchCondition : MotionGraphCondition 
    {
        [SerializeField] private SwitchParameter m_Property = null;
        [SerializeField] private bool m_CompareValue = true;

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_Property == null)
                return false;
            else
                return m_Property.on == m_CompareValue;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Property = map.Swap(m_Property);
        }
    }
}