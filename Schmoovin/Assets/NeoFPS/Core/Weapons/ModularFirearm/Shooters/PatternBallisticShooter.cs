using UnityEngine;
using UnityEngine.Serialization;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-patternballisticshooter.html")]
    public class PatternBallisticShooter : BaseShooterBehaviour, IUseCameraAim
    {
        [Header("Shooter Settings")]

        [SerializeField, Tooltip("The different points of the pattern as looking straight at the target. Clamped to the -1m to 1m range on both axes")]
        private Vector2[] m_PatternPoints = {
            new Vector2(0f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(-0.5f, 0f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, -0.5f)
        };

        [SerializeField, Tooltip("The distance from the muzzle tip at which the pattern will match the diagram.")]
        private float m_PatternDistance = 50f;

        [SerializeField, NeoPrefabField(typeof(IProjectile), required = true), Tooltip("The projectile to spawn.")]
        private PooledObject m_ProjectilePrefab = null;

        [SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The transform that the bullet actually fires from")]
        private Transform m_MuzzleTip = null;

        [SerializeField, Tooltip("The speed of the projectile.")]
        private float m_MuzzleSpeed = 100f;

        [SerializeField, Tooltip("The gravity for the projectile.")]
        private float m_Gravity = 9.8f;

        [SerializeField, Tooltip("The layers bullets will collide with.")]
        private LayerMask m_Layers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Tooltip("The minimum angle from forward (in degrees) of the shot (at full accuracy)")]
        private float m_MinAimOffset = 0f;

        [SerializeField, Tooltip("The maximum angle from forward (in degrees) of the shot (at zero accuracy)")]
        private float m_MaxAimOffset = 10f;

        [FormerlySerializedAs("m_UseCameraForward")] // Remove this
        [SerializeField, Tooltip("When set to use camera aim, the gun first casts from the FirstPersonCamera's aim transform, and then from the muzzle tip to that point to get more accurate firing.")]
        private UseCameraAim m_UseCameraAim = UseCameraAim.HipFireOnly;

        private ITargetingSystem m_TargetingSystem = null;
        private Vector3[] m_PatternVectors = null;

#if UNITY_EDITOR
        void OnValidate()
        {
            // Check shell prefab is valid
            if (m_ProjectilePrefab != null && m_ProjectilePrefab.GetComponent<IProjectile>() == null)
            {
                Debug.Log("Projectile prefab must have BallisticProjectile component attached: " + m_ProjectilePrefab.name);
                m_ProjectilePrefab = null;
            }

            if (m_MuzzleSpeed < 1f)
                m_MuzzleSpeed = 1f;
            if (m_PatternDistance < 1)
                m_PatternDistance = 1;
            if (m_Gravity < 0f)
                m_Gravity = 0f;
            m_MinAimOffset = Mathf.Clamp(m_MinAimOffset, 0f, 45f);
            m_MaxAimOffset = Mathf.Clamp(m_MaxAimOffset, 0f, 45f);

            // Clamp points
            for (int i = 0; i < m_PatternPoints.Length; ++i)
            {
                m_PatternPoints[i].x = Mathf.Clamp(m_PatternPoints[i].x, -1f, 1f);
                m_PatternPoints[i].y = Mathf.Clamp(m_PatternPoints[i].y, -1f, 1f);
            }
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

        public UseCameraAim useCameraAim
        {
            get { return m_UseCameraAim; }
            set { m_UseCameraAim = value; }
        }

        protected override void Awake()
        {
            base.Awake();
            ResetPatternRotations();
            m_TargetingSystem = GetComponentInChildren<ITargetingSystem>();
        }

        void ResetPatternRotations()
        {
            if (m_PatternVectors == null)
                m_PatternVectors = new Vector3[m_PatternPoints.Length];

            for (int i = 0; i < m_PatternVectors.Length; ++i)
            {
                m_PatternVectors[i] = new Vector3(m_PatternPoints[i].x, m_PatternPoints[i].y, m_PatternDistance);
                m_PatternVectors[i].Normalize();
            }
        }

        public override void Shoot(float accuracy, IAmmoEffect effect)
        {
            // Just return if there is no effect or projectile set
            if (effect == null || m_ProjectilePrefab == null)
                return;

            // Get root game object to prevent impacts with body
            Transform ignoreRoot = null;
            if (firearm.wielder != null)
                ignoreRoot = firearm.wielder.gameObject.transform;

            // Get the forward vector
            Vector3 m_MuzzlePosition = m_MuzzleTip.position;
            Vector3 startPosition = m_MuzzlePosition;
            Quaternion aimRotation = m_MuzzleTip.rotation;

            bool useCamera = false;
            if (firearm.wielder != null)
            {
                switch (m_UseCameraAim)
                {
                    case UseCameraAim.HipAndAimDownSights:
                        useCamera = true;
                        break;
                    case UseCameraAim.AimDownSightsOnly:
                        if (firearm.aimer != null)
                            useCamera = firearm.aimer.isAiming;
                        break;
                    case UseCameraAim.HipFireOnly:
                        if (firearm.aimer != null)
                            useCamera = !firearm.aimer.isAiming;
                        else
                            useCamera = true;
                        break;
                }
            }
            if (useCamera)
            {
                Transform aimTransform = firearm.wielder.fpCamera.aimTransform;
                startPosition = aimTransform.position;
                aimRotation = aimTransform.rotation;
            }

            // Get the direction (with accuracy offset)
            float accuracyAdjusted = Mathf.Lerp(m_MinAimOffset, m_MaxAimOffset, 1f - accuracy);
            if (accuracyAdjusted > Mathf.Epsilon)
            {
                Quaternion randomRot = Random.rotationUniform;
                aimRotation = Quaternion.Slerp(Quaternion.identity, randomRot, accuracyAdjusted / 360f) * aimRotation;
            }

            // Get the damage source
            IDamageSource damageSource = firearm as IDamageSource;

            // Fire individual shots
            for (int i = 0; i < m_PatternVectors.Length; ++i)
            {
                IProjectile projectile = PoolManager.GetPooledObject<IProjectile>(m_ProjectilePrefab, false);
                InitialiseProjectile(projectile);

                // Get the direction
                Vector3 randomDir = Random.onUnitSphere;
                Vector3 shotDirection = aimRotation * m_PatternVectors[i];

                projectile.Fire(m_MuzzleTip.position, shotDirection * m_MuzzleSpeed, m_Gravity, effect, ignoreRoot, m_Layers, damageSource);
                projectile.gameObject.SetActive(true);
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