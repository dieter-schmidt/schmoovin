using System;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-simpleballisticshooter.html")]
    public class SimpleBallisticShooter : BaseShooterBehaviour
    {
        [Header("Shooter Settings")]

        [SerializeField, NeoPrefabField(typeof(IProjectile), required = true), Tooltip("The projectile to spawn.")]
        private PooledObject m_ProjectilePrefab = null;

        [SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The position and direction the projectile is spawned.")]
        private Transform m_MuzzleTip = null;

        [SerializeField, Tooltip("The speed of the projectile.")]
        private float m_MuzzleSpeed = 100f;

        [SerializeField, Tooltip("The layers bullets will collide with.")]
        private LayerMask m_Layers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Tooltip("The gravity for the projectile.")]

        private float m_Gravity = 9.8f;

        private ITargetingSystem m_TargetingSystem = null;

#if UNITY_EDITOR
        void OnValidate()
        {
            // Check shell prefab is valid
            if (m_ProjectilePrefab != null && m_ProjectilePrefab.GetComponent<IProjectile>() == null)
            {
                Debug.Log("Projectile prefab must have ArtilleryProjectile component attached: " + m_ProjectilePrefab.name);
                m_ProjectilePrefab = null;
            }

            if (m_MuzzleSpeed < 1f)
                m_MuzzleSpeed = 1f;
            if (m_Gravity < 0f)
                m_Gravity = 0f;
        }
#endif
        public LayerMask collisionLayers
        {
            get { return m_Layers; }
            set { m_Layers = value; }
        }

        public override bool isModuleValid
        {
            get
            {
                return
                    m_ProjectilePrefab != null &&
                    m_MuzzleTip != null &&
                    m_Layers != 0;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            m_TargetingSystem = GetComponentInChildren<ITargetingSystem>();
        }

        public override void Shoot(float accuracy, IAmmoEffect effect)
        {
            if (m_ProjectilePrefab != null)
            {
                IProjectile projectile = PoolManager.GetPooledObject<IProjectile>(m_ProjectilePrefab);
                InitialiseProjectile(projectile);

                Transform ignoreRoot = (firearm.wielder == null) ? null : firearm.wielder.gameObject.transform;
                projectile.Fire(m_MuzzleTip.position, m_MuzzleTip.forward * m_MuzzleSpeed, m_Gravity, effect, ignoreRoot, m_Layers, firearm as IDamageSource);
            }
            base.Shoot(accuracy, effect);
        }

        protected virtual void InitialiseProjectile(IProjectile projectile)
        {
            if (m_TargetingSystem != null)
            {
                var tracker = projectile.gameObject.GetComponent<ITargetTracker>();
                if (tracker != null)
                    m_TargetingSystem.RegisterTracker(tracker);
            }
        }

        private static readonly NeoSerializationKey k_LayersKey = new NeoSerializationKey("layers");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);
            writer.WriteValue(k_LayersKey, m_Layers);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);
            int layers = m_Layers;
            if (reader.TryReadValue(k_LayersKey, out layers, layers))
                collisionLayers = layers;

        }
    }
}