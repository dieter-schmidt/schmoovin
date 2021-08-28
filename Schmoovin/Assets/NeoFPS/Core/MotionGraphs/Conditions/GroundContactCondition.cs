using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Character/Ground Contact")]
    public class GroundContactCondition : MotionGraphCondition
    {
        [SerializeField] private bool m_Comparison = false;

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            return controller.characterController.isGrounded == m_Comparison;
        }
    }
}