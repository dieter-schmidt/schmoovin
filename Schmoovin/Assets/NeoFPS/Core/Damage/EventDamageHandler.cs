using System;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-eventdamagehandler.html")]
    public class EventDamageHandler : MonoBehaviour, IDamageHandler
    {
		[SerializeField, Tooltip("The value to multiply any incoming damage by. Use to reduce damage to areas like feet, or raise it for areas like the head.")]
        private float m_Multiplier = 1f;

		[SerializeField, Tooltip("Does the damage count as critical. Used to change the feedback for the damage taker and dealer.")]
        private bool m_Critical = false;

		[SerializeField, Tooltip("An event that is invoked when damage is taken.")]
        private DamageEvent m_OnDamage = new DamageEvent();

        [Serializable]
        public class DamageEvent : UnityEvent<float, Vector3, bool>
        {
        }

        private Transform m_LocalTransform;
        private IHealthManager m_HealthManager;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_Multiplier < 0f)
                m_Multiplier = 0f;
        }
#endif
        void Awake()
        {
            m_LocalTransform = transform;
            m_HealthManager = GetComponentInParent<IHealthManager>();
        }

        #region IDamageHandler implementation

        private DamageFilter m_InDamageFilter = DamageFilter.AllDamageAllTeams;
        public DamageFilter inDamageFilter
        {
            get { return m_InDamageFilter; }
            set { m_InDamageFilter = value; }
        }

        public DamageResult AddDamage(float damage)
        {
            if (m_Multiplier > 0f)
            {
                damage *= m_Multiplier;

                if (m_HealthManager != null)
                    m_HealthManager.AddDamage(damage, m_Critical);

                // Invoke event
                m_OnDamage.Invoke(damage, Vector3.zero, m_Critical);

                return m_Critical ? DamageResult.Critical : DamageResult.Standard;
            }
            else
                return DamageResult.Ignored;
        }

        public DamageResult AddDamage(float damage, IDamageSource source)
        {
            // Apply damage
            if (m_Multiplier > 0f && CheckDamageCollision(source))
            {
                damage *= m_Multiplier;

                if (m_HealthManager != null)
                    m_HealthManager.AddDamage(damage, m_Critical);

                // Get hit direction
                Vector3 direction = Vector3.zero;
                if (source != null && source.damageSourceTransform != null)
                    direction = Vector3.Normalize(m_LocalTransform.position - source.damageSourceTransform.position);

                // Invoke event
                m_OnDamage.Invoke(damage, direction, m_Critical);
                
                // Report damage dealt
                if (damage > 0f && source != null && source.controller != null)
                    source.controller.currentCharacter.ReportTargetHit(m_Critical);

                return m_Critical ? DamageResult.Critical : DamageResult.Standard;
            }
            else
                return DamageResult.Ignored;
        }

        public DamageResult AddDamage(float damage, RaycastHit hit)
        {
            if (m_Multiplier > 0f)
            {
                damage *= m_Multiplier;

                if (m_HealthManager != null)
                    m_HealthManager.AddDamage(damage, m_Critical, hit);

                // Invoke event
                m_OnDamage.Invoke(damage, Vector3.zero, m_Critical);

                return m_Critical ? DamageResult.Critical : DamageResult.Standard;
            }
            else
                return DamageResult.Ignored;
        }

        public DamageResult AddDamage(float damage, RaycastHit hit, IDamageSource source)
        {
            // Apply damage
            if (m_Multiplier > 0f && CheckDamageCollision(source))
            {
                damage *= m_Multiplier;

                if (m_HealthManager != null)
                    m_HealthManager.AddDamage(damage, m_Critical, hit);

                // Get hit direction
                Vector3 direction = Vector3.zero;
                if (source != null && source.damageSourceTransform != null)
                    direction = Vector3.Normalize(m_LocalTransform.position - source.damageSourceTransform.position);

                // Invoke event
                m_OnDamage.Invoke(damage, direction, m_Critical);

                // Report damage dealt
                if (damage > 0f && source != null && source.controller != null)
                    source.controller.currentCharacter.ReportTargetHit(m_Critical);

                return m_Critical ? DamageResult.Critical : DamageResult.Standard;
            }
            else
                return DamageResult.Ignored;
        }

        bool CheckDamageCollision(IDamageSource source)
        {
            return !(source != null && !source.outDamageFilter.CollidesWith(inDamageFilter, FpsGameMode.friendlyFire));
        }

        #endregion
    }
}