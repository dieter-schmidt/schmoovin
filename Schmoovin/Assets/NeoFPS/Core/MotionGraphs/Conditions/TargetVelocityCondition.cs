#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Character/TargetVelocity")]
    public class TargetVelocityCondition : MotionGraphCondition
    {
        [SerializeField] private VelocityType m_VelocityType = VelocityType.CharacterSpeed;
        [SerializeField] private ComparisonType m_ComparisonType = ComparisonType.GreaterThan;
        [SerializeField] private float m_CompareValue = 0f;

        public enum VelocityType
        {
            CharacterSpeed,
            HorizontalSpeed,
            VerticalVelocity,
            GroundSpeed,
            GroundSurfaceSpeed,
            YawVelocity,
            YawGroundVelocity,
            YawGroundSurfaceVelocity
        }

        public enum ComparisonType
        {
            EqualTo,
            NotEqualTo,
            GreaterThan,
            GreaterOrEqual,
            LessThan,
            LessOrEqual
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            float lhs = 0f;
            bool squared = false;

            switch (m_VelocityType)
            {
                case VelocityType.CharacterSpeed:
                    lhs = controller.characterController.targetVelocity.sqrMagnitude;
                    squared = true;
                    break;
#if NEOFPS_LIGHTWEIGHT
                case VelocityType.HorizontalSpeed:
                    var v = controller.characterController.targetVelocity;
                    v.y = 0f;
                    lhs = v.sqrMagnitude;
                    squared = true;
                    break;
#else
                case VelocityType.HorizontalSpeed:
                    lhs = Vector3.ProjectOnPlane(
                        controller.characterController.targetVelocity,
                        controller.characterController.up
                        ).sqrMagnitude;
                    squared = true;
                    break;
#endif
                case VelocityType.VerticalVelocity:
                    lhs = Vector3.Dot(
                        controller.characterController.targetVelocity,
                        controller.characterController.up
                        );
                    break;
                case VelocityType.GroundSpeed:
                    lhs = Vector3.ProjectOnPlane(
                        controller.characterController.targetVelocity,
                        controller.characterController.groundNormal
                        ).sqrMagnitude;
                    squared = true;
                    break;
                case VelocityType.GroundSurfaceSpeed:
                    lhs = Vector3.ProjectOnPlane(
                        controller.characterController.targetVelocity,
                        controller.characterController.groundSurfaceNormal
                        ).sqrMagnitude;
                    squared = true;
                    break;
                case VelocityType.YawVelocity:
                    lhs = Vector3.Dot(
                        controller.characterController.targetVelocity,
                        controller.characterController.forward
                        );
                    break;
                case VelocityType.YawGroundVelocity:
                    {
                        var groundVelocity = Vector3.ProjectOnPlane(
                            controller.characterController.targetVelocity,
                            controller.characterController.groundNormal
                            );
                        lhs = Vector3.Dot(
                            groundVelocity,
                            controller.characterController.forward
                            );
                    }
                    break;
                case VelocityType.YawGroundSurfaceVelocity:
                    {
                        var groundVelocity = Vector3.ProjectOnPlane(
                        controller.characterController.targetVelocity,
                        controller.characterController.groundSurfaceNormal
                        );
                        lhs = Vector3.Dot(
                            groundVelocity,
                            controller.characterController.forward
                            );
                    }
                    break;
            }

            // Get right hand side of comparison
            float rhs = m_CompareValue;
            if (squared)
                rhs *= rhs;

            switch (m_ComparisonType)
            {
                case ComparisonType.EqualTo:
                    return Mathf.Approximately(lhs, rhs);
                case ComparisonType.NotEqualTo:
                    return !Mathf.Approximately(lhs, rhs);
                case ComparisonType.GreaterThan:
                    return lhs > rhs;
                case ComparisonType.GreaterOrEqual:
                    return lhs >= rhs;
                case ComparisonType.LessThan:
                    return lhs < rhs;
                case ComparisonType.LessOrEqual:
                    return lhs <= rhs;
            }

            return false;
        }
    }
}