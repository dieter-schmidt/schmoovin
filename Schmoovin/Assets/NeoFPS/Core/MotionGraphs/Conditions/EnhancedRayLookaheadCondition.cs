using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Physics/Ray Lookahead (Enhanced)")]
    public class EnhancedRayLookaheadCondition : MotionGraphCondition
    {
        [SerializeField, Tooltip("In which direction should the lookahead check")]
        private LookaheadType m_Lookahead = LookaheadType.VelocityAllAxes;
        [SerializeField, Tooltip("How far in the future to look ahead (does not take gravity/acceleration into account).")]
        private float m_MultiplyValue = 1f;
        [SerializeField, Tooltip("The point on the character capsule to cast from. 0 is the base of the capsule. 1 is the top of the capsule.")]
        private float m_NormalisedHeight = 0f;
        [SerializeField, Tooltip("The layers to check against.")]
        private LayerMask m_LayerMask = (int)PhysicsFilter.Masks.CharacterBlockers;
        [SerializeField, Tooltip("Is the condition true if the cast hits something or if it doesn't.")]
        private bool m_DoesHit = true;
        [SerializeField, Tooltip("The vector parameter to output the hit point to (optional).")]
        private VectorParameter m_OutputPoint = null;
        [SerializeField, Tooltip("The vector parameter to output the hit normal to (optional).")]
        private VectorParameter m_OutputNormal = null;
        [SerializeField, Tooltip("The transform parameter to output the hit transform to (optional).")]
        private TransformParameter m_OutputTransform = null;

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            // Get the cast vector
            Space space = Space.Self;
            Vector3 castVector = Vector3.zero;

            switch (m_Lookahead)
            {
                case LookaheadType.VelocityAllAxes:
                    castVector = controller.characterController.velocity * m_MultiplyValue;
                    space = Space.World;
                    break;
                case LookaheadType.VelocityHorizontalPlane:
                    castVector = Quaternion.Inverse(controller.localTransform.rotation) * controller.characterController.velocity * m_MultiplyValue;
                    castVector.y = 0f;
                    break;
                case LookaheadType.VelocityVertical:
                    castVector = Quaternion.Inverse(controller.localTransform.rotation) * controller.characterController.velocity * m_MultiplyValue;
                    castVector.x = 0f;
                    castVector.z = 0f;
                    break;
                case LookaheadType.DirectionAllAxes:
                    castVector = controller.characterController.velocity.normalized * m_MultiplyValue;
                    space = Space.World;
                    break;
                case LookaheadType.DirectionHorizontalPlane:
                    castVector = Quaternion.Inverse(controller.localTransform.rotation) * controller.characterController.velocity;
                    castVector.y = 0f;
                    castVector = castVector.normalized * m_MultiplyValue;
                    break;
                case LookaheadType.DirectionVertical:
                    castVector = Quaternion.Inverse(controller.localTransform.rotation) * controller.characterController.velocity;
                    castVector.x = 0f;
                    castVector.z = 0f;
                    castVector = castVector.normalized * m_MultiplyValue;
                    break;
                case LookaheadType.InputDirection:
                    if (controller.inputMoveScale > 0.05f)
                    {
                        castVector = new Vector3(controller.inputMoveDirection.x, 0f, controller.inputMoveDirection.y);
                        castVector *= m_MultiplyValue;
                    }
                    break;
            }

            // Quick check for valid cast vector
            if (castVector.sqrMagnitude < 0.0001f)
            {
                if (m_OutputPoint != null)
                    m_OutputPoint.value = Vector3.zero;
                if (m_OutputNormal != null)
                    m_OutputNormal.value = Vector3.zero;
                if (m_OutputTransform != null)
                    m_OutputTransform.value = null;
                return false;
            }

            // Perform the cast
            RaycastHit hit;
            bool didHit = controller.characterController.RayCast(m_NormalisedHeight, castVector, space, out hit, m_LayerMask);

            // Output to paramters
            if (m_OutputPoint != null)
                m_OutputPoint.value = hit.point;
            if (m_OutputNormal != null)
                m_OutputNormal.value = hit.normal;
            if (m_OutputTransform != null)
                m_OutputTransform.value = hit.transform;

            // return does hit
            return didHit == m_DoesHit;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_OutputPoint = map.Swap(m_OutputPoint);
            m_OutputNormal = map.Swap(m_OutputNormal);
            m_OutputTransform = map.Swap(m_OutputTransform);
        }
    }
}