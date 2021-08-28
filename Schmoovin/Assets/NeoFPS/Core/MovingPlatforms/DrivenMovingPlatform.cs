using UnityEngine;
using NeoCC;

namespace NeoFPS
{
    /// <summary>
    /// DrivenMovingPlatform is a simple implementation of the IMovingPlatform interface that is driven by some external mechanism such as a script or Animator component.
    /// It simply reads its position each frame. Because of this, it will always be one frame ahead of any contacting character due to interpolation.
    /// A moving platform is an environment object that will move a INeoCharacterController when it is in contact.
    /// </summary>
    public class DrivenMovingPlatform : MonoBehaviour, IMovingPlatform
    {
        /// <summary>
        /// The current fixed update position of the platform in world space (used for interpolation).
        /// </summary>
        public Vector3 fixedPosition { get; private set; }

        /// <summary>
        /// The position of the platform in world space on the last fixed update frame (used for interpolation).
        /// </summary>
        public Vector3 previousPosition { get; private set; }

        /// <summary>
        /// The current fixed update rotation of the platform in world space (used for interpolation).
        /// </summary>
        public Quaternion fixedRotation { get; private set; }

        /// <summary>
        /// The rotation of the platform in world space on the last fixed update frame (used for interpolation).
        /// </summary>
        public Quaternion previousRotation { get; private set; }

        private bool m_Initialised = false;
        private Transform m_LocalTransform = null;
        private Rigidbody m_Rigidbody = null;

        void Awake()
        {
            m_LocalTransform = transform;
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if (!m_Initialised)
            {
                // First time round previous is current
                fixedPosition = m_LocalTransform.position;
                fixedRotation = m_LocalTransform.rotation;
                previousPosition = fixedPosition;
                previousRotation = fixedRotation;

                m_Initialised = true;
            }
            else
            {
                if (m_Rigidbody != null)
                {
                    previousPosition = fixedPosition;
                    previousRotation = fixedRotation;
                    fixedPosition = m_Rigidbody.position;
                    fixedRotation = m_Rigidbody.rotation;
                }
                else
                {
                    previousPosition = fixedPosition;
                    previousRotation = fixedRotation;
                    fixedPosition = m_LocalTransform.position;
                    fixedRotation = m_LocalTransform.rotation;
                }
            }
        }
    }
}
