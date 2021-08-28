using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public abstract class AnimatedSprintAnimationHandler : BaseSprintAnimationHandler
    {
        [SerializeField, Tooltip("The movement speed that the sprint animations are synced to when playing at 1x speed.")]
        private float m_UnscaledSprintMoveSpeed = 10f;
        [Delayed, SerializeField, Tooltip("A maximum speed clamp for the character when used to calculate the animation speed multiplier.")]
        private float m_MaxSpeed = 15f;
        [Delayed, SerializeField, Tooltip("The speed below which the light sprint animation will be 100% used. Above this, the heavy animation is blended in.")]
        private float m_BlendZeroSpeed = 5f;
        [Delayed, SerializeField, Tooltip("The speed above which the heavy sprint animation will be 100% used. Below this, the light animation is blended in.")]
        private float m_BlendFullSpeed = 10f;
        
        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Bool, true, false), Tooltip("A bool parameter on the animator to signify when the weapon enters or exits sprint.")]
        private string m_SprintBoolParameter = "Sprint";
        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Float, true, false), Tooltip("A float parameter on the animator to set the playback speed of the sprint animation.")]
        private string m_SpeedFloatParameter = "SprintSpeed";
        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Float, true, false), Tooltip("A float parameter on the animator used to blend between the light and heavy sprint animations.")]
        private string m_BlendFloatParameter = "SprintBlend";

        private Animator m_Animator = null;
        private int m_SprintTriggerHash = -1;
        private int m_SpeedFloatHash = -1;
        private int m_BlendFloatHash = -1;

        protected override void OnValidate()
        {
            base.OnValidate();
            m_UnscaledSprintMoveSpeed = Mathf.Clamp(m_UnscaledSprintMoveSpeed, 1f, 50f);
            m_MaxSpeed = Mathf.Clamp(m_MaxSpeed, 1f, 50f);
            m_BlendZeroSpeed = Mathf.Clamp(m_BlendZeroSpeed, 1f, m_BlendFullSpeed);
            m_BlendFullSpeed = Mathf.Clamp(m_BlendFullSpeed, m_BlendZeroSpeed, m_MaxSpeed);
            m_UnscaledSprintMoveSpeed = Mathf.Clamp(m_UnscaledSprintMoveSpeed, 1f, 50f);
        }

        protected override void Awake()
        {
            base.Awake();

            m_Animator = GetComponentInChildren<Animator>();
            if (m_Animator != null)
            {
                if (!string.IsNullOrEmpty(m_SprintBoolParameter))
                    m_SprintTriggerHash = Animator.StringToHash(m_SprintBoolParameter);
                if (!string.IsNullOrEmpty(m_SpeedFloatParameter))
                    m_SpeedFloatHash = Animator.StringToHash(m_SpeedFloatParameter);
                if (!string.IsNullOrEmpty(m_BlendFloatParameter))
                    m_BlendFloatHash = Animator.StringToHash(m_BlendFloatParameter);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (sprintState != SprintState.NotSprinting)
            {
                float speed = GetSpeedParameterValue();
                if (m_SpeedFloatHash != -1)
                    m_Animator.SetFloat(m_SpeedFloatHash, speed);
                if (m_BlendFloatHash != -1)
                {
                    float blend = Mathf.Clamp01((speed - m_BlendZeroSpeed) / (m_BlendFullSpeed - m_BlendZeroSpeed));
                    m_Animator.SetFloat(m_BlendFloatHash, blend);
                }
            }
        }

        protected override void OnSprintStateChanged(SprintState s)
        {
            if (m_SprintTriggerHash != -1)
            {
                if (s == SprintState.EnteringSprint || s == SprintState.Sprinting)
                    m_Animator.SetBool(m_SprintTriggerHash, true);
                else
                    m_Animator.SetBool(m_SprintTriggerHash, false);
            }
        }

        protected virtual float GetSpeedParameterValue()
        {
            float result = sprintSpeed;
            if (result > m_MaxSpeed)
                result = m_MaxSpeed;
            return result / m_UnscaledSprintMoveSpeed;
        }
    }
}

