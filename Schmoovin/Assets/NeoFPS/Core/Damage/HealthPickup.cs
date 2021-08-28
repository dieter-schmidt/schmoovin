using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-healthpickup.html")]
    [RequireComponent(typeof(AudioSource))]
    public class HealthPickup : MonoBehaviour, IPickup, INeoSerializableComponent
    {
        [SerializeField, Tooltip("How the heal is applied. Fixed value adds the health. Factor adds amount * max health. MissingFactor adds amount * missing health.")]
        private HealType m_HealType = HealType.FixedValue;

        [SerializeField, Tooltip("The amount to heal by. If heal type is Factor or MissingFactor, this will be a value between 0 and 1.")]
        private float m_HealAmount = 10f;

        [SerializeField, Tooltip("An event called when the pickup heals a character.")]
        private UnityEvent m_OnHealedCharacter = null;

        [SerializeField, Tooltip("If the character needs less healing than the pickup amount, should the pickup still be destroyed or should the remainder be available to use again?")]
        private bool m_SingleUse = true;

        [SerializeField, Tooltip("What to do to the pickup object once its item has been used (fully, or single use).")]
        private ConsumeResult m_ConsumeResult = ConsumeResult.Destroy;

        [SerializeField, Tooltip("How long to wait before respawning if the consume result is set to \"Respawn\".")]
        private float m_RespawnDuration = 20f;

        [SerializeField, Tooltip("The display mesh of the pickup. This should not be the same game object as this, so that if this is disabled the pickup will still respawn if required.")]
        private GameObject m_DisplayMesh = null;

        private static readonly NeoSerializationKey k_RespawnKey = new NeoSerializationKey("respawn");
        private static readonly NeoSerializationKey k_EnabledKey = new NeoSerializationKey("pickupEnabled");
        private static readonly NeoSerializationKey k_RemainingKey = new NeoSerializationKey("remaining");

        public enum HealType
        {
            FixedValue,
            Factor,
            MissingFactor
        }

        public enum ConsumeResult
        {
            Destroy,
            Disable,
            Respawn
        }

        private float m_Remaining = 0f;
        private AudioSource m_AudioSource = null;
        private Collider m_Collider = null;
        private IEnumerator m_DelayedSpawnCoroutine = null;
        private float m_RespawnTimer = 0f;

        private void OnValidate()
        {
            if (m_HealType == HealType.FixedValue)
                m_HealAmount = Mathf.Clamp(m_HealAmount, 0f, float.MaxValue);
            else
                m_HealAmount = Mathf.Clamp01(m_HealAmount);

            // Get the display mesh object
            if (m_DisplayMesh == null)
            {
                var mesh = GetComponentInChildren<MeshRenderer>(true);
                if (mesh != null && mesh.gameObject != gameObject)
                    m_DisplayMesh = mesh.gameObject;
            }
        }

        void Awake()
        {
            m_Collider = GetComponent<Collider>();
            m_AudioSource = GetComponent<AudioSource>();

            EnablePickup(true);
        }

        public void Trigger(ICharacter character)
        {
            // Do nothing if character has no health manager
            var healthMgr = character.GetComponent<IHealthManager>();
            if (healthMgr == null)
                return;

            // Do nothing if character is max health
            if (Mathf.Approximately(healthMgr.health, healthMgr.healthMax))
                return;

            m_OnHealedCharacter.Invoke();

            // Apply the heal
            switch (m_HealType)
            {
                case HealType.FixedValue:
                    {
                        if (m_SingleUse)
                        {
                            healthMgr.health += m_HealAmount;
                            DestroyPickup();
                        }
                        else
                        {
                            // Record the current health
                            float before = healthMgr.health;

                            // Apply the heal
                            healthMgr.health += m_Remaining;

                            // If only partially used, set new amount, else destroy
                            float remainder = m_Remaining - healthMgr.health + before;
                            if (remainder > 0.01f)
                                m_Remaining = remainder;
                            else
                                DestroyPickup();
                        }
                    }
                    break;
                case HealType.Factor:
                    healthMgr.health += m_HealAmount * healthMgr.healthMax;
                    DestroyPickup();
                    break;
                case HealType.MissingFactor:
                    healthMgr.health += m_HealAmount * (healthMgr.healthMax - healthMgr.health);
                    DestroyPickup();
                    break;
            }
        }

        void DestroyPickup ()
        {
            if (m_DelayedSpawnCoroutine != null)
                StopCoroutine(m_DelayedSpawnCoroutine);
            // NB: The item will have been moved into the inventory heirarchy
            switch (m_ConsumeResult)
            {
                case ConsumeResult.Destroy:
                    if (m_AudioSource.clip != null)
                        AudioSource.PlayClipAtPoint(m_AudioSource.clip, transform.position);
                    Destroy(gameObject);
                    break;
                case ConsumeResult.Disable:
                    m_AudioSource.Play();
                    EnablePickup(false);
                    break;
                case ConsumeResult.Respawn:
                    m_AudioSource.Play();
                    EnablePickup(false);
                    m_RespawnTimer = m_RespawnDuration;
                    m_DelayedSpawnCoroutine = DelayedSpawn();
                    StartCoroutine(m_DelayedSpawnCoroutine);
                    break;
            }
        }

        public virtual void EnablePickup(bool value)
        {
            // Enable the mesh
            if (m_DisplayMesh != null)
                m_DisplayMesh.SetActive(value);

            // Enable the collider
            m_Collider.enabled = value;

            // Reset the heal amount to full
            m_Remaining = m_HealAmount;
        }

        IEnumerator DelayedSpawn()
        {
            while (m_RespawnTimer > 0f)
            {
                yield return null;
                m_RespawnTimer -= Time.deltaTime;
            }
            EnablePickup(true);
            m_DelayedSpawnCoroutine = null;
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_DelayedSpawnCoroutine != null)
                writer.WriteValue(k_RespawnKey, m_RespawnTimer);

            if (!m_Collider.enabled)
                writer.WriteValue(k_EnabledKey, false);

            if (!Mathf.Approximately(m_Remaining, m_HealAmount))
                writer.WriteValue(k_RemainingKey, m_Remaining);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            // Enable the pickup
            bool pickupEnabled = true;
            reader.TryReadValue(k_EnabledKey, out pickupEnabled, true);
            EnablePickup(pickupEnabled);

            // Respawn timer (start the coroutine if the property is found)
            float respawn = 0f;
            if (reader.TryReadValue(k_RespawnKey, out respawn, 0f))
            {
                m_RespawnTimer = respawn;
                m_DelayedSpawnCoroutine = DelayedSpawn();
                StartCoroutine(m_DelayedSpawnCoroutine);
            }

            // Remaining heal
            reader.TryReadValue(k_RemainingKey, out m_Remaining, m_Remaining);
        }
    }
}