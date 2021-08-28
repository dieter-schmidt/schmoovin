using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Physics/Sphere Cast (Enhanced)")]
    public class EnhancedSphereCastCondition : MotionGraphCondition
    {
        [SerializeField, Tooltip("The point on the character capsule to cast from. 0 is the base of the capsule. 1 is the top of the capsule.")]
        private float m_NormalisedHeight = 0f;
        [SerializeField, Tooltip("What to use for the cast vector.")]
        private CastType m_CastType = CastType.LocalVector;
        [SerializeField, Tooltip("The direction and distance to cast relative to the character. The vector does not have to be normalised, as the magnitude will be the maximum distance.")]
        private Vector3 m_CastVector = Vector3.forward;
        [SerializeField, Tooltip("The distance to cast based on the parameter direction vector.")]
        private VectorParameter m_CastVectorParameter = null;
        [SerializeField, Tooltip("The distance to cast based on the parameter direction vector.")]
        private float m_Distance = 1f;
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

        public enum CastType
        {
            LocalVector,
            WorldVector,
            LocalParameter,
            LocalParameterInverse,
            WorldParameter,
            WorldParameterInverse,
        }

        bool GetScaledVector(out Vector3 output, bool inverse)
        {
            if (m_CastVectorParameter == null)
            {
                output = Vector3.zero;
                return false;
            }

            float length = m_CastVectorParameter.value.magnitude;
            if (length < 0.5f)
            {
                output = Vector3.zero;
                return false;
            }

            float sign = inverse ? -1f : 1f;
            output = m_CastVectorParameter.value * (sign * m_Distance / length);

            return true;
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            bool valid = true;
            Vector3 castVector = Vector3.zero;
            Space space = Space.Self;

            switch(m_CastType)
            {
                case CastType.LocalVector:
                    castVector = m_CastVector;
                    space = Space.Self;
                    break;
                case CastType.WorldVector:
                    castVector = m_CastVector;
                    space = Space.World;
                    break;
                case CastType.LocalParameter:
                    valid = GetScaledVector(out castVector, false);
                    space = Space.Self;
                    break;
                case CastType.LocalParameterInverse:
                    valid = GetScaledVector(out castVector, true);
                    space = Space.Self;
                    break;
                case CastType.WorldParameter:
                    valid = GetScaledVector(out castVector, false);
                    space = Space.World;
                    break;
                case CastType.WorldParameterInverse:
                    valid = GetScaledVector(out castVector, true);
                    space = Space.World;
                    break;
            }

            if (valid)
            {
                // Perform the cast
                RaycastHit hit;
                bool didHit = controller.characterController.SphereCast(m_NormalisedHeight, castVector, space, out hit, m_LayerMask);

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
            else
            {
                // Output to paramters
                if (m_OutputPoint != null)
                    m_OutputPoint.value = Vector3.zero;
                if (m_OutputNormal != null)
                    m_OutputNormal.value = Vector3.zero;
                if (m_OutputTransform != null)
                    m_OutputTransform.value = null;

                return false;
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_CastVectorParameter = map.Swap(m_CastVectorParameter);
            m_OutputPoint = map.Swap(m_OutputPoint);
            m_OutputNormal = map.Swap(m_OutputNormal);
            m_OutputTransform = map.Swap(m_OutputTransform);
        }
    }
}

