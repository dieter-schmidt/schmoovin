using UnityEngine;
using UnityEngine.Serialization;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-spreadballisticshooter.html")]
	public class SpreadBallisticShooter : BaseShooterBehaviour, IUseCameraAim
    {
        [Header("Shooter Settings")]

        [SerializeField, NeoPrefabField(typeof(IProjectile), required = true), Tooltip("The projectile to spawn.")]
        private PooledObject m_ProjectilePrefab = null;

        [SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The transform that the bullet actually fires from")]
		private Transform m_MuzzleTip = null;

        [SerializeField, FormerlySerializedAs("m_MuzzleSpeed"), Tooltip("The maximum speed of each projectile.")]
        private float m_MaxMuzzleSpeed = 100f;

        [SerializeField, FormerlySerializedAs("m_MuzzleSpeed"), Tooltip("The minimum speed of each projectile.")]
        private float m_MinMuzzleSpeed = 100f;

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

        [SerializeField, Tooltip("How many pellets are fired each shot")]
		private int m_BulletCount = 8;

        [SerializeField, Tooltip("The spread of the cone in degrees")]
		private float m_Cone = 15f;

        private ITargetingSystem m_TargetingSystem = null;

#if UNITY_EDITOR
        void OnValidate()
        {
            // Check shell prefab is valid
            if (m_ProjectilePrefab != null && m_ProjectilePrefab.GetComponent<IProjectile>() == null)
            {
                Debug.Log("Projectile prefab must have BallisticProjectile component attached: " + m_ProjectilePrefab.name);
                m_ProjectilePrefab = null;
            }

            if (m_BulletCount < 2)
                m_BulletCount = 2;
            if (m_MaxMuzzleSpeed < 1f)
                m_MaxMuzzleSpeed = 1f;
            if (m_MinMuzzleSpeed < 1f)
                m_MinMuzzleSpeed = 1f;
            if (m_Gravity < 0f)
                m_Gravity = 0f;
            m_Cone = Mathf.Clamp(m_Cone, 0.1f, 90f);
            m_MinAimOffset = Mathf.Clamp(m_MinAimOffset, 0f, 45f);
            m_MaxAimOffset = Mathf.Clamp(m_MaxAimOffset, 0f, 45f);
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
            m_TargetingSystem = GetComponentInChildren<ITargetingSystem>();
        }

        public override void Shoot (float accuracy, IAmmoEffect effect)
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
            Vector3 forwardVector = m_MuzzleTip.forward;

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
                forwardVector = aimTransform.forward;
            }

            // Get the direction (with accuracy offset)
            float accuracyAdjusted = Mathf.Lerp(m_MinAimOffset, m_MaxAimOffset, 1f - accuracy);
            if (accuracyAdjusted > Mathf.Epsilon)
            {
                Quaternion randomRot = Random.rotationUniform;
                forwardVector = Quaternion.Slerp(Quaternion.identity, randomRot, accuracyAdjusted / 360f) * forwardVector;
            }

            // Get the damage source
            IDamageSource damageSource = firearm as IDamageSource;

            // Fire individual shots
            bool randomiseSpeed = !Mathf.Approximately(m_MinMuzzleSpeed, m_MaxMuzzleSpeed);
            for (int i = 0; i < m_BulletCount; ++i)
            {
                IProjectile projectile = PoolManager.GetPooledObject<IProjectile>(m_ProjectilePrefab, false);
                InitialiseProjectile(projectile);

                // Get the direction
                Vector3 randomDir = Random.onUnitSphere;
				Vector3 shotDirection = Vector3.Slerp (forwardVector, randomDir, m_Cone / 360f);

                // Get the muzzle speed (randomised)
                float muzzleSpeed = randomiseSpeed ? Random.Range(m_MinMuzzleSpeed, m_MaxMuzzleSpeed) : m_MaxMuzzleSpeed;

                // Fire the projectile
                projectile.Fire(m_MuzzleTip.position, shotDirection * muzzleSpeed, m_Gravity, effect, ignoreRoot, m_Layers, damageSource);
                projectile.gameObject.SetActive(true);
            }

			base.Shoot (accuracy, effect);
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