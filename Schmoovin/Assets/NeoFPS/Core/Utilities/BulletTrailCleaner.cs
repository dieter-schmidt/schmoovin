using NeoFPS.ModularFirearms;
using System.Collections;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(TrailRenderer))]
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-bullettrailcleaner.html")]
    public class BulletTrailCleaner : MonoBehaviour
    {
        [SerializeField, Tooltip("The delay before enabling the trail render")]
        private float m_EmitDelay = 0f;

        private TrailRenderer m_TrailRenderer = null;
        private State m_State = State.Start;
        private float m_Timer = 0;

        private enum State
        {
            Start,
            Flying,
            Impacted,
            Disabled
        }

        private void Awake()
        {
            m_TrailRenderer = GetComponent<TrailRenderer>();
            var projectile = GetComponentInParent<IProjectile>();
            if (projectile != null)
            {
                projectile.onTeleported += OnTeleported;
                projectile.onHit += OnHit;
            }
        }

        void OnEnable()
        {
            m_TrailRenderer.emitting = false;
            m_TrailRenderer.Clear();
            m_Timer = 0f;
            m_State = State.Start;
        }

        void OnTeleported()
        {
            m_TrailRenderer.Clear();
        }

        void OnHit()
        {
            m_State = State.Impacted;
            m_Timer = m_TrailRenderer.time;
        }

        void Update()
        {
            if (m_State == State.Start)
            {
                if (m_Timer > m_EmitDelay)
                {
                    m_TrailRenderer.emitting = true;
                    m_State = State.Flying;
                }
                m_Timer += Time.deltaTime;
            }

            if (m_State == State.Impacted)
            {
                m_Timer -= Time.deltaTime;
                if (m_Timer <= 0f)
                {
                    m_TrailRenderer.emitting = false;
                    m_TrailRenderer.Clear();
                    m_State = State.Disabled;
                }
            }
        }
    }
}
