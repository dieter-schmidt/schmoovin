using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Character/Ground Surface Normal")]
    public class GroundSurfaceNormalCondition : MotionGraphCondition
    {
        [SerializeField] private float m_SlopeAngle = 45f;
        [SerializeField] private ComparisonType m_Comparison = ComparisonType.GreaterThan;

        public enum ComparisonType
        {
            GreaterThan,
            GreaterOrEqual,
            LessThan,
            LessOrEqual,
            EqualTo
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (!controller.characterController.isGrounded)
                return false;

            float groundSlopeAngle = Vector3.Angle(controller.characterController.groundSurfaceNormal, controller.characterController.up);
            switch (m_Comparison)
            {
                case ComparisonType.EqualTo:
                    return Mathf.Approximately(groundSlopeAngle, m_SlopeAngle);
                case ComparisonType.GreaterThan:
                    return groundSlopeAngle > m_SlopeAngle;
                case ComparisonType.GreaterOrEqual:
                    return groundSlopeAngle >= m_SlopeAngle;
                case ComparisonType.LessThan:
                    return groundSlopeAngle < m_SlopeAngle;
                case ComparisonType.LessOrEqual:
                    return groundSlopeAngle <= m_SlopeAngle;
            }

            return false;
        }
    }
}