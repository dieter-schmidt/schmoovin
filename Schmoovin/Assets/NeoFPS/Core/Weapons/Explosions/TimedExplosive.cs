using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    public class TimedExplosive : MonoBehaviour
    {
        [SerializeField, NeoPrefabField(typeof (PooledExplosion), required = true), Tooltip("The explosion object to spawn at the impact location")]
        private PooledObject m_Explosion = null;

        [SerializeField, RequiredObjectProperty, Tooltip("")]
        private Transform m_ExplosionPoint = null;

        [SerializeField, Tooltip("The time from activation to explosion")]
        private float m_Delay = 3f;

        [SerializeField, Tooltip("The maximum damage the explosion will do at the center of its area of effect. Drops off to zero at the edge of its radius.")]
        private float m_Damage = 25f;

        [SerializeField, Tooltip("The maximum force to be imparted onto objects in the area of effect. Requires either a Rigidbody or an impact handler.")]
        private float m_MaxForce = 15f;

        private float m_Timer = 0f;

        private void OnValidate()
        {
            if (m_ExplosionPoint == null)
                m_ExplosionPoint = transform;

            if (m_Delay < 0.1f)
                m_Delay = 0.1f;
            if (m_Damage < 0f)
                m_Damage = 0f;
            if (m_MaxForce < 0f)
                m_MaxForce = 0f;
        }

        private void OnEnable()
        {
            m_Timer = m_Delay;
        }

        private void FixedUpdate()
        {
            m_Timer -= Time.deltaTime;
            if (m_Timer <= 0f)
            {
                var explosion = PoolManager.GetPooledObject<PooledExplosion>(m_Explosion, m_ExplosionPoint.position, Quaternion.identity);
                explosion.Explode(m_Damage, m_MaxForce, GetComponent<IDamageSource>()); // Ehhhhhhh

                var pooledObject = GetComponent<PooledObject>();
                if (pooledObject != null)
                    pooledObject.ReturnToPool();
                else
                    Destroy(gameObject);
            }
        }
    }
}