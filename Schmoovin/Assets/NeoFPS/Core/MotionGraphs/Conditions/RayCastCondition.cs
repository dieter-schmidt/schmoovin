using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Physics/Ray Cast")]
    public class RayCastCondition : MotionGraphCondition
    {
        [SerializeField, Tooltip("The point on the character capsule to cast from. 0 is the base of the capsule centerline. 1 is the top of the capsule centerline.")]
        private float m_NormalisedHeight = 0f;
        [SerializeField, Tooltip("The direction and distance to cast relative to the character. The vector does not have to be normalised, as the magnitude will be the maximum distance.")]
        private Vector3 m_CastVector = Vector3.forward;
        [SerializeField, Tooltip("The layers to check against.")]
        private LayerMask m_LayerMask = (int)PhysicsFilter.Masks.CharacterBlockers;
        [SerializeField, Tooltip("Is the condition true if the cast hits something or if it doesn't.")]
        private bool m_DoesHit = true;
        
        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            return controller.characterController.RayCast(m_NormalisedHeight, m_CastVector, Space.Self, m_LayerMask) == m_DoesHit;
        }
    }
}
