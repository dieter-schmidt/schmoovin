using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;
using NeoCC;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [RequireComponent(typeof(MotionController))]
    public abstract class BaseCharacter : MonoBehaviour, ICharacter, INeoSerializableComponent
    {
        private IHealthManager m_HealthManager = null;
        public IHealthManager healthManager
        {
            get { return m_HealthManager; }
            private set
            {
                if (m_HealthManager != null)
                {
                    m_HealthManager.onIsAliveChanged -= OnIsAliveChanged;
                    m_HealthManager.onHealthChanged -= OnHealthChanged;
                }
                m_HealthManager = value;
                if (m_HealthManager != null)
                {
                    m_HealthManager.onIsAliveChanged += OnIsAliveChanged;
                    m_HealthManager.onHealthChanged += OnHealthChanged;
                }
            }
        }

        public Transform localTransform
        {
            get;
            private set;
        }

        protected virtual void OnValidate()
        {
            ValidateImpactDamage();
        }

        protected virtual void Awake()
        {
            localTransform = transform;
            m_MotionController = GetComponent<MotionController>();
            aimController = GetComponent<IAimController>();
            audioHandler = GetComponent<ICharacterAudioHandler>();
            healthManager = GetComponent<IHealthManager>();
            inventory = GetComponent<IInventory>();
            quickSlots = GetComponent<IQuickSlots>();
            m_ImpactDamageSource = new ImpactDamageSource(this);

            GetCamera();
            SetFirstPerson(false);
        }

        protected virtual void GetCamera()
        {
            fpCamera = GetComponentInChildren<FirstPersonCamera>();
        }

        protected virtual void Start()
        {
            // Attach event handlers
            m_MotionController.onGroundImpact += OnGroundImpact;
            m_MotionController.onGroundImpact += OnLanded;
            m_MotionController.onBodyImpact += OnBodyImpact;
            m_MotionController.onHeadImpact += OnHeadImpact;

            // Disable the object (needs a controller to function)
            if (m_Controller == null || !m_Controller.isActiveAndEnabled)
                gameObject.SetActive(false);
        }

        protected virtual void SetFirstPerson(bool firstPerson)
        {
            fpCamera.LookThrough(firstPerson);

            // Hide arms

            // Show body, hide fps arms, etc
            // ...
            // Implement 3rd person body & 1st person body
        }

        #region ICharacter implementation

        [Header("Spring Effects")]

        [SerializeField, NeoObjectInHierarchyField(false), Tooltip("The additive transform handler attached to the head heirarchy of this character (used for things like weapon recoil and impacts).")]
        private AdditiveTransformHandler m_HeadTransformHandler = null;

        [SerializeField, NeoObjectInHierarchyField(false), Tooltip("The additive transform handler attached to the body heirarchy of this character (used for things like leaning and impacts).")]
        private AdditiveTransformHandler m_BodyTransformHandler = null;

        public event CharacterDelegates.OnControllerChange onControllerChanged;
        public event CharacterDelegates.OnIsAliveChange onIsAliveChanged;
        public event CharacterDelegates.OnHitTarget onHitTarget;

        private BaseController m_Controller = null;
        public IController controller
        {
            get { return m_Controller; }
            set
            {
                m_Controller = value as BaseController;
                OnControllerChanged();
            }
        }

        public FirstPersonCamera fpCamera
        {
            get;
            private set;
        }

        public IAimController aimController
        {
            get;
            private set;
        }

        private MotionController m_MotionController = null;
        public IMotionController motionController
        {
            get { return m_MotionController; }
        }

        public ICharacterAudioHandler audioHandler
        {
            get;
            private set;
        }

        public AdditiveTransformHandler headTransformHandler
        {
            get { return m_HeadTransformHandler; }
        }

        public AdditiveTransformHandler bodyTransformHandler
        {
            get { return m_BodyTransformHandler; }
        }

        public IInventory inventory
        {
            get;
            private set;
        }

        public IQuickSlots quickSlots
        {
            get;
            private set;
        }
        
        public bool isAlive
        {
            get
            {
                if (healthManager != null)
                    return healthManager.isAlive;
                else
                    return true;
            }
        }

        public bool isPlayerControlled
        {
            get { return controller != null && controller.isPlayer; }
        }

        public bool isLocalPlayerControlled
        {
            get { return isPlayerControlled; }
        }

        public bool isRemotePlayerControlled
        {
            get { return false; }
        }

        public void Kill()
        {
            if (healthManager != null)
                healthManager.health = 0f;
        }

        protected virtual void OnControllerChanged ()
        {
            if (onControllerChanged != null)
                onControllerChanged(this, m_Controller);
        }

        protected virtual void OnIsAliveChanged(bool isAlive)
        {
            // Collapse here
            if (!isAlive)
            {
                motionController.SetHeightMultiplier(0.45f, 0.5f);

                // Trigger audio
                if (!isAlive && audioHandler != null)
                    audioHandler.PlayAudio(FpsCharacterAudio.Collapse);
            }
            else
                motionController.SetHeightMultiplier(1f, 1f);

            if (onIsAliveChanged != null)
                onIsAliveChanged(this, isAlive);
        }

        public virtual void ReportTargetHit(bool critical)
        {
            if (onHitTarget != null)
                onHitTarget(this, critical);
        }

        #endregion

        #region DAMAGE AUDIO

        [Header("Damage Audio")]

        [Range(0f, 50f)]
        [Tooltip("The amount of damage to take in a single hit before playing a character damage audio clip.")]
        [SerializeField] private float m_DamageAudioThreshold = 10f;

        protected virtual void OnHealthChanged(float from, float to, bool critical, IDamageSource source)
        {
            // Play pain audio if required
            float diff = from - to;
            if (diff > m_DamageAudioThreshold && audioHandler != null)
                audioHandler.PlayAudio(FpsCharacterAudio.Pain);
        }

        #endregion

        #region  IMPACT DAMAGE HANDLING

        [Header("Impacts")]

        [Tooltip("Should the character be subject to damage from landing impacts (impacts where the character capsule is hit in the bottom hemisphere).")]
        [SerializeField] private bool m_ApplyFallDamage = true;
        [Tooltip("The minimum landing impact magnitude before any damage is applied.")]
        [SerializeField] private float m_LandingMinForce = 10f;
        [Tooltip("The landing impact magnitude where a full 100 damage will be applied.")]
        [SerializeField] private float m_LandingFullForce = 40f;
        [Tooltip("Should the character be subject to damage from body impacts (impacts where the character capsule is hit in the central cylinder).")]
        [SerializeField] private bool m_BodyImpactDamage = true;
        [Tooltip("The minimum body impact magnitude before any damage is applied.")]
        [SerializeField] private float m_BodyMinForce = 25f;
        [Tooltip("The body impact magnitude where a full 100 damage will be applied.")]
        [SerializeField] private float m_BodyFullForce = 100f;
        [Tooltip("Should the character be subject to damage from head impacts (impacts where the character capsule is hit in the top hemisphere).")]
        [SerializeField] private bool m_HeadImpactDamage = true;
        [Tooltip("The minimum head impact magnitude before any damage is applied.")]
        [SerializeField] private float m_HeadMinForce = 7.5f;
        [Tooltip("The head impact magnitude where a full 100 damage will be applied.")]
        [SerializeField] private float m_HeadFullForce = 20f;

        public class ImpactDamageSource : IDamageSource
        {
            private DamageFilter m_DamageFilter = new DamageFilter(DamageType.Fall, DamageTeamFilter.All);
            public DamageFilter outDamageFilter
            {
                get { return m_DamageFilter; }
                set { m_DamageFilter = value; }
            }

            private BaseCharacter m_Character = null;
            public IController controller
            {
                get { return m_Character.controller; }
            }

            public Transform damageSourceTransform
            {
                get { return m_Character.localTransform; }
            }

            public string description { get { return "Impact"; } }

            public ImpactDamageSource(BaseCharacter c)
            {
                m_Character = c;
            }
        }

        private ImpactDamageSource m_ImpactDamageSource = null;

        public bool applyFallDamage
        {
            get { return m_ApplyFallDamage; }
            set { m_ApplyFallDamage = value; }
        }

        public bool applyBodyImpactDamage
        {
            get { return m_BodyImpactDamage; }
            set { m_BodyImpactDamage = value; }
        }

        public bool applyHeadImpactDamage
        {
            get { return m_HeadImpactDamage; }
            set { m_HeadImpactDamage = value; }
        }

        void ValidateImpactDamage()
        {
            m_LandingMinForce = Mathf.Clamp(m_LandingMinForce, 1f, 100f);
            m_LandingFullForce = Mathf.Clamp(m_LandingFullForce, 10f, 1000f);
            m_BodyMinForce = Mathf.Clamp(m_BodyMinForce, 1f, 100f);
            m_BodyFullForce = Mathf.Clamp(m_BodyFullForce, 10f, 1000f);
            m_HeadMinForce = Mathf.Clamp(m_HeadMinForce, 1f, 100f);
            m_HeadFullForce = Mathf.Clamp(m_HeadFullForce, 10f, 1000f);
        }

        public void OnGroundImpact(Vector3 impulse)
        {
            if (healthManager == null || !m_ApplyFallDamage)
                return;

            float sqrMagnitude = impulse.sqrMagnitude;
            if (sqrMagnitude > m_LandingMinForce * m_LandingMinForce)
            {
                float damage = 100f * (Mathf.Sqrt(sqrMagnitude) - m_LandingMinForce) / (m_LandingFullForce - m_LandingMinForce);
                healthManager.AddDamage(damage, false, m_ImpactDamageSource);
            }
        }

        public void OnHeadImpact(Vector3 impulse)
        {
            if (healthManager == null || !m_HeadImpactDamage)
                return;

            float sqrMagnitude = impulse.sqrMagnitude;
            if (sqrMagnitude > m_HeadMinForce * m_HeadMinForce)
            {
                float damage = 100f * (Mathf.Sqrt(sqrMagnitude) - m_HeadMinForce) / (m_HeadFullForce - m_HeadMinForce);
                healthManager.AddDamage(damage, false, m_ImpactDamageSource);
            }
        }

        public void OnBodyImpact(Vector3 impulse)
        {
            if (healthManager == null || !m_BodyImpactDamage)
                return;

            float sqrMagnitude = impulse.sqrMagnitude;
            if (sqrMagnitude > m_BodyMinForce * m_BodyMinForce)
            {
                float damage = 100f * (Mathf.Sqrt(sqrMagnitude) - m_BodyMinForce) / (m_BodyFullForce - m_BodyMinForce);
                healthManager.AddDamage(damage, false, m_ImpactDamageSource);
            }
        }

        #endregion

        #region LANDING AUDIO

        [Header("Landing Audio")]
        [Tooltip("Surface audio library used to trigger the correct sound when the character lands below the \"hard landing\" threshold.")]
        [SerializeField] private SurfaceAudioData m_SoftLandings = null;
        [Tooltip("Surface audio library used to trigger the correct sound when the character makes a heavy landing.")]
        [SerializeField] private SurfaceAudioData m_HardLandings = null;
        [Tooltip("The magnitude of the landing force below which no landing sound will be played.")]
        [SerializeField] private float m_MinLandingThreshold = 0.5f;
        [Tooltip("The magnitude of the landing force above which to play a hard landing sound.")]
        [SerializeField] private float m_HardLandingThreshold = 8f;
        [Tooltip("The maximum downward ray length for a ground test.")]
        [SerializeField] private float m_MaxRayDistance = 1f;
        [Tooltip("The vertical offset from the absolute bottom of the character to start the ground test raycast.")]
        [SerializeField] private float m_RayOffset = 0.5f;

        private RaycastHit m_Hit = new RaycastHit();

        public void OnLanded(Vector3 force)
        {
            if (audioHandler == null)
                return;

            // Get force magnitude
            float mag = force.magnitude;
            if (mag < m_MinLandingThreshold)
                return;

            // Get correct surface audio data (soft/hard)
            SurfaceAudioData audio = m_SoftLandings;
            if (audio == null)
            {
                audio = m_HardLandings;
                if (audio == null)
                    return;
            }
            else
            {
                if (m_HardLandings != null && mag > m_HardLandingThreshold)
                    audio = m_HardLandings;
            }

            // Get and play landing clip
            float volume = 1f;
            AudioClip clip = audio.GetAudioClip(GetGroundSurface(), out volume);
            if (clip != null)
                audioHandler.PlayClip(clip, FpsCharacterAudioSource.Feet, volume);
        }

        FpsSurfaceMaterial GetGroundSurface()
        {
            FpsSurfaceMaterial result = FpsSurfaceMaterial.Default;

            Vector3 position = motionController.localTransform.position;
            position.y += m_RayOffset;
            Ray ray = new Ray(position, Vector3.down);
            if (PhysicsExtensions.RaycastNonAllocSingle(ray, out m_Hit, m_MaxRayDistance, PhysicsFilter.Masks.BulletBlockers, motionController.localTransform, QueryTriggerInteraction.Ignore))
            {
                Transform t = m_Hit.transform;
                if (t != null)
                {
                    BaseSurface s = t.GetComponent<BaseSurface>();
                    if (s != null)
                        result = s.GetSurface(m_Hit);
                }
            }
            return result;
        }

        #endregion

        #region INeoSerializableComponent implementation

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
        }

        #endregion
    }
}
