using NeoCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class CharacterImpactHandler : MonoBehaviour, IImpactHandler
    {
        [SerializeField, Tooltip("Use this to exagerate the effects of forces compared to solid items of similar mass.")]
        private float m_ForceMultiplier = 2.5f;
        [SerializeField, Tooltip("Use this to clamp the multiplied force to prevent crazy effects")]
        private float m_MaxForce = 2000f;

        INeoCharacterController m_CharacterController = null;
        
        void Awake()
        {
            m_CharacterController = GetComponentInParent<INeoCharacterController>();
        }

        public void HandlePointImpact(Vector3 position, Vector3 force)
        {
            if (m_CharacterController != null)
                m_CharacterController.AddForce(Vector3.ClampMagnitude(force * m_ForceMultiplier, m_MaxForce), ForceMode.Impulse, true);
        }
    }
}