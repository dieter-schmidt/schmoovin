using System;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-recharginghealthmanager.html")]
    public class RechargingHealthManager : BasicHealthManager
    {
        [Header ("Health Regeneration")]

        [Tooltip("The recharge speed for health regeneration.")]
        [SerializeField] private float m_RechargeRate = 5f;
        [Tooltip("The delay between taking damage and starting health regen.")]
        [SerializeField] private float m_RechargeDelay = 5f;
        [Tooltip("Health recharge will be interrupted if damage greater than this amount is received.")]
        [Delayed, SerializeField] private float m_InterruptDamage = 1f;

        private static readonly NeoSerializationKey k_RechargeInterruptKey = new NeoSerializationKey("healthRegenInterrupt");

        private float m_InterruptTimer = 0f;

        protected override void OnValidate()
        {
            base.OnValidate();
            m_RechargeRate = Mathf.Clamp(m_RechargeRate, 0f, 1000f);
            m_RechargeDelay = Mathf.Clamp(m_RechargeDelay, 0f, 300f);
            m_InterruptDamage = Mathf.Clamp(m_InterruptDamage, 0f, healthMax - 1f);
        }

        protected override void OnHealthChanged(float from, float to, bool critical, IDamageSource source)
        {
            // Interrupt health regen
            if ((from - to) > m_InterruptDamage)
                m_InterruptTimer = m_RechargeDelay;

            base.OnHealthChanged(from, to, critical, source);
        }

        protected override void OnMaxHealthChanged(float from, float to)
        {
            // Interrupt health regen
            if ((to - from) > m_InterruptDamage)
                m_InterruptTimer = m_RechargeDelay;

            base.OnMaxHealthChanged(from, to);
        }

        protected virtual void FixedUpdate()
        {
            if (isAlive)
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
                    // Regen health if required
                    if (health < healthMax)
                        AddHealth(m_RechargeRate * Time.deltaTime);
                }
            }
        }

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);
            writer.WriteValue(k_RechargeInterruptKey, m_InterruptTimer);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);
            reader.TryReadValue(k_RechargeInterruptKey, out m_InterruptTimer, m_InterruptTimer);
        }
    }
}
