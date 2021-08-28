using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ThrownProjectiles
{
    [RequireComponent(typeof(Rigidbody))]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-contactgrenadethrownprojectile.html")]
    public class ContactGrenadeThrownProjectile : ThrownWeaponProjectile
    {
        [SerializeField, NeoPrefabField(required = true), Tooltip("The explosion object to spawn at the impact location")]
        private PooledExplosion m_Explosion = null;

        [SerializeField, Tooltip("The damage the explosion does at its center.")]
        private float m_ExplosionDamage = 50f;

        [SerializeField, Tooltip("The max force to be imparted onto any objects in the explosion radius. The force falls off as distance from the center increases. Requires either a rigidbody or an impact handler.")]
        private float m_MaxForce = 500f;

        private PooledObject m_Prototype = null;
        private Rigidbody m_Rigidbody = null;
        private bool m_Unexploded = false;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_ExplosionDamage < 0f)
                m_ExplosionDamage = 0f;
            if (m_MaxForce < 0f)
                m_MaxForce = 0f;
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            m_Rigidbody = GetComponent<Rigidbody>();
            if (m_Explosion != null)
                m_Prototype = m_Explosion.GetComponent<PooledObject>();
        }

        public override void Throw(Vector3 velocity, IDamageSource source)
        {
            base.Throw(velocity, source);
            m_Rigidbody.velocity = velocity;
            m_Unexploded = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (m_Unexploded)
            {
                m_Unexploded = false;

                if (m_Prototype != null)
                {
                    var explosion = PoolManager.GetPooledObject<PooledExplosion>(m_Prototype, transform.position, Quaternion.identity);
                    explosion.Explode(m_ExplosionDamage, m_MaxForce, damageSource);
                }
                else
                    Debug.LogError("Explosion prototype not set for GrenadeThrownProjectile object.", gameObject);

                pooledObject.ReturnToPool();
            }
        }
    }
}
