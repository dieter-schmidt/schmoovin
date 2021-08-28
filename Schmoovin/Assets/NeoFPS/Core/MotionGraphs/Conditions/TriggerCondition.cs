using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Parameters/Trigger")]
    public class TriggerCondition : MotionGraphCondition
    {
        [SerializeField] private TriggerParameter m_Property = null;

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_Property == null)
                return false;
            else
                return m_Property.CheckTrigger();
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Property = map.Swap(m_Property);
        }
    }
}