using System.Collections;
using UnityEngine;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-animonlyaimer.html")]
	public class AnimOnlyAimer : BaseAimerBehaviour
    {
        [SerializeField, Range(0f, 2f), Tooltip("The time it takes to reach full aim, or return to zero aim.")]
        private float m_AimTime = 0.25f;

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Bool, true, true), Tooltip("The animator parameter key for a bool used to control aiming state in animations.")]
        private string m_AimAnimBool = "";

        [SerializeField, Tooltip("If true then the gun cannot fire while transitioning in and out of aim mode. This is used to prevent gunshots interrupting the animation.")]
        private bool m_BlockTrigger = true;

        private float m_Lerp = 0f;
        private float m_LerpMultiplier = 0f;
        private int m_AimAnimBoolHash = -1;

        public override float fovMultiplier
        {
            get { return 1f; }
        }

        public bool blockTriggerOnTransition
        {
            get { return m_AimAnimBoolHash != -1 && m_BlockTrigger; }
        }

        public override float aimUpDuration
        {
            get { return m_AimTime; }
        }

        public override float aimDownDuration
        {
            get { return m_AimTime; }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // Set starting values
            m_Lerp = 0f;
            firearm.SetRecoilMultiplier(1f, 1f);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            firearm.RemoveTriggerBlocker(this);
        }

        protected override void Awake()
        {
            base.Awake();

            m_LerpMultiplier = 1f / m_AimTime;
            // Get animation hash
            if (!string.IsNullOrEmpty(m_AimAnimBool))
                m_AimAnimBoolHash = Animator.StringToHash(m_AimAnimBool);
        }

        protected override void AimInternal()
        {
            // Stop aim coroutine
            if (m_CurrentAimCoroutine != null)
                StopCoroutine(m_CurrentAimCoroutine);
            // Start aim up coroutine
            if (gameObject.activeInHierarchy)
                m_CurrentAimCoroutine = StartCoroutine(AimCoroutine(true));
            // Set animator bool parameter
            if (firearm.animator != null && m_AimAnimBoolHash != -1 && firearm.animator.isActiveAndEnabled)
                firearm.animator.SetBool(m_AimAnimBoolHash, true);
        }

        protected override void StopAimInternal(bool instant)
        {
            // Set animator bool parameter
            if (m_AimAnimBoolHash != -1 && firearm.animator != null && firearm.animator.isActiveAndEnabled)
                firearm.animator.SetBool(m_AimAnimBoolHash, false);
            // Stop aim coroutine
            if (m_CurrentAimCoroutine != null)
            {
                StopCoroutine(m_CurrentAimCoroutine);
                m_CurrentAimCoroutine = null;
            }
            // Insant vs animated
            if (instant)
            {
                // Enter hip fire mode
                m_Lerp = 0f;
                SetAimState(FirearmAimState.HipFire);
            }
            else
            {
                // Start aim down coroutine
                if (gameObject.activeInHierarchy)
                    m_CurrentAimCoroutine = StartCoroutine(AimCoroutine(false));
            }
        }

        private Coroutine m_CurrentAimCoroutine = null;
        IEnumerator AimCoroutine(bool up)
        {
            // Block the trigger (prevents shot interrupting raise / lower)
            if (blockTriggerOnTransition)
                firearm.AddTriggerBlocker(this);

            if (up)
            {
                SetAimState(FirearmAimState.EnteringAim);
                while (m_Lerp < 1f)
                {
                    yield return null;
                    m_Lerp += Time.deltaTime * m_LerpMultiplier;
                    if (m_Lerp > 1f)
                    {
                        m_Lerp = 1f;
                        SetAimState(FirearmAimState.Aiming);
                    }
                }
            }
            else
            {
                SetAimState(FirearmAimState.ExitingAim);
                while (m_Lerp > 0f)
                {
                    yield return null;
                    m_Lerp -= Time.deltaTime * m_LerpMultiplier;
                    if (m_Lerp < 0f)
                    {
                        m_Lerp = 0f;
                        SetAimState(FirearmAimState.HipFire);
                    }
                }
            }

            // Unblock the trigger
            if (blockTriggerOnTransition)
                firearm.RemoveTriggerBlocker(this);

            m_CurrentAimCoroutine = null;
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            if (isAiming)
                m_Lerp = 1f;
            else
                m_Lerp = 0f;
        }
    }
}