using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-advancedparticlemuzzleeffect.html")]
    public class AdvancedParticleMuzzleEffect : BaseMuzzleEffectBehaviour
    {
        [Header("Muzzle Effect Settings")]

        [SerializeField, NeoObjectInHierarchyField(false, required = true), Tooltip("The effect transform will be reparented under the character so it persists between weapon switches and handles character movement better.")]
        private Transform m_EffectTransform = null;

        [SerializeField, Tooltip("The amount of time after a shot, that the particle effect transform will sync with the weapon. If you have particle systems that emit over time then ensure this duration is long enough to cover that.")]
        private float m_FollowDuration = 0f;

        [SerializeField, Tooltip("The particle systems to play.")]
        private ParticlesInfo[] m_ParticleSystems = { };

        [SerializeField, Tooltip("The audio clips to use when firing. Chosen at random.")]
        private AudioClip[] m_FiringSounds = null;

        [SerializeField, Range(0f, 1f), Tooltip("The volume that firing sounds are played at.")]
        private float m_ShotVolume = 1f;

        private Vector3 m_PositionOffset = Vector3.zero;
        private Quaternion m_RotationOffset = Quaternion.identity;
        private Transform m_WeaponTransform = null;
        private float m_FollowTimeout = 0f;

        public enum SimulationSpace
        {
            World,
            Weapon,
            Character,
            NoChange
        }

        [Serializable]
        public struct ParticlesInfo
        {
#pragma warning disable 0649

            public ParticleSystem particleSystem;
            public SimulationSpace space;

#pragma warning restore 0649
        }

        public override bool isModuleValid
        {
            get { return m_EffectTransform != null; }
        }

        void OnValidate()
        {
            m_FollowDuration = Mathf.Clamp(m_FollowDuration, 0f, 10f);
            if (m_EffectTransform != null && (m_EffectTransform == transform || !m_EffectTransform.IsChildOf(transform)))
            {
                Debug.LogError("Effect transform must be a child of the AdvancedParticleMuzzleEffect object.");
                m_EffectTransform = null;
            }
        }

        protected override void Awake()
        {
            base.Awake();
                        
            firearm.onWielderChanged += OnWielderChanged;
            OnWielderChanged(firearm.wielder);
        }

        private void OnWielderChanged(ICharacter c)
        {
            if (c != null && m_WeaponTransform == null)
            {
                // Get the weapon transform
                m_WeaponTransform = m_EffectTransform.parent;

                // Get the container transform
                var inventoryRoot = firearm.transform.parent;
                var container = inventoryRoot.Find("ParticleSystems");
                if (container == null)
                {
                    var go = new GameObject("ParticleSystems");
                    container = go.transform;
                    container.SetParent(inventoryRoot);
                    container.localScale = Vector3.one;
                    container.localPosition = Vector3.zero;
                    container.localRotation = Quaternion.identity;
                }

                // Move the effect into the container
                m_EffectTransform.SetParent(container, true);

                // Get the offset from the weapon transform
                m_PositionOffset = m_WeaponTransform.InverseTransformPoint(m_EffectTransform.position);
                m_RotationOffset = Quaternion.Inverse(m_WeaponTransform.rotation) * m_EffectTransform.rotation;

                // Set simulation spaces
                for (int i = 0; i < m_ParticleSystems.Length; ++i)
                {
                    if (m_ParticleSystems[i].particleSystem != null && m_ParticleSystems[i].space != SimulationSpace.NoChange)
                    {
                        var main = m_ParticleSystems[i].particleSystem.main;
                        switch (m_ParticleSystems[i].space)
                        {
                            case SimulationSpace.World:
                                main.simulationSpace = ParticleSystemSimulationSpace.World;
                                break;
                            case SimulationSpace.Character:
                                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                                break;
                            case SimulationSpace.Weapon:
                                main.simulationSpace = ParticleSystemSimulationSpace.Custom;
                                main.customSimulationSpace = m_WeaponTransform;
                                break;
                        }
                    }
                }

                m_EffectTransform.gameObject.SetActive(true);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            //m_EffectTransform.gameObject.SetActive(true);
        }

        void OnDisable()
        {
            StopContinuous();
            //m_EffectTransform.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            // Destroy if the firearm is being destroyed (this might need a better solution longer term)
            if (m_WeaponTransform != null)
                Destroy(m_EffectTransform.gameObject);
        }

        void Update()
        {
            if (m_FollowTimeout > 0f)
            {
                // Position the effects
                m_EffectTransform.position = m_WeaponTransform.TransformPoint(m_PositionOffset);
                m_EffectTransform.rotation = m_RotationOffset * m_WeaponTransform.rotation;

                // Decrement the timer
                m_FollowTimeout -= Time.deltaTime;
                if (m_FollowTimeout < 0f)
                    m_FollowTimeout = 0f;
            }
        }

        public override void Fire()
        {
            m_FollowTimeout = m_FollowDuration;

            // Position the effects
            m_EffectTransform.position = m_WeaponTransform.TransformPoint(m_PositionOffset);
            m_EffectTransform.rotation = m_RotationOffset * m_WeaponTransform.rotation;

            // Play particle systems
            for (int i = 0; i < m_ParticleSystems.Length; ++i)
            {
                if (m_ParticleSystems[i].particleSystem != null)
                    m_ParticleSystems[i].particleSystem.Play(true);
            }

            // Play audio
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
            // Play particle systems
            for (int i = 0; i < m_ParticleSystems.Length; ++i)
                if (m_ParticleSystems[i].particleSystem != null)
                    m_ParticleSystems[i].particleSystem.Play(true);
        }

        public override void StopContinuous()
        {
            // Play particle systems
            for (int i = 0; i < m_ParticleSystems.Length; ++i)
                if (m_ParticleSystems[i].particleSystem != null)
                    m_ParticleSystems[i].particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}