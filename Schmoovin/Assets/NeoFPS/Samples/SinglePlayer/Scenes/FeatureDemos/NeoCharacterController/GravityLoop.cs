using NeoCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.Samples.SinglePlayer
{
    [RequireComponent(typeof(BoxCollider))]
    public class GravityLoop : MonoBehaviour
    {
        [SerializeField] private float m_StraightLength = 20f;

        private Collider m_ControllerCollider = null;
        private INeoCharacterController m_Controller = null;
        private float m_GravityStrength = 9.82f;
        private Transform m_LocalTransform = null;

        private void Awake()
        {
            m_LocalTransform = transform;
        }

        private void OnTriggerEnter(Collider other)
        {
            var cc = other.GetComponent<INeoCharacterController>();
            if (cc != null && cc.characterGravity != null)
            {
                if (cc.characterGravity != null)
                    cc.characterGravity.gravity = Vector3.down * m_GravityStrength;

                m_Controller = cc;
                m_ControllerCollider = other;
                m_GravityStrength = m_Controller.gravity.magnitude;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == m_ControllerCollider)
            {
                m_Controller.characterGravity.gravity = Vector3.down * m_GravityStrength;
                m_Controller = null;
                m_ControllerCollider = null;
            }
        }

        void FixedUpdate()
        {
            if (m_Controller as Component == null)
                return;

            Vector3 position = m_Controller.transform.position + (m_Controller.up * m_Controller.radius);
            position = m_LocalTransform.InverseTransformPoint(position); // Check this???

            // Check if in center zone
            if (position.z >= m_StraightLength * -0.5f && position.z <= m_StraightLength * 0.5f)
            {
                if (position.y > 0f)
                    m_Controller.characterGravity.gravity = Vector3.up * m_GravityStrength;
                else
                    m_Controller.characterGravity.gravity = Vector3.down * m_GravityStrength;
            }
            else
            {
                position.z += m_StraightLength * 0.5f * -Mathf.Sign(position.z);
                position.x = 0f;
                m_Controller.characterGravity.gravity = position.normalized * m_GravityStrength;
            }
        }
    }
}