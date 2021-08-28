using System;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-basichealthmanager.html")]
    public class BasicHealthManager : MonoBehaviour, IHealthManager, INeoSerializableComponent
    {
        [Tooltip("The starting health of the character.")]
        [Delayed, SerializeField] private float m_Health = 100f;
        [Tooltip("The maximum health of the character.")]
        [Delayed, SerializeField] private float m_HealthMax = 100f;
        [Tooltip("Can the character damage itself (eg with explosives).")]
        [SerializeField] private bool m_CanDamageSelf = true;
        [Tooltip("An event called whenever the health changes")]
        [SerializeField] private FloatEvent m_OnHealthChanged = null;
        [Tooltip("An event called whenever the alive state of the health manager changes")]
        [SerializeField] private BoolEvent m_OnIsAliveChanged = null;

        private static readonly NeoSerializationKey k_HealthKey = new NeoSerializationKey("health");
        private static readonly NeoSerializationKey k_HealthMaxKey = new NeoSerializationKey("healthMax");
        private static readonly NeoSerializationKey k_IsAliveKey = new NeoSerializationKey("isAlive");

        public event HealthDelegates.OnIsAliveChanged onIsAliveChanged;
        public event HealthDelegates.OnHealthChanged onHealthChanged;
        public event HealthDelegates.OnHealthMaxChanged onHealthMaxChanged;

        private IController m_Controller = null;

        [Serializable]
        public class FloatEvent : UnityEvent<float>
        {
        }

        [Serializable]
        public class BoolEvent : UnityEvent<bool>
        {
        }

        private bool m_IsAlive = true;
        public bool isAlive
        {
            get { return m_IsAlive; }
            protected set
            {
                if (m_IsAlive != value)
                {
                    m_IsAlive = value;
                    OnIsAliveChanged();
                }
            }
        }

        public float health
        {
            get { return m_Health; }
            set { SetHealth(value, false, null); }
        }

        public float healthMax
        {
            get { return m_HealthMax; }
            set
            {
                if (m_HealthMax != value)
                {
                    float old = m_HealthMax;
                    // Set value
                    m_HealthMax = value;
                    // Check lower limit
                    if (m_HealthMax < 0f)
                        m_HealthMax = 0f;
                    // Fire event
                    OnMaxHealthChanged(old, m_HealthMax);
                    // Check health is still valid
                    if (health > m_HealthMax)
                        health = m_HealthMax;
                }
            }
        }
		
		public float normalisedHealth
		{
			get { return health / healthMax; }
			set { health = value * healthMax; }
		}

        protected virtual void OnValidate()
        {
            m_HealthMax = Mathf.Clamp(m_HealthMax, 1f, 10000f);
            m_Health = Mathf.Clamp(m_Health, 1f, m_HealthMax);
        }

        protected virtual void Awake()
        {
            var character = GetComponent<ICharacter>();
            if (character != null)
            {
                character.onControllerChanged += OnCharacterControllerChanged;
                OnCharacterControllerChanged(character, character.controller);
            }
        }

        protected virtual void OnDestroy()
        {
            var character = GetComponent<ICharacter>();
            if (character != null)
                character.onControllerChanged += OnCharacterControllerChanged;
        }

        void OnCharacterControllerChanged(ICharacter character, IController controller)
        {
            m_Controller = controller;
        }

        bool CheckDamageSource(IDamageSource source)
        {
            if (source == null)
                return true;

            // Check if damage source is self
            if (!m_CanDamageSelf && m_Controller != null && source.controller == m_Controller)
            {
                // Check against damage types
                if (source.outDamageFilter.IsDamageType(DamageType.Fall))
                    return true;
                if (source.outDamageFilter.IsDamageType(DamageType.Drowning))
                    return true;

                return false;
            }
            return true;
        }

        protected void SetHealth(float h, bool critical, IDamageSource source)
        {
            float old = m_Health;
            // Set value
            m_Health = Mathf.Clamp(h, 0f, m_HealthMax);
            // Check if changed
            if (m_Health != old)
            {
                OnHealthChanged(old, m_Health, critical, source);
                // Check if dead
                if (Mathf.Approximately(m_Health, 0f) && isAlive)
                    isAlive = false;
            }
        }

        protected virtual void OnHealthChanged(float from, float to, bool critical, IDamageSource source)
        {
            // Fire event
            if (onHealthChanged != null)
                onHealthChanged(from, to, critical, source);
            m_OnHealthChanged.Invoke(to);
        }

        protected virtual void OnMaxHealthChanged(float from, float to)
        {
            // Fire event
            if (onHealthMaxChanged != null)
                onHealthMaxChanged(from, to);
        }

        protected virtual void OnIsAliveChanged ()
        {
            if (onIsAliveChanged != null)
                onIsAliveChanged(m_IsAlive);
            m_OnIsAliveChanged.Invoke(m_IsAlive);
        }

        public virtual void AddDamage(float damage)
        {
            SetHealth(health - damage, false, null);
        }

        public virtual void AddDamage(float damage, bool critical)
        {
            SetHealth(health - damage, critical, null);
        }

        public virtual void AddDamage(float damage, IDamageSource source)
        {
            if (CheckDamageSource(source))
                SetHealth(health - damage, false, source);
        }

        public virtual void AddDamage(float damage, bool critical, IDamageSource source)
        {
            if (CheckDamageSource(source))
                SetHealth(health - damage, critical, source);
        }

        public void AddDamage(float damage, bool critical, RaycastHit hit)
        {
            SetHealth(health - damage, critical, null);
        }

        public void AddDamage(float damage, bool critical, IDamageSource source, RaycastHit hit)
        {
            if (CheckDamageSource(source))
                SetHealth(health - damage, critical, source);
        }

        public virtual void AddHealth(float h)
        {
            SetHealth(health + h, false, null);

            // Check if brought back to life
            if (!isAlive && health > 0f)
                isAlive = true;
        }

        public virtual void AddHealth(float h, IDamageSource source)
        {
            // Change health
            SetHealth(health + h, false, source);

            // Check if brought back to life
            if (!isAlive && health > 0f)
                isAlive = true;
        }

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_HealthMaxKey, healthMax);
            writer.WriteValue(k_HealthKey, health);
            writer.WriteValue(k_IsAliveKey, isAlive);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            float floatValue = 0f;
            if (reader.TryReadValue(k_HealthMaxKey, out floatValue, healthMax))
                healthMax = floatValue;
            if (reader.TryReadValue(k_HealthKey, out floatValue, health))
                health = floatValue;

            bool boolValue = true;
            if (reader.TryReadValue(k_IsAliveKey, out boolValue, boolValue))
                isAlive = boolValue;
        }
    }
}
