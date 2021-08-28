using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Physics/Capsule Cast")]
    public class CapsuleCastCondition : MotionGraphCondition
    {
        [SerializeField] private Vector3 m_CastVector = Vector3.forward;
        [SerializeField] private LayerMask m_LayerMask = (int)PhysicsFilter.Masks.CharacterBlockers;
        [SerializeField] private bool m_DoesHit = true;
        
        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            return controller.characterController.CapsuleCast(m_CastVector, Space.Self, m_LayerMask) == m_DoesHit;
        }
    }
}

