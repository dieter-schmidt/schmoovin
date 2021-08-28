using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Physics/Capsule Lookahead")]
    public class CapsuleLookaheadCondition : MotionGraphCondition
    {
        [SerializeField, Tooltip("In which direction should the lookahead check")]
        private LookaheadType m_Lookahead = LookaheadType.VelocityAllAxes;
        [SerializeField, Tooltip("How far in the future to look ahead (does not take gravity/acceleration into account).")]
        private float m_MultiplyValue = 1f;
        [SerializeField, Tooltip("The layers to check against.")]
        private LayerMask m_LayerMask = (int)PhysicsFilter.Masks.CharacterBlockers;
        [SerializeField, Tooltip("Is the condition true if the cast hits something or if it doesn't.")]
        private bool m_DoesHit = true;

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
                return false;

            // Return cast result
            return controller.characterController.CapsuleCast(castVector, space, m_LayerMask) == m_DoesHit;
        }
    }
}