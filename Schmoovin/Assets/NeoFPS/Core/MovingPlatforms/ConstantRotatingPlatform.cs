using UnityEngine;

namespace NeoFPS
{
    class ConstantRotatingPlatform : BaseMovingPlatform
    {
        [SerializeField, Tooltip("The rotation speed on each axis in world space.")]
        private Vector3 m_RotationPerSecond = Vector3.zero;

        protected override Vector3 GetNextPosition()
        {
            return fixedPosition;
        }

        protected override Quaternion GetNextRotation()
        {
            return fixedRotation * Quaternion.Euler(m_RotationPerSecond * Time.deltaTime);
        }
    }
}
