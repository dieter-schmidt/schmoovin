using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-ballisticprojectilewithparticles.html")]
	public class BallisticProjectileWithParticles : BallisticProjectile
    {
        [SerializeField, Tooltip("The particle systems to play")]
        private ParticleSystem[] m_ParticleSystems = { };

        [SerializeField, Tooltip("The distance from the firing point before the particle system is activated")]
        private float m_ParticleStartDistance = 2f;

        private bool m_Playing = false;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            m_ParticleStartDistance = Mathf.Clamp(m_ParticleStartDistance, 0f, 10f);
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            Stop();
        }

        void Play()
        {
            for (int i = 0; i < m_ParticleSystems.Length; ++i)
            {
                if (m_ParticleSystems[i] != null)
                    m_ParticleSystems[i].Play();
            }
        }

        void Stop()
        {
            for (int i = 0; i < m_ParticleSystems.Length; ++i)
            {
                if (m_ParticleSystems[i] != null)
                    m_ParticleSystems[i].Stop();
            }
        }

        public override void Fire(Vector3 position, Vector3 velocity, float gravity, IAmmoEffect effect, Transform ignoreRoot, LayerMask layers, IDamageSource damageSource = null, bool wait1 = false)
        {
            base.Fire(position, velocity, gravity, effect, ignoreRoot, layers, damageSource, wait1);

            if (m_ParticleStartDistance == 0f)
            {
                Play();
                m_Playing = true;
            }
            else
            {
                Stop();
                m_Playing = false;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!m_Playing)
            {
                if (distanceTravelled > m_ParticleStartDistance)
                {
                    Play();
                    m_Playing = true;
                }
            }
        }

        protected override void OnHit(RaycastHit hit)
        {
            base.OnHit(hit);
            Stop();
        }
    }
}