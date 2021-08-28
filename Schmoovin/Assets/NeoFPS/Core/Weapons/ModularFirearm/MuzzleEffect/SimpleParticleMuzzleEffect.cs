using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-simpleparticlemuzzleeffect.html")]
    public class SimpleParticleMuzzleEffect : BaseMuzzleEffectBehaviour
    {
        [Header("Muzzle Effect Settings")]

        [SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The particle system to play.")]
        private ParticleSystem m_ParticleSystem = null;

        [SerializeField, Tooltip("The audio clips to use when firing. Chosen at random.")]
        private AudioClip[] m_FiringSounds = null;

        [SerializeField, Range(0f, 1f), Tooltip("The volume that firing sounds are played at.")]
        private float m_ShotVolume = 1f;

        public override bool isModuleValid
        {
            get { return m_ParticleSystem != null; }
        }

        void OnDisable()
        {
            StopContinuous();
        }

        public override void Fire()
        {
            if (m_ParticleSystem != null)
                m_ParticleSystem.Play(true);

            switch (m_FiringSounds.Length)
            {
                case 0:
                    return;
                case 1:
                    firearm.PlaySound(m_FiringSounds[0], m_ShotVolume);
                    return;
                default:
                    firearm.PlaySound(m_FiringSounds[UnityEngine.Random.Range(0, m_FiringSounds.Length)], m_ShotVolume);
                    return;
            }
        }

        public override void FireContinuous()
        {
            if (m_ParticleSystem != null)
                m_ParticleSystem.Play(true);
        }

        public override void StopContinuous()
        {
            if (m_ParticleSystem != null)
                m_ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}