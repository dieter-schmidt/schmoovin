using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Character/Height Restriction")]
    public class HeightRestrictionCondition : MotionGraphCondition
    {
        [SerializeField] private float m_TargetHeight = 1f;
        [SerializeField] private bool m_Blocked = false;

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            return controller.CheckIsHeightMultiplierRestricted(m_TargetHeight) == m_Blocked;
        }
    }
}