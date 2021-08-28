using NeoFPS.ModularFirearms;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-firearmoverheat.html")]
    public class FirearmOverheat : MonoBehaviour, INeoSerializableComponent
    {
        [Header("Glow")]

        [SerializeField, Tooltip("The mesh renderer of the glow material")]
        private MeshRenderer m_GlowRenderer = null;
        [SerializeField, Tooltip("The index of the glow material in the mesh renderer")]
        private int m_GlowMaterialIndex = 0;
        [SerializeField, Tooltip("The heat level required before the weapon starts to glow")]
        private float m_GlowThreshold = 0.25f;

        [Header("Haze")]

        [SerializeField, Tooltip("The mesh renderer of the haze material")]
        private MeshRenderer m_HazeRenderer = null;
        [SerializeField, Tooltip("The index of the haze material in the mesh renderer")]
        private int m_HazeMaterialIndex = 0;
        [SerializeField, Tooltip("The heat level required before the weapon haze starts to show")]
        private float m_HazeThreshold = 0.1f;

        [Header("Overheat")]
        [SerializeField, Tooltip("The amount of heat to add with each shot of the weapon. When this reaches 1, the gun must cool down before it can fire again.")]
        private float m_HeatPerShot = 0.02f;
        [SerializeField, Tooltip("The amount of heat that is dissipated per second. The weapon will never overheat if this is higher than the heat per shot multiplied by rate of fire (rounds per second).")]
        private float m_HeatLostPerSecond = 0.2f;
        [SerializeField, Range (0f, 1f), Tooltip("An event that is fired when the heat hits the max level")]
        private float m_Damping = 0.5f;
        [SerializeField, Tooltip("If true, then once the weapon reaches max heat the weapon will overheat, blocking the trigger until it has cooled down to a set threshold")]
        private bool m_DoOverheat = true;
        [SerializeField, Tooltip("Once overheated, the weapon must cool to this heat level before it can fire again.")]
        private float m_CoolingThreshold = 0.25f;
        [SerializeField, Tooltip("The audio clip to play once max heat is hit and the trigger is blocked")]
        private AudioClip m_OverheatSound = null;
        [SerializeField, Range(0f, 1f), Tooltip("The volume to play the overheat sound at")]
        private float m_Volume = 1f;
        [SerializeField, Tooltip("An event that is fired when the heat hits the max level")]
        private UnityEvent m_OnOverheat = new UnityEvent();

        private const float k_MinDampingMultiplier = 0.5f;
        private const float k_MinUnsmoothedLimit = 0.05f;
        private const float k_MaxUnsmoothedLimit = 0.1f;
        private static readonly NeoSerializationKey k_HeatKey = new NeoSerializationKey("heat");

        public event UnityAction<float> onHeatValueChanged;

        public event UnityAction onOverheat
        {
            add { m_OnOverheat.AddListener(value); }
            remove { m_OnOverheat.RemoveListener(value); }
        }

        private IModularFirearm m_Firearm = null;
        private IShooter m_Shooter = null;
        private MaterialPropertyBlock m_GlowPropertyBlock = null;
        private int m_GlowNameID = -1;
        private MaterialPropertyBlock m_HazePropertyBlock = null;
        private int m_HazeNameID = -1;
        private float m_LastChangeTime = 0f;
        private float m_UnsmoothedHeat = 0f;
        private float m_SmoothingOverrun = 1f;
        private float m_MaxDampingMultiplier = 25f;

        public float heat
        {
            get;
            private set;
        }

        public bool overheated
        {
            get;
            private set;
        }

        public bool canOverheat
        {
            get { return m_DoOverheat; }
        }

        public float coolingThreshold
        {
            get { return m_CoolingThreshold; }
        }

        void OnValidate()
        {
            m_GlowThreshold = Mathf.Clamp01(m_GlowThreshold);
            m_HazeThreshold = Mathf.Clamp01(m_HazeThreshold);
            m_HeatPerShot = Mathf.Clamp(m_HeatPerShot, 0.01f, 1f);
            m_HeatLostPerSecond = Mathf.Clamp(m_HeatLostPerSecond, 0.01f, 1f);
        }
        
        void Awake()
        {
            Initialise();

            // Subscribe to shooter change event
            m_Firearm = GetComponentInParent<IModularFirearm>();
            if (m_Firearm != null)
            {
                m_Firearm.onShooterChange += OnShooterChange;
                OnShooterChange(m_Firearm, m_Firearm.shooter);
            }

            // Get heat limit before damping
            if (m_Damping > 0.0001f)
            {
                m_SmoothingOverrun = Mathf.Lerp(k_MinUnsmoothedLimit, k_MaxUnsmoothedLimit, m_Damping);
                m_MaxDampingMultiplier = (0.75f / Time.fixedDeltaTime);
            }
        }

        void OnDestroy()
        {
            if (m_Firearm != null)
            {
                OnShooterChange(m_Firearm, null);
                m_Firearm.onShooterChange -= OnShooterChange;
            }
        }

        void Initialise()
        {
            // Set up glow
            if (m_GlowRenderer != null && m_GlowPropertyBlock == null)
            {
                m_GlowPropertyBlock = new MaterialPropertyBlock();
                m_GlowNameID = Shader.PropertyToID("_Glow");
            }

            // Set up haze
            if (m_HazeRenderer != null && m_HazePropertyBlock == null)
            {
                m_HazePropertyBlock = new MaterialPropertyBlock();
                m_HazeNameID = Shader.PropertyToID("_HazeIntensity");
            }

            m_UnsmoothedHeat = 0f;
            SetHeat(0f);
        }

        void FixedUpdate()
        {
            if (heat > 0f)
            {
                float delta = Time.fixedTime - m_LastChangeTime;
                SetHeat(m_UnsmoothedHeat - m_HeatLostPerSecond * delta);
            }
        }

        public void SetHeat(float h)
        {
            float delta = Time.fixedTime - m_LastChangeTime;

            // Damp the heat
            if (m_Damping > 0.0001f)
            {
                float easedDamping = EasingFunctions.EaseOutQuadratic(m_Damping);
                m_SmoothingOverrun = Mathf.Lerp(k_MinUnsmoothedLimit, k_MaxUnsmoothedLimit, easedDamping);
                m_UnsmoothedHeat = Mathf.Clamp(h, -m_SmoothingOverrun, 1f + m_SmoothingOverrun);
                m_MaxDampingMultiplier = (0.75f / Time.fixedDeltaTime);
                float timeMultiplier = Mathf.Lerp(m_MaxDampingMultiplier, k_MinDampingMultiplier, easedDamping);
                heat = Mathf.Lerp(heat, m_UnsmoothedHeat, delta * timeMultiplier);
            }
            else
                heat = Mathf.Clamp01(m_UnsmoothedHeat);

            // Apply the glow
            if (m_GlowRenderer != null && m_GlowThreshold < 0.999f)
            {
                float glow = EasingFunctions.EaseInQuadratic((heat - m_GlowThreshold) / (1f - m_GlowThreshold));
                m_GlowPropertyBlock.SetFloat(m_GlowNameID, glow);
                m_GlowRenderer.SetPropertyBlock(m_GlowPropertyBlock, m_GlowMaterialIndex);
            }

            // Apply the haze
            if (m_HazeRenderer != null && m_HazeThreshold < 0.999f)
            {
                float haze = Mathf.Clamp01((heat - m_HazeThreshold) / (1f - m_HazeThreshold));
                if (haze <= 0.001f)
                {
                    m_HazeRenderer.gameObject.SetActive(false);
                }
                else
                {
                    m_HazeRenderer.gameObject.SetActive(true);
                    m_HazePropertyBlock.SetFloat(m_HazeNameID, haze);
                    m_HazeRenderer.SetPropertyBlock(m_HazePropertyBlock, m_HazeMaterialIndex);
                }
            }

            // Check heat thresholds
            if (overheated)
            {
                // Cooling threshold reached
                if (heat < m_CoolingThreshold)
                {
                    // Unblock
                    overheated = false;
                    m_Firearm.RemoveTriggerBlocker(this);

                    // Call function
                    OnCooledToThreshold();
                }
            }
            else
            {
                // Overheat limit reached
                if (heat > 0.999f && m_DoOverheat)
                {
                    // Block the trigger
                    overheated = true;
                    m_Firearm.AddTriggerBlocker(this);

                    // Hisssssssssss
                    if (m_OverheatSound != null)
                        m_Firearm.PlaySound(m_OverheatSound, m_Volume);

                    // Call function (fires event)
                    OnOverheated();
                }
            }

            // Record the changed time (prevents cooling from pausing while gun is holstered)
            m_LastChangeTime = Time.fixedTime;

            // Fire on-change event
            if (onHeatValueChanged != null)
                onHeatValueChanged(heat);
        }

        void OnShooterChange(IModularFirearm firearm, IShooter shooter)
        {
            if (m_Shooter != null)
                m_Shooter.onShoot -= OnShooterShoot;
            m_Shooter = shooter;
            if (m_Shooter != null)
                m_Shooter.onShoot += OnShooterShoot;
        }

        void OnShooterShoot(IModularFirearm firearm)
        {
            SetHeat(m_UnsmoothedHeat + m_HeatPerShot);
        }

        protected virtual void OnOverheated()
        {
            m_OnOverheat.Invoke();
        }
         
        protected virtual void OnCooledToThreshold()
        { }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            Initialise();

            float h = 0f;
            if (reader.TryReadValue(k_HeatKey, out h, 0f))
                SetHeat(h);
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_HeatKey, heat);
        }
    }
}