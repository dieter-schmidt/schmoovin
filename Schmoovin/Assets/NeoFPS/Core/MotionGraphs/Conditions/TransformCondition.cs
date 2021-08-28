using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Parameters/Transform")]
    public class TransformCondition : MotionGraphCondition
    {
        [SerializeField] private TransformParameter m_Property = null;
        [SerializeField] private bool m_IsNull = false;

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_Property != null)
                return (m_Property.value == null) == m_IsNull;
            return false;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Property = map.Swap(m_Property);
        }
    }
}