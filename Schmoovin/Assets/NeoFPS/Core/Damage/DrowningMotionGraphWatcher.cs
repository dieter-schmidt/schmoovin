using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [RequireComponent(typeof(MotionController))]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mb-drowningmotiongraphwatcher.html")]
    public class DrowningMotionGraphWatcher : MonoBehaviour, IDamageSource
    {
        [SerializeField, Tooltip("The motion graph parameter used to track time underwater.")]
        private string m_ParameterKey = "underwaterTime";
        [SerializeField, Tooltip("The description of the damage to use in logs, etc.")]
        private string m_DamageDescription = "Drowning";
        [SerializeField, Tooltip("The damage type filter.")]
        private DamageType m_DamageType = DamageType.Drowning;
        [SerializeField, Tooltip("The amount of time the character can hold their breath before taking damage.")]
        private float m_HoldBreathDuration = 20f;
        [SerializeField, Tooltip("The interval between damage ticks (in seconds) when drowning.")]
        private float m_DamageSpacing = 4f;
        [SerializeField, Tooltip("The amount of damage the character takes each damnage tick when drowning.")]
        private float m_DamageAmount = 5f;
        
        public event UnityAction onBreathRemainingChanged;
        public event UnityAction<bool> onUnderwaterChanged;

        private IHealthManager m_HealthManager = null;
        private FloatParameter m_UnderwaterParameter = null;
        private float m_NextDamageTick = 0f;
        private float m_BreathRemaining = 0f;
        private bool m_Underwater = false;

        public float holdBreathDuration
        {
            get { return m_HoldBreathDuration; }
        }

        public float breathRemaining
        {
            get { return m_BreathRemaining; }
            private set
            {
                value = Mathf.Clamp(value, 0f, m_HoldBreathDuration);
                if (m_BreathRemaining != value)
                {
                    m_BreathRemaining = value;
                    if (onBreathRemainingChanged != null)
                        onBreathRemainingChanged();
                }
            }
        }

        public bool underwater
        {
            get { return m_Underwater; }
            private set
            {
                if (m_Underwater != value)
                {
                    m_Underwater = value;
                    if (onUnderwaterChanged != null)
                        onUnderwaterChanged(m_Underwater);
                }
            }
        }

        private void Awake()
        {
            // Set damage type
            m_OutDamageFilter.SetDamageType(m_DamageType);

            // Get health manager
            m_HealthManager = GetComponent<IHealthManager>();
            if (m_HealthManager == null)
            {
                Debug.LogError("No health manager found on character: " + name);
                return;
            }

            // Get motion controller
            var mc = GetComponent<MotionController>();
            m_UnderwaterParameter = mc.motionGraph.GetFloatProperty(m_ParameterKey);
            if (m_UnderwaterParameter == null)
            {
                Debug.LogError(string.Format("Motion graph for character \"{0}\" does not have a float parameter named \"{1}\"", name, m_ParameterKey));
                return;
            }
            else
            {
                // Subscribe
                m_UnderwaterParameter.onValueChanged += OnUnderwaterTimeChanged;
                m_NextDamageTick = m_HoldBreathDuration;
            }

            breathRemaining = m_HoldBreathDuration;
        }

        private void OnDestroy()
        {
            // Unsubscribe
            if (m_UnderwaterParameter != null)
                m_UnderwaterParameter.onValueChanged -= OnUnderwaterTimeChanged;
        }

        void OnUnderwaterTimeChanged (float time)
        {
            if (time == 0f)
            {
                m_NextDamageTick = m_HoldBreathDuration;
                underwater = false;
            }
            else
            {
                if (time > m_NextDamageTick)
                {
                    m_HealthManager.AddDamage(m_DamageAmount, false, this);
                    m_NextDamageTick += m_DamageSpacing;
                }
                underwater = true;
            }

            breathRemaining = m_HoldBreathDuration - time;
        }

        #region IDamageSource IMPLEMENTATION

        private DamageFilter m_OutDamageFilter = new DamageFilter(DamageType.Drowning, DamageTeamFilter.All);
        public DamageFilter outDamageFilter
        {
            get { return m_OutDamageFilter; }
            set { m_OutDamageFilter = value; }
        }

        public IController controller
        {
            get { return GetComponent<ICharacter>().controller; }
        }

        public Transform damageSourceTransform
        {
            get { return transform; }
        }

        public string description
        {
            get { return m_DamageDescription; }
        }

        #endregion
    }
}