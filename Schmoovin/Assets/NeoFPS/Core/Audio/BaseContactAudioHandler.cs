using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class BaseContactAudioHandler : MonoBehaviour
    {
        [SerializeField, Tooltip("The minimum impulse between this object and the object it collides with for an impact noise to be played.")]
        private float m_MinImpulse = 2f;

        [SerializeField, Tooltip("The minimum time between impact sounds")]
        private float m_MinDelay = 0.5f;

        private bool m_Collided = false;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (m_MinImpulse < 0f)
                m_MinImpulse = 0f;
        }
#endif
        protected virtual void Awake()
        {
        }

        void OnCollisionExit()
        {
            m_Collided = false;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (m_Collided)
                return;

            if (m_MinImpulse == 0f || collision.impulse.sqrMagnitude > (m_MinImpulse * m_MinImpulse))
            {
                PlayContactAudio(collision);

                if (m_MinDelay > 0f)
                {
                    m_Collided = true;
                    StartCoroutine(TimeoutContactAudio());
                }
            }
        }

        IEnumerator TimeoutContactAudio()
        {
            float timer = 0f;
            while (timer < m_MinDelay)
            {
                yield return null;
                timer += Time.deltaTime;
            }
        }

        protected abstract void PlayContactAudio(Collision collision);
    }
}