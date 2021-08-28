using UnityEngine;
using UnityEngine.Serialization;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using System.Collections;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-patternhitscanshooter.html")]
    public class PatternHitscanShooter : BaseShooterBehaviour, IUseCameraAim
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

        [Header("Raycast")]

        [SerializeField, Tooltip("The maximum distance that the weapon will register a hit.")]
        private float m_MaxDistance = 1000f;

        [SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The transform that the bullet actually fires from")]
        private Transform m_MuzzleTip = null;

        [SerializeField, Tooltip("The layers bullets will collide with.")]
        private LayerMask m_Layers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Tooltip("Should the shot be tested against trigger colliders.")]
        private bool m_QueryTriggerColliders = false;

        [SerializeField, Tooltip("The minimum angle from forward (in degrees) of the shot (at full accuracy)")]
        private float m_MinAimOffset = 0f;

        [SerializeField, Tooltip("The maximum angle from forward (in degrees) of the shot (at zero accuracy)")]
        private float m_MaxAimOffset = 10f;

        [FormerlySerializedAs("m_UseCameraForward")] // Remove this
        [SerializeField, Tooltip("When set to use camera aim, the gun first casts from the FirstPersonCamera's aim transform, and then from the muzzle tip to that point to get more accurate firing.")]
        private UseCameraAim m_UseCameraAim = UseCameraAim.HipFireOnly;

        [Header("Tracer")]

        [SerializeField, NeoPrefabField(typeof(IPooledHitscanTrail)), Tooltip("The optional pooled tracer prototype to use (must implement the IPooledHitscanTrail interface)")]
        private PooledObject m_TracerPrototype = null;

        [SerializeField, Tooltip("How size (thickness/radius) of the tracer line")]
        private float m_TracerSize = 0.01f;

        [SerializeField, Tooltip("How long the tracer line will last")]
        private float m_TracerDuration = 0.05f;

        private Vector3[] m_PatternVectors = null;
        private Vector3[] m_HitPoints = null;
        private RaycastHit m_Hit = new RaycastHit();
        private WaitForEndOfFrame m_WaitForEndOfFrame = new WaitForEndOfFrame();

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_MaxDistance < 0.5f)
                m_MaxDistance = 0.5f;
            if (m_PatternDistance < 1)
                m_PatternDistance = 1;
            if (m_TracerDuration < 0f)
                m_TracerDuration = 0f;
            m_TracerSize = Mathf.Clamp(m_TracerSize, 0.001f, 0.25f);
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
            get { return m_MuzzleTip != null && m_Layers != 0; }
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

            if (m_TracerPrototype != null)
                m_HitPoints = new Vector3[m_PatternPoints.Length];
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
            // Just return if there is no effect
            if (effect == null)
                return;

            // Get root game object to prevent impacts with body
            Transform ignoreRoot = null;
            if (firearm.wielder != null)
                ignoreRoot = firearm.wielder.gameObject.transform;

            // Get the forward vector
            Vector3 muzzlePosition = m_MuzzleTip.position;
            Vector3 startPosition = muzzlePosition;
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
            float spread = Mathf.Lerp(m_MinAimOffset, m_MaxAimOffset, 1f - accuracy);
            if (spread > Mathf.Epsilon)
            {
                Quaternion randomRot = UnityEngine.Random.rotationUniform;
                aimRotation = Quaternion.Slerp(Quaternion.identity, randomRot, spread / 360f) * aimRotation;
            }

            // Get the damage source
            IDamageSource damageSource = firearm as IDamageSource;

            // Fire individual shots
            var queryTriggers = m_QueryTriggerColliders ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
            for (int i = 0; i < m_PatternVectors.Length; ++i)
            {
                // Get the shot direction
                Vector3 shotDirection = aimRotation * m_PatternVectors[i];

                // Check for raycast hit
                Ray ray = new Ray(startPosition, shotDirection);
                Vector3 hitPoint;
                bool didHit = PhysicsExtensions.RaycastNonAllocSingle(ray, out m_Hit, m_MaxDistance, m_Layers, ignoreRoot, queryTriggers);
                if (didHit)
                    hitPoint = m_Hit.point;
                else
                    hitPoint = startPosition + (shotDirection * m_MaxDistance);

                // Double check hit from gun muzzle to prevent near scenery weirdness
                if (useCamera)
                {
                    Vector3 newRayDirection = hitPoint - muzzlePosition;
                    newRayDirection.Normalize();
                    ray = new Ray(muzzlePosition, newRayDirection);
                    if (PhysicsExtensions.RaycastNonAllocSingle(ray, out m_Hit, m_MaxDistance, m_Layers, ignoreRoot, queryTriggers))
                    {
                        hitPoint = m_Hit.point;
                        effect.Hit(m_Hit, newRayDirection, m_Hit.distance, float.PositiveInfinity, damageSource);
                    }
                }
                else
                {
                    if (didHit)
                        effect.Hit(m_Hit, shotDirection, m_Hit.distance, float.PositiveInfinity, damageSource);
                }

                // Add a tracer hit point
                if (m_TracerPrototype != null)
                    m_HitPoints[i] = hitPoint;
            }

            // Show the tracers
            if (m_HitPoints != null)
                StartCoroutine(ShowTracers());

            base.Shoot(accuracy, effect);
        }

        IEnumerator ShowTracers()
        {
            yield return m_WaitForEndOfFrame;
            Vector3 muzzlePosition = m_MuzzleTip.position;
            for (int i = 0; i < m_HitPoints.Length; ++i)
            {
                var tracer = PoolManager.GetPooledObject<IPooledHitscanTrail>(m_TracerPrototype);
                tracer.Show(muzzlePosition, m_HitPoints[i], m_TracerSize, m_TracerDuration);
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