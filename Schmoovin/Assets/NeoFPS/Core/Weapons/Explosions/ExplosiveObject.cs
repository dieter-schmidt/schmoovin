using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-explosiveobject.html")]
    public class ExplosiveObject : MonoBehaviour, IDamageHandler, INeoSerializableComponent
    {
        [Header("Explosion")]

        [SerializeField, NeoPrefabField(required = true), Tooltip("The explosion object to spawn")]
        private PooledExplosion m_Explosion = null;

        [SerializeField, Tooltip("The type of damage the object can take")]
        private Vector3 m_Offset = Vector3.zero;

        [SerializeField, FormerlySerializedAs("m_Damage"), Tooltip("The damage the explosion does at its center (drops off to zero at full radius).")]
        private float m_ExplosionDamage = 25f;

        [SerializeField, FormerlySerializedAs("m_MaxForce"), Tooltip("The force to be imparted onto the hit object (drops off to zero at full radius). Requires either a Rigidbody or an impact handler.")]
        private float m_ExplosionForce = 15f;

        [Header("Health")]

        [SerializeField, Tooltip("The type of damage the object can take")]
        private DamageType m_TakesDamage = DamageType.Default | DamageType.Explosion;

        [SerializeField, Tooltip("The amount of health the item has before exploding")]
        private float m_Health = 1f;

        [SerializeField, Tooltip("What to do to the gameobject when killed.")]
        private DestroyAction m_DestroyGameobject = DestroyAction.Destroy;

        [SerializeField, Tooltip("An event invoked when the item's health reaches zero")]
        private UnityEvent m_OnDestroyed = null;

        public enum DestroyAction
        {
            Destroy,
            Disable,
            Nothing
        }

        private PooledObject m_Prototype = null;
        private float m_StartingHealth = 0f;

        private DamageTeamFilter m_DamageTeamFilter = DamageTeamFilter.All;
        public DamageFilter inDamageFilter
        {
            get { return new DamageFilter(m_TakesDamage, m_DamageTeamFilter); }
            set
            {
                m_TakesDamage = value.GetDamageType();
                m_DamageTeamFilter = value.GetTeamFilter();
            }
        }
		
		public float damageTaken
		{
			get { return m_StartingHealth - m_Health; }
		}

        protected virtual void Awake()
        {
            if (m_Explosion != null)
                m_Prototype = m_Explosion.GetComponent<PooledObject>();
			m_StartingHealth = m_Health;
        }

        public virtual DamageResult AddDamage(float damage)
        {
            return AddDamageInternal(damage, null);
        }

        public virtual DamageResult AddDamage(float damage, RaycastHit hit)
        {
            return AddDamageInternal(damage, null);
        }

        public virtual DamageResult AddDamage(float damage, IDamageSource source)
        {
            return AddDamageInternal(damage, source);
        }

        public virtual DamageResult AddDamage(float damage, RaycastHit hit, IDamageSource source)
        {
            return AddDamageInternal(damage, source);
        }

        DamageResult AddDamageInternal(float damage, IDamageSource source)
        {
            if (source != null && !inDamageFilter.CollidesWith(source.outDamageFilter, FpsGameMode.friendlyFire))
                return DamageResult.Ignored;

            if (m_Health > 0f)
            {
                m_Health -= damage;
                if (m_Health <= 0f)
                    OnKilled(source);

                // Report damage dealt
                if (damage > 0f && source != null && source.controller != null)
                    source.controller.currentCharacter.ReportTargetHit(false);

                return DamageResult.Standard;
            }
            else
                return DamageResult.Ignored;
        }

        protected virtual void OnKilled(IDamageSource source)
        {
            // Invoke the event
            m_OnDestroyed.Invoke();

            // Spawn the explosion
            if (m_Prototype != null)
            {
                Transform t = transform;
                Quaternion rotation = t.rotation;
                var explosion = PoolManager.GetPooledObject<PooledExplosion>(m_Prototype, t.position + rotation * m_Offset, rotation);
                explosion.Explode(m_ExplosionDamage, m_ExplosionForce, source, t);
            }

            switch(m_DestroyGameobject)
            {
                case DestroyAction.Destroy:
                    Destroy(gameObject);
                    break;
                case DestroyAction.Disable:
                    gameObject.SetActive(false);
                    break;
            }
        }

        private static readonly NeoSerializationKey k_FilterKey = new NeoSerializationKey("filter");
        private static readonly NeoSerializationKey k_HealthKey = new NeoSerializationKey("health");

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_HealthKey, m_Health);
            writer.WriteValue(k_FilterKey, inDamageFilter);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_HealthKey, out m_Health, m_Health);
            ushort filter;
            if (reader.TryReadValue(k_FilterKey, out filter, 0))
            {
                var df = (DamageFilter)filter;
                m_DamageTeamFilter = df.GetTeamFilter();
                m_TakesDamage = df.GetDamageType();
            }
        }
    }
}