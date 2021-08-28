using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.CharacterMotion;
using NeoFPS.ModularFirearms;
using NeoCC;
using System;

namespace NeoFPS
{
    public abstract class BaseSprintAnimationHandler : MonoBehaviour
    {
        [Header("Motion Graph")]

        [SerializeField, Tooltip("The minimum speed the character must be moving for the sprint animation to play.")]
        private float m_MinSpeed = 2.5f;
        [SerializeField, Tooltip("[REQUIRED] - The switch parameter on the motion graph which is set by the input handler to tell the character when to sprint.")]
        private string m_SprintInputParamKey = "sprint";
        [SerializeField, Tooltip("[REQUIRED] - A switch parameter on the motion graph which the graph sets when the character is sprinting.")]
        private string m_IsSprintingParamKey = "isSprinting";
        [SerializeField, Tooltip("[OPTIONAL] - A switch parameter on the motion graph which tells the character if it can sprint or not.")]
        private string m_CanSprintParamKey = "canSprint";

        [Header("Animation")]

        [SerializeField, Tooltip("The time taken to blend into the sprint animation.")]
        private float m_InTime = 0.5f;
        [SerializeField, Tooltip("The time taken to blend out of the sprint animation to idle.")]
        private float m_OutTime = 0.25f;
        
        private NeoCharacterController m_CharacterController = null;
        private SwitchParameter m_SprintInputParameter = null;
        private SwitchParameter m_IsSprintingParameter = null;
        private SwitchParameter m_CanSprintParameter = null;
        private SprintState m_SprintState = SprintState.NotSprinting;
        private IWieldable m_Wieldable = null;
        private ICharacter m_Wielder = null;
        private int m_SprintBlockers = 0;
        private int m_AnimationBlockers = 0;
        private bool m_TooSlow = false;

        public enum SprintState
        {
            NotSprinting,
            EnteringSprint,
            Sprinting,
            ExitingSprint
        }

        public SprintState sprintState
        {
            get { return m_SprintState; }
            private set
            {
                if (m_SprintState != value)
                {
                    m_SprintState = value;
                    OnSprintStateChanged(m_SprintState);
                }
            }
        }

        public float sprintSpeed
        {
            get;
            private set;
        }

        protected float sprintWeight
        {
            get;
            private set;
        }

        public float inTime
        {
            get { return m_InTime; }
        }

        public float outTime
        {
            get { return m_OutTime; }
        }

        protected virtual void OnValidate()
        {
            if (m_MinSpeed < 0f)
                m_MinSpeed = 0f;
            if (m_InTime < 0f)
                m_InTime = 0f;
            if (m_OutTime < 0f)
                m_OutTime = 0f;
        }

        protected virtual void Awake()
        {
            m_Wieldable = GetComponent<IWieldable>();
            if (m_Wieldable == null)
                Debug.LogError("Sprint animation handler requires a wieldable component such as a firearm or melee weapon", gameObject);
        }

        protected virtual void OnEnable()
        {
            if (m_Wieldable != null)
            {
                m_Wieldable.onWielderChanged += OnWielderChanged;
                OnWielderChanged(m_Wieldable.wielder);
            }
        }

        protected virtual void OnDisable()
        {
            if (m_Wieldable != null)
            {
                m_Wieldable.onWielderChanged -= OnWielderChanged;
                OnWielderChanged(null);
            }

            ResetSprintBlockers();
            ResetAnimationBlockers();

            m_TooSlow = false;
        }

        protected virtual void Update()
        {
            // Get sprint speed
            if (m_CharacterController != null)
            {
                sprintSpeed = Mathf.Lerp(sprintSpeed, m_CharacterController.velocity.magnitude, Time.deltaTime * 5f);

                // Check sprint speed
                if (sprintSpeed > (m_MinSpeed + 0.001f) && m_TooSlow)
                {
                    m_TooSlow = false;
                    RemoveAnimationBlocker();
                }
                if (sprintSpeed < (m_MinSpeed - 0.001f) && !m_TooSlow)
                {
                    m_TooSlow = true;
                    AddAnimationBlocker();
                }
            }

            switch (m_SprintState)
            {
                case SprintState.EnteringSprint:
                    sprintWeight += Time.deltaTime / m_InTime;
                    if (sprintWeight > 1f)
                    {
                        sprintWeight = 1f;
                        sprintState = SprintState.Sprinting;
                    }
                    break;
                case SprintState.ExitingSprint:
                    sprintWeight -= Time.deltaTime / m_OutTime;
                    if (sprintWeight < 0f)
                    {
                        sprintWeight = 0f;
                        sprintState = SprintState.NotSprinting;
                    }
                    break;
            }
        }
        
        protected void AddSprintBlocker()
        {
            // Set sprint input parameter
            if (m_SprintInputParameter != null)
                m_SprintInputParameter.on = false;

            // Set can-sprint parameter
            if (m_CanSprintParameter != null)
                m_CanSprintParameter.on = false;

            ++m_SprintBlockers;
        }

        protected void RemoveSprintBlocker()
        {
            --m_SprintBlockers;
            if (m_SprintBlockers < 0)
                m_SprintBlockers = 0;

            // Set can-sprint parameter
            if (m_SprintBlockers == 0 && m_CanSprintParameter != null)
                m_CanSprintParameter.on = true;
        }

        protected void ResetSprintBlockers()
        {
            m_SprintBlockers = 0;

            // Set can-sprint parameter
            if (m_CanSprintParameter != null)
                m_CanSprintParameter.on = true;
        }

        protected void AddAnimationBlocker()
        {
            // If the animation is active, exit the animation
            if (sprintState == SprintState.EnteringSprint || sprintState == SprintState.Sprinting)
            {
                if (m_OutTime > 0f)
                    sprintState = SprintState.ExitingSprint;
                else
                    sprintState = SprintState.NotSprinting;
            }

            ++m_AnimationBlockers;
        }

        protected void RemoveAnimationBlocker()
        {
            --m_AnimationBlockers;
            if (m_AnimationBlockers < 0)
                m_AnimationBlockers = 0;

            if (m_AnimationBlockers == 0)
            {
                // Resume sprinting
                if (m_IsSprintingParameter != null && m_IsSprintingParameter.on)
                {
                    if (m_InTime > 0f)
                        sprintState = SprintState.EnteringSprint;
                    else
                        sprintState = SprintState.Sprinting;
                }
            }
        }

        protected void ResetAnimationBlockers()
        {
            m_AnimationBlockers = 0;
        }

        void OnWielderChanged(ICharacter wielder)
        {
            // Detach from old wielder
            if (m_Wielder != null)
                DetachFromWielder(m_Wielder);

            // Clear old references
            m_SprintInputParameter = null;
            m_CanSprintParameter = null;
            m_CharacterController = null;

            // Set new wielder
            m_Wielder = wielder;

            // Get new parameter references
            if (m_Wielder != null)
                AttachToWielder(m_Wielder);
        }

        protected virtual void AttachToWielder(ICharacter wielder)
        {
            var mc = wielder.GetComponent<MotionController>();
            if (mc != null)
            {
                m_CharacterController = wielder.GetComponent<NeoCharacterController>();

                var mg = mc.motionGraph;
                if (mg != null)
                {
                    if (!string.IsNullOrEmpty(m_SprintInputParamKey))
                        m_SprintInputParameter = mg.GetSwitchProperty(m_SprintInputParamKey);
                    if (!string.IsNullOrEmpty(m_IsSprintingParamKey))
                        m_IsSprintingParameter = mg.GetSwitchProperty(m_IsSprintingParamKey);
                    if (!string.IsNullOrEmpty(m_CanSprintParamKey))
                        m_CanSprintParameter = mg.GetSwitchProperty(m_CanSprintParamKey);
                }

                // Attach event handlers
                if (m_IsSprintingParameter != null)
                {
                    m_IsSprintingParameter.onValueChanged += OnMotionGraphIsSprintingChanged;
                    OnMotionGraphIsSprintingChanged(m_IsSprintingParameter.on);
                }
                else
                    sprintState = SprintState.NotSprinting;
            }
        }

        protected virtual void DetachFromWielder(ICharacter wielder)
        {
            // Detach event handlers
            if (m_IsSprintingParameter != null)
                m_IsSprintingParameter.onValueChanged -= OnMotionGraphIsSprintingChanged;

            m_IsSprintingParameter = null;
        }

        void OnMotionGraphIsSprintingChanged(bool value)
        {
            if (m_AnimationBlockers == 0 && m_SprintBlockers == 0)
            {
                if (value)
                {
                    sprintSpeed = 0f;
                    if (m_InTime > 0f)
                        sprintState = SprintState.EnteringSprint;
                    else
                        sprintState = SprintState.Sprinting;
                }
                else
                {
                    if (m_OutTime > 0f)
                        sprintState = SprintState.ExitingSprint;
                    else
                        sprintState = SprintState.NotSprinting;
                }
            }
        }

        protected abstract void OnSprintStateChanged(SprintState s);
    }
}