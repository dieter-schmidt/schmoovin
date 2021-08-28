using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.MotionData;
using System;
using UnityEngine.Events;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    public class StaminaSystem : MonoBehaviour, IMotionGraphDataOverride, IStaminaSystem, IBreathHandler
    {
        [Header("Stamina")]

        [SerializeField, Tooltip("The current stamina of the character. This acts as the starting stamina and changes at runtime.")]
        private float m_Stamina = 100f;
        [SerializeField, Tooltip("The maximum stamina of the character.")]
        private float m_MaxStamina = 100f;
        [SerializeField, Tooltip("The rate that stamina increases over time when no drains are applied.")]
        private float m_StaminaRefreshRate = 10f;

        [Header("Movement Speed")]

        [SerializeField, Tooltip("Should the stamina system modify movement speed based on current stamina. The other settings will be hidden if this is false.")]
        private bool m_AffectMovementSpeed = true;

        [SerializeField, Range(0f, 1f), Tooltip("A multiplier applied to the walking speed at minimum stamina.")]
        private float m_MinWalkMultiplier = 0.5f;
        [SerializeField, Range(0f, 1f), Tooltip("A multiplier applied to the sprinting speed at minimum stamina.")]
        private float m_MinSprintMultiplier = 0.5f;
        [SerializeField, Range(0f, 1f), Tooltip("A multiplier applied to the crouching speed at minimum stamina.")]
        private float m_MinCrouchMultiplier = 0.5f;

        [SerializeField, Tooltip("A curve that defines the character speed based on stamina. The X axis is the normalised stamina (stamina / max), while the Y axis is the min to max lerp value (0 = min, 1 = max).")]
        private AnimationCurve m_MoveSpeedCurve = new AnimationCurve(new Keyframe[] {
            new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 1f)
            });

        [SerializeField, MotionGraphDataKey(MotionGraphDataType.Float), Tooltip("The name of the motion data property on the motion graph that defines walk speed.")]
        private string m_WalkSpeedData = string.Empty;
        [SerializeField, MotionGraphDataKey(MotionGraphDataType.Float), Tooltip("The name of the motion data property on the motion graph that defines walk speed when aiming.")]
        private string m_AimWalkSpeedData = string.Empty;
        [SerializeField, MotionGraphDataKey(MotionGraphDataType.Float), Tooltip("The name of the motion data property on the motion graph that defines sprint speed.")]
        private string m_SprintSpeedData = string.Empty;
        [SerializeField, MotionGraphDataKey(MotionGraphDataType.Float), Tooltip("The name of the motion data property on the motion graph that defines sprint speed when aiming.")]
        private string m_AimSprintSpeedData = string.Empty;
        [SerializeField, MotionGraphDataKey(MotionGraphDataType.Float), Tooltip("The name of the motion data property on the motion graph that defines crouch movement speed.")]
        private string m_CrouchSpeedData = string.Empty;
        [SerializeField, MotionGraphDataKey(MotionGraphDataType.Float), Tooltip("The name of the motion data property on the motion graph that defines crouch movement speed when aiming.")]
        private string m_AimCrouchSpeedData = string.Empty;

        [Header("Breathing")]

        [SerializeField, Tooltip("The time in seconds between breaths (when breathing slow).")]
        private float m_BreatheSlowInterval = 5f;
        [SerializeField, Tooltip("The time in seconds between breaths (when breathing fast).")]
        private float m_BreatheFastInterval = 1f;
        [SerializeField, Tooltip("A curve that defines the breathing rate based on stamina. The X-axis is the normalised stamina (stamina / max stamina), and the Y-Axis is a lerp between slow and fast breathing rate (0 = slow, 1 = fast).")]
        private AnimationCurve m_BreathingRateCurve = new AnimationCurve(new Keyframe[] {
            new Keyframe(0f, 1f), new Keyframe(0.5f, 0.5f), new Keyframe(1f, 0.5f)
            });
        [SerializeField, Tooltip("A curve that defines the breathing strength based on stamina. The X-axis is the normalised stamina (stamina / max stamina), and the Y-Axis is the strength of the character's breathing (0 = non-existant, 1 = heaving/panting).")]
        private AnimationCurve m_BreathingStrengthCurve = new AnimationCurve(new Keyframe[] {
            new Keyframe(0f, 1f), new Keyframe(0.5f, 0.25f), new Keyframe(1f, 0.25f)
            });
			
        [Header("Exhaustion")]

        [SerializeField, Tooltip("Should the character suffer an exhaustion effect on hitting a specific stamina threshold. The other properties will be hidden if this is false.")]
        private bool m_UseExhaustion = true;
        [SerializeField, Tooltip("The stamina level below which the character will become exhausted.")]
        private float m_ExhaustionThreshold = 0f;
        [SerializeField, Tooltip("The character will stop being exhausted once their stamina has recovered above this value.")]
        private float m_RecoverThreshold = 50f;
        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Switch), Tooltip("The name of the switch motion graph parameter that the graph uses as a condition for preventing sprinting.")]
        private string m_ExhaustedMotionParameter = string.Empty;
        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Switch), Tooltip("The name of the switch motion graph parameter that the character input handler sets to tell the motion graph to start sprinting.")]
        private string m_SprintMotionParameter = string.Empty;

        [Space]

        [SerializeField, Tooltip("An event which is fired when the character hits the exhaustion threshold.")]
        private UnityEvent m_OnExhausted = new UnityEvent();
        [SerializeField, Tooltip("An event which is fired when the character recovers from exhaustion.")]
        private UnityEvent m_OnRecovered = new UnityEvent();

        private List<StaminaDrainDelegate> m_StaminaDrains = new List<StaminaDrainDelegate>(4);
        private SwitchParameter m_ExhaustedSwitch = null;
        private SwitchParameter m_SprintSwitch = null;

        public event UnityAction onStaminaChanged;

        public event UnityAction onExhausted
        {
            add { m_OnExhausted.AddListener(value); }
            remove { m_OnExhausted.RemoveListener(value); }
        }

        public event UnityAction onRecovered
        {
            add { m_OnRecovered.AddListener(value); }
            remove { m_OnRecovered.RemoveListener(value); }
        }

        public float stamina
        {
            get { return m_Stamina; }
            private set { m_Stamina = value; }
        }

        public float maxStamina
        {
            get { return m_MaxStamina; }
            set
            {
                m_MaxStamina = Mathf.Clamp(value, 1f, 100000f);
                staminaNormalised = stamina / m_MaxStamina;
            }
        }

        public float staminaRefreshRate
        {
            get { return m_StaminaRefreshRate; }
            set { m_StaminaRefreshRate = Mathf.Clamp(value, 0.1f, 100000f); }
        }

        public float staminaNormalised
        {
            get;
            private set;
        }

        public bool isExhausted
        {
            get;
            private set;
        }
        
        public float breathCounter
        {
            get;
            private set;
        }

        public float breathStrength
        {
            get;
            private set;
        }
        
        public float breathingRate
        {
            get;
            private set;
        }

        void OnValidate()
        {
            // Stamina
            m_MaxStamina = Mathf.Clamp(m_MaxStamina, 1f, 100000f);
            m_Stamina = Mathf.Clamp(m_Stamina, 1f, 100000f);
            m_StaminaRefreshRate = Mathf.Clamp(m_StaminaRefreshRate, 0.1f, 100000f);
            
            // Breathing
            m_BreatheSlowInterval = Mathf.Clamp(m_BreatheSlowInterval, 1f, 20f);
            m_BreatheFastInterval = Mathf.Clamp(m_BreatheFastInterval, 0.1f, 10f);

            // Exhaustion
            m_ExhaustionThreshold = Mathf.Clamp(m_ExhaustionThreshold, 1f, m_MaxStamina - 1f);
            m_RecoverThreshold = Mathf.Clamp(m_RecoverThreshold, m_ExhaustionThreshold + 1f, m_MaxStamina);
        }

        void Start()
        {
            var mc = GetComponent<MotionController>();
            if (mc != null)
            {
                // Apply movement speed overrides
                if (m_AffectMovementSpeed)
                    mc.motionGraph.AddDataOverrides(this);
                // Get motion graph parameter references
                if (!string.IsNullOrEmpty(m_ExhaustedMotionParameter))
                    m_ExhaustedSwitch = mc.motionGraph.GetSwitchProperty(m_ExhaustedMotionParameter);
                if (!string.IsNullOrEmpty(m_SprintMotionParameter))
                    m_SprintSwitch = mc.motionGraph.GetSwitchProperty(m_SprintMotionParameter);
            }
                        
            // Set starting stamina (to update breathing, etc)
            SetStamina(stamina);
        }

        public void SetStamina(float s, bool normalised = false)
        {
            // Apply
            if (normalised)
            {
                staminaNormalised = Mathf.Clamp01(s);
                stamina = staminaNormalised * m_MaxStamina;
            }
            else
            {
                stamina = Mathf.Clamp(s, 0f, m_MaxStamina);
                staminaNormalised = stamina / m_MaxStamina;
            }


            // Update breathing
            breathStrength = m_BreathingStrengthCurve.Evaluate(staminaNormalised);
            breathingRate = m_BreathingRateCurve.Evaluate(staminaNormalised);

            // Check for exhaustion changes
            if (m_UseExhaustion)
            {
                if (isExhausted)
                {
                    if (stamina > m_RecoverThreshold)
                    {
                        isExhausted = false;
                        m_OnRecovered.Invoke();
                        if (m_ExhaustedSwitch != null)
                            m_ExhaustedSwitch.on = false;
                        if (m_SprintSwitch != null)
                        {
                            m_SprintSwitch.on = false;
                            m_SprintSwitch.RemoveBlocker();
                        }
                    }
                }
                else
                {
                    if (stamina < m_ExhaustionThreshold)
                    {
                        isExhausted = true;
                        m_OnExhausted.Invoke();
                        if (m_ExhaustedSwitch != null)
                            m_ExhaustedSwitch.on = true;
                        if (m_SprintSwitch != null)
                        {
                            m_SprintSwitch.on = false;
                            m_SprintSwitch.AddBlocker();
                        }
                    }
                }
            }

            // Signal stamina changed
            OnStaminaChange();
        }

        protected virtual void OnStaminaChange()
        {
            // Trigger event
            if (onStaminaChanged != null)
                onStaminaChanged();
        }

        public Func<float, float> GetFloatOverride(FloatData data)
        {
            if (!string.IsNullOrEmpty(m_WalkSpeedData) && data.name == m_WalkSpeedData)
                return GetMoveSpeedWalking;
            if (!string.IsNullOrEmpty(m_AimWalkSpeedData) && data.name == m_AimWalkSpeedData)
                return GetMoveSpeedWalking;
            if (!string.IsNullOrEmpty(m_SprintSpeedData) && data.name == m_SprintSpeedData)
                return GetMoveSpeedSprinting;
            if (!string.IsNullOrEmpty(m_AimSprintSpeedData) && data.name == m_AimSprintSpeedData)
                return GetMoveSpeedSprinting;
            if (!string.IsNullOrEmpty(m_CrouchSpeedData) && data.name == m_CrouchSpeedData)
                return GetMoveSpeedCrouching;
            if (!string.IsNullOrEmpty(m_AimCrouchSpeedData) && data.name == m_AimCrouchSpeedData)
                return GetMoveSpeedCrouching;

            return null;
        }
        
        float GetMoveSpeedWalking(float input)
        {
            return Mathf.Lerp(input * m_MinWalkMultiplier, input, m_MoveSpeedCurve.Evaluate(staminaNormalised));
        }
        
        float GetMoveSpeedSprinting(float input)
        {
            return Mathf.Lerp(input * m_MinSprintMultiplier, input, m_MoveSpeedCurve.Evaluate(staminaNormalised));
        }
        
        float GetMoveSpeedCrouching(float input)
        {
            return Mathf.Lerp(input * m_MinCrouchMultiplier, input, m_MoveSpeedCurve.Evaluate(staminaNormalised));
        }
        
        public Func<int, int> GetIntOverride(IntData data)
        {
            return null;
        }

        public Func<bool, bool> GetBoolOverride(BoolData data)
        {
            return null;
        }

        public void AddStaminaDrain(StaminaDrainDelegate drain)
        {
            if (drain == null || m_StaminaDrains.Contains(drain))
                return;

            m_StaminaDrains.Add(drain);
        }

        public void RemoveStaminaDrain(StaminaDrainDelegate drain)
        {
            m_StaminaDrains.Remove(drain);
        }

        public void IncrementStamina(float amount, bool isFactor = false)
        {
            if (isFactor)
                SetStamina(stamina + amount * maxStamina);
            else
                SetStamina(stamina + amount);
        }

        public void DecrementStamina(float amount, bool isFactor = false)
        {
            if (isFactor)
                SetStamina(stamina - amount * maxStamina);
            else
                SetStamina(stamina - amount);
        }
        
        void FixedUpdate()
        {
            // Update breathing cycle
            breathCounter += 2f * Time.deltaTime / Mathf.Lerp(m_BreatheSlowInterval, m_BreatheFastInterval, breathingRate);

            // Refresh first (falloff doesn't work as well otherwise)
            float s = stamina + m_StaminaRefreshRate * Time.deltaTime;

            // Drain
            for (int i = 0; i < m_StaminaDrains.Count; ++i)
                s -= m_StaminaDrains[i].Invoke(this, s);

            // Clamp & assign
            s = Mathf.Clamp(s, 0f, m_MaxStamina);
            if (!Mathf.Approximately(s, stamina))
                SetStamina(s);
        }

        public float GetBreathCycle()
        {
            return EasingFunctions.EaseInOutQuadratic(Mathf.PingPong(breathCounter, 1f)) * 2f - 1f;
        }

        public float GetBreathCycle(float offset)
        {
            return EasingFunctions.EaseInOutQuadratic(Mathf.PingPong(breathCounter + offset, 1f)) * 2f - 1f;
        }

        public float GetBreathCycle(float offset, float multiplier)
        {
            return EasingFunctions.EaseInOutQuadratic(Mathf.PingPong(multiplier * (breathCounter + offset), 1f)) * 2f - 1f;
        }

        #region INeoSerializableComponent IMPLEMENTATION
        
        private static readonly NeoSerializationKey k_BreathCounterKey = new NeoSerializationKey("counter");
        private static readonly NeoSerializationKey k_BreathStaminaKey = new NeoSerializationKey("stamina");
        private static readonly NeoSerializationKey k_BreathMaxStaminaKey = new NeoSerializationKey("maxStamina");
        private static readonly NeoSerializationKey k_BreathStaminaRefreshKey = new NeoSerializationKey("staminaRefresh");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_BreathCounterKey, breathCounter);
            writer.WriteValue(k_BreathStaminaKey, stamina);
            writer.WriteValue(k_BreathMaxStaminaKey, maxStamina);
            writer.WriteValue(k_BreathStaminaRefreshKey, staminaRefreshRate);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            float floatValue = 0f;
            if (reader.TryReadValue(k_BreathCounterKey, out floatValue, 0f))
                breathCounter = floatValue;
            if (reader.TryReadValue(k_BreathMaxStaminaKey, out floatValue, maxStamina))
                maxStamina = floatValue;
            if (reader.TryReadValue(k_BreathStaminaKey, out floatValue, stamina))
                SetStamina(floatValue);
            if (reader.TryReadValue(k_BreathStaminaRefreshKey, out floatValue, staminaRefreshRate))
                staminaRefreshRate = floatValue;
        }

        #endregion
    }
}