using System;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-shieldmanager.html")]
    public class ShieldManager : MonoBehaviour, IShieldManager, INeoSerializableComponent
    {
        [Header("Capacity")]

        [SerializeField, Tooltip("The starting shield amount.")]
        private float m_Shield = 100f;

        [SerializeField, Tooltip("The shield capacity of each shield step / block.")]
        private float m_StepCapacity = 100f;

        [SerializeField, Tooltip("The number of shield steps / blocks.")]
        private int m_StepCount = 1;

        [Header("Damage")]

        [SerializeField, Range(0f, 1f), Tooltip("The amount of damage (multiplier) that the shield negates.")]
        private float m_DamageMitigation = 1f;

        //[SerializeField, Tooltip("If false, only the current shield step can be consumed per damage event. If true, the damage will be taken from every shield step as required.")]
        //private bool m_DamageOverflow = true;

        [Header("Charging")]

        [SerializeField, Tooltip("The recharge speed for shield regeneration.")]
        private float m_ChargeRate = 5f;

        [SerializeField, Tooltip("The delay between taking damage and starting shield regen.")]
        private float m_ChargeDelay = 5f;

        [SerializeField, Tooltip("Shield steps only recharge if the shield value is greater than their starting level. If this property is false, step 1 will always recharge, even if it hits zero.")]
        private bool m_CanBreakStep1 = true;
        
        public event ShieldDelegates.OnShieldValueChanged onShieldValueChanged;
        public event ShieldDelegates.OnShieldStateChanged onShieldStateChanged;
        public event ShieldDelegates.OnShieldConfigChanged onShieldConfigChanged;

        private ShieldState m_ShieldState = ShieldState.Stable;
        private float m_InterruptTimer = 0f;
        private int m_CurrentStep = 0;

        public ShieldState shieldState
        {
            get { return m_ShieldState; }
            private set
            {
                if (m_ShieldState != value)
                {
                    m_ShieldState = value;
                    // Fire event
                    if (onShieldStateChanged != null)
                        onShieldStateChanged(this, m_ShieldState);
                }
            }
        }

        public float shield
        {
            get { return m_Shield; }
            set
            {
                // Clamp
                value = Mathf.Clamp(value, 0f, m_StepCount * m_StepCapacity);

                // Only assign if changed
                if (m_Shield != value)
                {
                    // Record old value
                    float old = m_Shield;
                    // Set new value
                    m_Shield = value;
                    // Interrupt recharge
                    CheckInterrupt(old, m_Shield);
                    // Fire event
                    if (onShieldValueChanged != null)
                        onShieldValueChanged(this, old, m_Shield);
                }
            }
        }

        public float shieldChargeRate
        {
            get { return m_ChargeRate; }
            set
            {
                // Clamp
                if (value < 0f)
                    value = 0f;

                // Only assign if changed
                if (m_ChargeRate != value)
                {
                    m_ChargeRate = value;

                    // Fire config change event
                    if (onShieldConfigChanged != null)
                        onShieldConfigChanged(this);

                    // Check if new rate froze recharge
                    if (m_ChargeRate == 0f && shieldState == ShieldState.Recharging)
                        shieldState = ShieldState.Interrupted;

                }
            }
        }

        public float shieldStepCapacity
        {
            get { return m_StepCapacity; }
            set
            {
                // Clamp
                if (value < 0f)
                    value = 0f;

                // Only assign if changed
                if (m_StepCapacity != value)
                {
                    // Record old value
                    float old = m_StepCapacity;
                    // Set new value
                    m_StepCapacity = value;
                    // Scale shield value to maintain relative
                    shield *= m_StepCapacity / old;
                    // Fire event
                    if (onShieldConfigChanged != null)
                        onShieldConfigChanged(this);
                }
            }
        }

        public int shieldStepCount
        {
            get { return m_StepCount; }
            set
            {
                // Clamp
                if (value < 1)
                    value = 1;
                if (m_StepCount != value)
                {
                    // Set new value
                    m_StepCount = value;
                    // Update shield
                    float max = m_StepCount * shieldStepCapacity;
                    if (shield > max)
                        shield = max;
                    // Fire event
                    if (onShieldConfigChanged != null)
                        onShieldConfigChanged(this);
                }
            }
        }

        void OnValidate()
        {
            m_Shield = Mathf.Clamp(m_Shield, 1f, 10000f);
            m_StepCapacity = Mathf.Clamp(m_StepCapacity, 1f, 10000f);
            m_StepCount = Mathf.Clamp(m_StepCount, 1, 99);
            m_ChargeRate = Mathf.Clamp(m_ChargeRate, 0f, 1000f);
            m_ChargeDelay = Mathf.Clamp(m_ChargeDelay, 0f, 300f);
        }

        void Start()
        {
            // Get the current step
            CheckCurrentStep();
        }

        void FixedUpdate()
        {
            // Check if interrupted
            if (m_InterruptTimer > 0f)
            {
                // Update interrupt timer
                m_InterruptTimer -= Time.deltaTime;
                if (m_InterruptTimer < 0f)
                    m_InterruptTimer = 0f;
            }
            else
            {
                if (m_ChargeRate > 0f)
                {
                    // Get recharge limit
                    float limit = m_CurrentStep * shieldStepCapacity;
                    if (!m_CanBreakStep1 && limit == 0f)
                        limit += shieldStepCapacity;

                    // Regen health if required
                    if (shield < limit)
                    {
                        float to = shield + m_ChargeRate * Time.deltaTime;
                        if (to > limit)
                            to = limit;
                        shield = to;
                    }
                }
            }
        }

        void CheckCurrentStep()
        {
            m_CurrentStep = Mathf.CeilToInt((shield - 0.01f) / shieldStepCapacity);
        }

        void CheckInterrupt(float from, float to)
        {
            if (from > to)
            {
                if (!Mathf.Approximately(GetPendingShield(), 0f))
                {
                    // Reset the timer
                    m_InterruptTimer = m_ChargeDelay;
                    // Set the shield state
                    shieldState = ShieldState.Interrupted;
                }
            }
            // Get the current step
            CheckCurrentStep();
        }


        float GetPendingShield()
        {
            int full = Mathf.FloorToInt((shield + 0.01f) / shieldStepCapacity);
            if (full < shieldStepCount)
            {
                float remainder = shield - (full * shieldStepCapacity);
                return shieldStepCapacity - remainder;
            }
            else
                return 0f; // None left to fill
        }

        public int FillShieldSteps(int count = 1)
        {
            // Check for invalid parameter
            if (count < 1)
                return 0;

            // Get the number of filled shield steps
            int full = Mathf.FloorToInt((shield + 0.01f) / shieldStepCapacity);
            if (full < shieldStepCount)
            {
                // Get the available step count
                int available = shieldStepCount - full;

                // Get the amount of steps to actually fill
                int toFill = Mathf.Min(available, count);

                // Fill the shield
                shield = (full + toFill) * shieldStepCapacity;
                shieldState = ShieldState.Stable;

                // Return filled (allows partial consume of pickups, etc)
                return toFill;
            }
            else
                return 0; // None left to fill
        }

        public int EmptyShieldSteps(int count = 1)
        {
            // Check for invalid parameter
            if (count < 1)
                return 0;

            // Get the number of filled shield steps
            int active = Mathf.CeilToInt((shield - 0.01f) / shieldStepCapacity);
            if (active > 0)
            {
                // Get the amount of steps to actually fill
                int toClear = Mathf.Min(active, count);

                // Reset the shield
                shield = (active - toClear) * shieldStepCapacity;
                shieldState = ShieldState.Stable;

                // Return cleared (allows partial consume of pickups, etc)
                return toClear;
            }
            else
                return 0; // Already empty
        }

        protected virtual float GetDamageMultiplier(DamageType t)
        {
            return 1f;
        }

        public float GetShieldedDamage(float damage, DamageType t)
        {
            // Do nothing to damage if shields are down
            if (shield == 0f || damage <= 0f || m_DamageMitigation == 0f)
                return damage;

            // Get shield damage multiplier
            float shieldDamageMultiplier = GetDamageMultiplier(t);
            if (shieldDamageMultiplier == 0f)
                return damage;

            // Get mitigated damage amount
            float mitigated = damage * m_DamageMitigation;

            // Remove damage overflow option, as having this false doesn't play nice with explosions or scatter weapons
            //if (m_DamageOverflow)
            //{
                // Scale shield damage
                mitigated *= shieldDamageMultiplier;

                // Clamp mitigated damage to shield
                if (mitigated > shield)
                    mitigated = shield;

                // Set new shield value
                shield -= mitigated;

                // Reverse damage scale (to calculate absorbed)
                mitigated /= shieldDamageMultiplier;
            //}
            //else
            //{
            //    // Get the number of filled shield steps & remainder
            //    int prevStep = Mathf.FloorToInt((shield + 0.01f) / shieldStepCapacity);
            //    float remainder = shield - (prevStep * shieldStepCapacity);
            //
            //    // If no remainder, can use full step
            //    if (Mathf.Approximately(remainder, 0f))
            //    {
            //        --prevStep;
            //        remainder = shieldStepCapacity;
            //    }
            //
            //    // Scale shield damage
            //    float scaled = mitigated * shieldDamageMultiplier;
            //
            //    // Clamp mitigated damage to shield
            //    if (scaled > remainder)
            //        scaled = remainder;
            //
            //    // Set new shield value
            //    Debug.Log("Removing shield: " + scaled);
            //    shield -= scaled;
            //}

            // Return unabsorbed damage
            return damage - mitigated;
        }

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_ShieldKey = new NeoSerializationKey("shield");
        private static readonly NeoSerializationKey k_StepCapacityKey = new NeoSerializationKey("stepCapacity");
        private static readonly NeoSerializationKey k_StepCountKey = new NeoSerializationKey("stepCount");
        private static readonly NeoSerializationKey k_RechargeRateKey = new NeoSerializationKey("rechargeRate");
        private static readonly NeoSerializationKey k_InterruptKey = new NeoSerializationKey("interrupt");
        private static readonly NeoSerializationKey k_DelayKey = new NeoSerializationKey("delay");
        private static readonly NeoSerializationKey k_StateKey = new NeoSerializationKey("state");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_ShieldKey, m_Shield);
            writer.WriteValue(k_StepCapacityKey, m_StepCapacity);
            writer.WriteValue(k_StepCountKey, m_StepCount);
            writer.WriteValue(k_RechargeRateKey, m_ChargeRate);
            writer.WriteValue(k_InterruptKey, m_InterruptTimer);
            writer.WriteValue(k_DelayKey, m_ChargeDelay);
            writer.WriteValue(k_StateKey, (int)m_ShieldState);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_StepCapacityKey, out m_StepCapacity, m_StepCapacity);
            reader.TryReadValue(k_StepCountKey, out m_StepCount, m_StepCount);
            reader.TryReadValue(k_RechargeRateKey, out m_ChargeRate, m_ChargeRate);
            reader.TryReadValue(k_InterruptKey, out m_InterruptTimer, m_InterruptTimer);
            reader.TryReadValue(k_DelayKey, out m_ChargeDelay, m_ChargeDelay);

            // Set shield value
            float floatValue;
            if (reader.TryReadValue(k_ShieldKey, out floatValue, m_Shield))
                shield = floatValue;

            // set state
            int intValue;
            if (reader.TryReadValue(k_StateKey, out intValue, (int)shieldState))
                shieldState = (ShieldState)intValue;

            // Get the current step
            CheckCurrentStep();
        }

        #endregion
    }
}
