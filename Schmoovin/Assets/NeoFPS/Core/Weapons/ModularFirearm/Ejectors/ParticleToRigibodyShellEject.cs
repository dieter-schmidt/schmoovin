using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-particletorigibodyshelleject.html")]
    public class ParticleToRigibodyShellEject : BaseEjectorBehaviour
    {
        [Header("Ejector Settings")]

        [SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The particle system to play when a shell is ejected.")]
        private ParticleSystem m_ParticleSystem = null;

        [SerializeField, NeoPrefabField(typeof(Rigidbody), required = true), Tooltip("The pooled rigidbody to swap the particles for once out of view.")]
        private PooledObject m_RigidbodyPrefab = null;

        [SerializeField, Tooltip("The maximum number of particles that can be visible.")]
        private int m_MaxParticles = 64;

        [SerializeField, Tooltip("The delay type between firing and ejecting a shell.")]
        private FirearmDelayType m_DelayType = FirearmDelayType.None;

        [SerializeField, Tooltip("The delay time between firing and ejecting a shell if the delay type is set to elapsed time.")]
        private float m_Delay = 0f;

        public float lifeRemain = 0.2f;

        public override bool ejectOnFire { get { return m_DelayType != FirearmDelayType.ExternalTrigger; } }

        private Particle[] m_Particles = null;

        private int m_ParticleCount = 0;

        private Queue<float> m_Pending = null;
        private float m_CurrentTime = 0f;

#if UNITY_EDITOR
        void OnValidate()
        {
            m_Delay = Mathf.Clamp(m_Delay, 0f, 5f);
        }
#endif

        public override bool isModuleValid
        {
            get { return m_ParticleSystem != null && m_RigidbodyPrefab != null; }
        }

        protected override void Awake()
        {
            base.Awake();

            if (m_DelayType == FirearmDelayType.ElapsedTime)
                m_Pending = new Queue<float>();

            m_Particles = new Particle[m_MaxParticles];
        }

        public override void Eject()
        {
            if (m_DelayType == FirearmDelayType.ElapsedTime && m_Delay > 0f)
                m_Pending.Enqueue(m_CurrentTime + m_Delay);
            else
                OnEject();
        }

        protected virtual void OnEject()
        {
            if (m_ParticleCount < m_MaxParticles)
            {
                m_ParticleSystem.Play(true);
                ++m_ParticleCount;
            }
        }

        private void FixedUpdate()
        {
            m_CurrentTime += Time.deltaTime;

            m_ParticleCount = m_MaxParticles;

            if (m_ParticleCount > 0)
            {
                var t = m_ParticleSystem.transform;

                int culled = 0;
                m_ParticleCount = m_ParticleSystem.GetParticles(m_Particles, m_ParticleCount);
                for (int i = 0; i < m_ParticleCount; ++i)
                {
                    // Check if out of view & swap
                    if (m_Particles[i].remainingLifetime < lifeRemain)
                    {
                        m_Particles[i].remainingLifetime = -1f;

                        var pos = t.position;
                        var rot = t.rotation;

                        var rb = PoolManager.GetPooledObject<Rigidbody>(m_RigidbodyPrefab, pos + rot * m_Particles[i].position, rot * Quaternion.Euler(m_Particles[i].rotation3D));
                        rb.angularVelocity = m_Particles[i].angularVelocity3D;
                        rb.velocity = rot *  m_Particles[i].velocity;

                        ++culled;
                    }
                }

                m_ParticleSystem.SetParticles(m_Particles, m_ParticleCount);
                m_ParticleCount -= culled;
            }

            if (m_Pending != null && m_Pending.Count > 0 && m_Pending.Peek() < m_CurrentTime)
            {
                m_Pending.Dequeue();
                OnEject();
            }
        }
    }
}