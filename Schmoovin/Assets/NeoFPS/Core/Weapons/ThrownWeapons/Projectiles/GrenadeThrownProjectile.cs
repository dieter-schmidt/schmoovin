using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ThrownProjectiles
{
    [RequireComponent(typeof(Rigidbody))]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-grenadethrownprojectile.html")]
    public class GrenadeThrownProjectile : ThrownWeaponProjectile
    {
        [SerializeField, NeoPrefabField(required = true), Tooltip("The explosion object to spawn once the timer expires.")]
        private PooledExplosion m_Explosion = null;

        [SerializeField, Tooltip("The delay before exploding.")]
        private float m_Delay = 5f;

        [SerializeField, Tooltip("The damage the explosion does at its center.")]
        private float m_ExplosionDamage = 50f;

        [SerializeField, Tooltip("The max force to be imparted onto any objects in the explosion radius. The force falls off as distance from the center increases. Requires either a rigidbody or an impact handler.")]
        private float m_MaxForce = 500f;

        private PooledObject m_Prototype = null;
        private Rigidbody m_Rigidbody = null;
        private float m_Timer = 0f;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_Delay < 0.5f)
                m_Delay = 0.5f;
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

        void OnEnable()
        {
            // Start timer
            m_Timer = 0f;
        }

        public override void Throw(Vector3 velocity, IDamageSource source)
        {
            base.Throw(velocity, source);
            m_Rigidbody.velocity = velocity;
        }

        void Update()
        {
            m_Timer += Time.deltaTime;

            if (m_Timer > m_Delay)
            {
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

        private static readonly NeoSerializationKey k_TimerKey = new NeoSerializationKey("timer");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_TimerKey, m_Timer);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_TimerKey, out m_Timer, 0f);
        }
    }
}
