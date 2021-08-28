#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;
using NeoCC;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Physics/Climbable")]
    public class ClimbableCondition : MotionGraphCondition
    {
        [SerializeField, Tooltip("The vector parameter to store the wall normal in once the check is complete")]
        private VectorParameter m_OutputWallNormal = null;
        [SerializeField, Tooltip("The direction to check in. YawForward casts directly forward in the direction the character is facing. InverseWallNormal takes the value already set in OutputWallNormal and casts back along it.")]
        private CheckDirection m_CheckDirection = CheckDirection.YawForward;
        [SerializeField, Range(0.05f, 0.5f), Tooltip("The distance to perform the initial wall check.")]
        private FloatDataReference m_CheckDistance = new FloatDataReference(0.25f);
        [SerializeField, Tooltip("The collision mask to use when checking the wall normal")]
        private LayerMask m_WallCollisionMask = PhysicsFilter.Masks.StaticCharacterBlockers;
        [SerializeField, Tooltip("The maximum height the character can move up to climb over the ledge")]
        private float m_MaxClimbHeight = 1.5f;
        [SerializeField, Tooltip("The distance the character should be able to move forwards past the edge of the climbable surface")]
        private float m_ClimbForward = 0.25f;
        [SerializeField, Tooltip("An optional parameter to store the climb height")]
        private FloatParameter m_OutputClimbHeight = null;

        public enum CheckDirection
        {
            YawForward,
            InverseWallNormal
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            // Check wall vector
            if (m_OutputWallNormal == null)
                return false;
            
            var ncc = controller.characterController;
            float radius = ncc.radius;
            float height = ncc.height;
            RaycastHit hit;

            // Get the check direction
            Vector3 checkForward = Vector3.zero;
            if (m_CheckDirection == CheckDirection.YawForward)
            {
                // Check collision flags
                //if ((ncc.collisionFlags & (NeoCharacterCollisionFlags.MaskFront | NeoCharacterCollisionFlags.Below)) == NeoCharacterCollisionFlags.None)
                //    return false;

                // Use the character forward
                checkForward = ncc.forward;
            }
            else
            {
                // Only works if the stored wall normal has length
                float mag = m_OutputWallNormal.value.magnitude;
                if (mag < 0.5f)
                    return false;

                // Use the inverse of the normal
                checkForward = m_OutputWallNormal.value / -mag;
            }

#if NEOFPS_LIGHTWEIGHT

            // Cast and get the initial wall contact
            if (ncc.CapsuleCast(checkForward * m_CheckDistance.value, Space.World, out hit, m_WallCollisionMask, QueryTriggerInteraction.Ignore))
            {
                // Record the wall normal
                m_OutputWallNormal.value = hit.normal;

                // Get head point at wall contact
                Vector3 top = controller.localTransform.position;
                top.y += height - radius;
                top += checkForward * hit.distance;

                // Sphere cast up the wall from the head point
                if (PhysicsExtensions.SphereCastNonAllocSingle(
                    new Ray(top, Vector3.up),
                    radius - ncc.skinWidth,
                    out hit,
                    m_MaxClimbHeight,
                    PhysicsFilter.Masks.CharacterBlockers,
                    controller.localTransform,
                    QueryTriggerInteraction.Ignore
                    ))
                {
                    // Hit a ceiling
                    top = hit.point + hit.normal * radius;
                }
                else
                {
                    // No obstruction overhead
                    top.y += m_MaxClimbHeight;
                }

                // Get forward direction
                checkForward = -m_OutputWallNormal.value;
                checkForward.y = 0f;
                checkForward.Normalize();

                // Cast forward
                if (PhysicsExtensions.CapsuleCastNonAllocSingle(
                    top,
                    top - ncc.up * (height - radius * 2f),
                    radius - ncc.skinWidth,
                    checkForward,
                    out hit,
                    radius + m_ClimbForward,
                    PhysicsFilter.Masks.CharacterBlockers,
                    controller.localTransform,
                    QueryTriggerInteraction.Ignore
                    ))
                {
                    // No ledge to step onto, or ledge obstructed
                    return false;
                }

                // Route clear
                return true;
            }

#else

            // Cast and get the initial wall contact
            if (ncc.CapsuleCast(checkForward * m_CheckDistance.value, Space.World, out hit, m_WallCollisionMask, QueryTriggerInteraction.Ignore))
            {
                // Record the wall normal
                m_OutputWallNormal.value = hit.normal;
                
                // Get wall up vector
                Vector3 projectedUp = Vector3.ProjectOnPlane(ncc.up, hit.normal).normalized;

                // Get head point at wall contact
                Vector3 top = controller.localTransform.position + ncc.up * (height - radius);
                top += checkForward * hit.distance;

                float verticalTravel = 0f;

                // Sphere cast up the wall from the head point
                if (PhysicsExtensions.SphereCastNonAllocSingle(
                    new Ray(top, projectedUp),
                    radius - ncc.skinWidth,
                    out hit,
                    m_MaxClimbHeight,
                    PhysicsFilter.Masks.CharacterBlockers,
                    controller.localTransform,
                    QueryTriggerInteraction.Ignore
                    ))
                {
                    // Hit a ceiling
                    top = hit.point + hit.normal * radius;
                    verticalTravel += hit.distance;
                }
                else
                {
                    // No obstruction overhead
                    top = top + projectedUp * m_MaxClimbHeight;
                    verticalTravel += m_MaxClimbHeight;
                }

                // Get forward direction
                checkForward = Vector3.ProjectOnPlane(-m_OutputWallNormal.value, ncc.up).normalized;

                // Cast forward
                var bottom = top - ncc.up * (height - radius * 2f);
                if (PhysicsExtensions.CapsuleCastNonAllocSingle(
                    top,
                    bottom,
                    radius - ncc.skinWidth,
                    checkForward,
                    out hit,
                    radius + m_ClimbForward,
                    PhysicsFilter.Masks.CharacterBlockers,
                    controller.localTransform,
                    QueryTriggerInteraction.Ignore
                    ))
                {
                    // No ledge to step onto, or ledge obstructed
                    return false;
                }

                // If required, get the climb height
                if (m_OutputClimbHeight != null)
                {
                    // Raycast down to get a rough approximation of the height to climb
                    bottom += checkForward;
                    if (PhysicsExtensions.SphereCastNonAllocSingle(
                        new Ray(bottom, -ncc.up),
                        radius - ncc.skinWidth,
                        out hit,
                        verticalTravel,
                        PhysicsFilter.Masks.CharacterBlockers,
                        controller.localTransform,
                        QueryTriggerInteraction.Ignore
                        ))
                    {
                        m_OutputClimbHeight.value = verticalTravel - hit.distance;
                    }
                }

                // Route clear
                return true;
            }
#endif

            // No wall to climb
            return false;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_OutputWallNormal = map.Swap(m_OutputWallNormal);
            m_OutputClimbHeight = map.Swap(m_OutputClimbHeight);
        }
    }
}