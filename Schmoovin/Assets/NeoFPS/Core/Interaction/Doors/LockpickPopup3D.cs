using NeoFPS.Samples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [RequireComponent(typeof(AudioSource))]
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-lockpickpopup3d.html")]
    public class LockpickPopup3D : LockpickPopup, IPickAngleLockpickPopup
    {
        [Header("UI")]
        [SerializeField, Tooltip("The UI popup prefab to be drawn over the top of this minigame using the prefab popup container system.")]
        private LockpickPopupUI m_UiPrefab = null;

        [Header("Lock")]
        [SerializeField, Tooltip("The transform of the lock barrel.")]
        private Transform m_LockTransform = null;
        [SerializeField, Tooltip("The rotation speed of the lock barrel when tensioned (in degrees per second).")]
        private float m_RotationSpeed = 90f;

        [Header("Pick")]
        [SerializeField, Tooltip("The transform of the lock pick object. Its pivot should be lined up with the hole of the lock.")]
        private Transform m_LockPickTransform = null;
        [SerializeField, Tooltip("The smallest size the safe range for the pick can be (at highest difficulty).")]
        private float m_MinSafeRange = 2f;
        [SerializeField, Tooltip("The maximum size the safe range for the pick can be (at lowest difficulty).")]
        private float m_MaxSafeRange = 5f;
        [SerializeField, Tooltip("The smallest falloff outside the safe range, where the lock can rotate but will still snag (at highest difficulty).")]
        private float m_MinFalloff = 10f;
        [SerializeField, Tooltip("The largest falloff outside the safe range, where the lock can rotate but will still snag (at lowest difficulty).")]
        private float m_MaxFalloff = 30f;
        [SerializeField, Tooltip("The number of fixed update ticks where the pick is catching before it will break.")]
        private int m_PickBreakTicks = 50;

        [Header("Jiggle")]
        [SerializeField, Tooltip("The minimum jiggle angle when the pick catches. It will bounce between this and max.")]
        private Vector3 m_JiggleMin = new Vector3(0f, 0f, -1f);
        [SerializeField, Tooltip("The maximum jiggle angle when the pick catches. It will bounce between this and min.")]
        private Vector3 m_JiggleMax = new Vector3(0f, 0f, 1f);
        [SerializeField, Tooltip("The number of shakes per second when catching.")]
        private float m_JiggleRate = 15f;
        [SerializeField, Range(0f, 1f), Tooltip("The amount of time it takes for the jiggle to fade in when the lock catches.")]
        private float m_JiggleStartDamping = 0.25f;
        [SerializeField, Range(0f, 1f), Tooltip("The amount of time it takes for the jiggle to fade out once tension is released.")]
        private float m_JiggleEndDamping = 0.5f;

        [Header("Audio")]

        [SerializeField, Tooltip("The audio loop to play while the pick is jammed in the lock and rattling.")]
        private AudioClip m_PickRattleLoop = null;
        [SerializeField, Tooltip("The clip to play when the lock is successfully picked.")]
        private AudioClip m_UnlockClip = null;
        [SerializeField, Tooltip("The clip to play when the pick is snapped.")]
        private AudioClip m_PickBreakClip = null;
        [SerializeField, Tooltip("The clip to play when starting to tension the lock.")]
        private AudioClip m_TensionClip = null;

        private AudioSource m_AudioSource = null;
        private bool m_Unlocked = false;
        private bool m_HittingLimit = false;
        private bool m_Resetting = false;
        private bool m_Tension = false;
        private bool m_OutOfPicks = false;
        private Vector3 m_PickRotationOffset = Vector3.zero;
        private float m_LockRotation = 0f;
        private float m_PickRotation = 0f;
        private float m_SafeRangeStart = 0f;
        private float m_SafeRangeEnd = 0f;
        private float m_Falloff = 0f;
        private float m_Timeout = 0f;
        private float m_Jiggle = 0f;
        private int m_CurrentPickHealth = 0;
        private LockpickPopupUI m_ActiveUI = null;

        void OnValidate()
        {
            m_MinSafeRange = Mathf.Clamp(m_MinSafeRange, 0.5f, 30f);
            m_MaxSafeRange = Mathf.Clamp(m_MaxSafeRange, 1f, 60f);
            m_MinFalloff = Mathf.Clamp(m_MinFalloff, 0f, 30f);
            m_MaxFalloff = Mathf.Clamp(m_MaxFalloff, 1f, 60f);
            m_RotationSpeed = Mathf.Clamp(m_RotationSpeed, 5f, 360f);
            m_PickBreakTicks = Mathf.Clamp(m_PickBreakTicks, 1, 500);
            //m_JiggleDuration = Mathf.Clamp(m_JiggleDuration, 0.1f, 5f);
        }

        void OnDisable()
        {
            // Show the HUD
            HudHider.ShowHUD();

            if (m_ActiveUI != null)
            {
                m_ActiveUI.menu.ShowPopup(null);
                m_ActiveUI = null;
            }
        }

        protected override void Initialise(ICharacter character)
        {
            // Get audio source
            m_AudioSource = GetComponent<AudioSource>();
            m_AudioSource.loop = true;
            m_AudioSource.playOnAwake = false;

            // Set initial pick health for first pick
            m_CurrentPickHealth = m_PickBreakTicks;

            // Hide the HUD
            HudHider.HideHUD();

            // Calculate the safe range
            float halfSafeRange = Mathf.Lerp(m_MaxSafeRange, m_MinSafeRange, difficulty);
            float center = Random.Range(-90f + halfSafeRange, 90f - halfSafeRange);
            m_SafeRangeStart = center - halfSafeRange;
            m_SafeRangeEnd = center + halfSafeRange;
            m_Falloff = Mathf.Lerp(m_MaxFalloff, m_MinFalloff, difficulty);

            // Reset settings
            m_Unlocked = false;
            m_OutOfPicks = false;
            m_PickRotation = 0f;
            m_LockRotation = 0f;

            // Show UI
            if (m_UiPrefab != null)
            {
                m_ActiveUI = PrefabPopupContainer.ShowPrefabPopup(m_UiPrefab);
                m_ActiveUI.Initialise(this);
            }
        }

        public void ApplyInput(float pickRotation, bool tension)
        {
            // Play tension sound
            if (!m_Tension && tension && m_TensionClip != null)
                m_AudioSource.PlayOneShot(m_TensionClip);

            m_Tension = tension;

            if (!m_Tension)
                m_PickRotation = Mathf.Clamp(m_PickRotation + pickRotation, -90f, 90f);
        }

        void FixedUpdate()
        {
            if (m_Unlocked || m_OutOfPicks)
            {
                m_Timeout += Time.deltaTime;
                if (m_Timeout > 0.5f)
                {
                    if (m_Unlocked)
                        Unlock();
                    else
                        Cancel();
                }
            }
            else
            {
                // Check if lockpick is outside safe range
                float maxLockRotation = 90f;
                if (m_PickRotation < m_SafeRangeStart)
                {
                    // Get max lock rotation
                    float outOfRange = Mathf.Abs(m_PickRotation - m_SafeRangeStart);
                    float normalised = 1f - Mathf.Clamp01(outOfRange / m_Falloff);
                    normalised *= normalised;
                    maxLockRotation = 2f + normalised * 88f;
                }
                if (m_PickRotation > m_SafeRangeEnd)
                {
                    float outOfRange = Mathf.Abs(m_PickRotation - m_SafeRangeEnd);
                    float normalised = 1f - Mathf.Clamp01(outOfRange / m_Falloff);
                    normalised *= normalised;
                    maxLockRotation = 2f + normalised * 88f;
                }

                // Get current lock rotation
                bool rotateLock = m_Tension && !m_Resetting;
                float lockRotationSpeed = rotateLock ? m_RotationSpeed : -m_RotationSpeed;
                m_LockRotation = m_LockRotation + lockRotationSpeed * Time.deltaTime;
                if (m_LockRotation < 0f)
                    m_LockRotation = 0f;

                // Check if lock is opened
                if (m_LockRotation >= 89.99f)
                {
                    m_Unlocked = true;

                    // Play a sound
                    if (m_UnlockClip != null)
                        m_AudioSource.PlayOneShot(m_UnlockClip);
                }
                else
                {
                    // Detect if hitting pick limit
                    if (m_LockRotation > maxLockRotation)
                    {
                        m_LockRotation = maxLockRotation;

                        if (!m_HittingLimit)
                        {
                            // Start the pick rattle sound
                            if (m_PickRattleLoop != null)
                            {
                                m_AudioSource.clip = m_PickRattleLoop;
                                m_AudioSource.Play();
                            }
                        }
                        else
                        {
                            if (pickItem != null)
                            {
                                // Damage pick
                                --m_CurrentPickHealth;
                                if (m_CurrentPickHealth <= 0)
                                {
                                    // Play a sound
                                    if (m_PickBreakClip != null)
                                        m_AudioSource.PlayOneShot(m_PickBreakClip);

                                    if (pickItem.quantity <= 1)
                                    {
                                        m_OutOfPicks = true;
                                    }
                                    else
                                    {
                                        m_CurrentPickHealth = m_PickBreakTicks;
                                        m_Resetting = true;

                                        // Play a sound
                                        StartCoroutine(BreakAndReplacePick());
                                    }

                                    // Reduce pick quantity
                                    --pickItem.quantity;
                                }
                            }
                        }

                        m_HittingLimit = true;
                    }
                    else
                    {
                        if (m_HittingLimit)
                        {
                            m_AudioSource.Stop();
                            m_HittingLimit = false;
                        }
                    }

                    if (!m_Resetting)
                    {
                        // Jiggle
                        bool doJiggle = true;
                        float jiggleLerp = 0f;
                        float jiggleTarget = 0f;
                        if (m_HittingLimit)
                        {
                            jiggleTarget = 1f;
                            jiggleLerp = Mathf.Lerp(0.75f, 0.05f, m_JiggleStartDamping);
                        }
                        else
                        {
                            if (m_Jiggle < 0.001f)
                                doJiggle = false;
                            else
                                jiggleLerp = Mathf.Lerp(0.75f, 0.05f, m_JiggleEndDamping);
                        }

                        if (doJiggle)
                        {
                            float jiggleWave = Mathf.Sin(Time.timeSinceLevelLoad * Mathf.PI * m_JiggleRate);
                            m_PickRotationOffset = Vector3.Lerp(m_JiggleMin, m_JiggleMax, jiggleWave * 0.5f + 0.5f);

                            m_Jiggle = Mathf.Lerp(m_Jiggle, jiggleTarget, jiggleLerp);

                            m_PickRotationOffset *= m_Jiggle;
                        }
                        else
                            m_PickRotationOffset = Vector3.zero;
                    }
                }

                // Rotate the lock transform
                m_LockTransform.localRotation = Quaternion.Euler(0f, 0f, m_LockRotation);

                // Rotate the pick transform
                m_LockPickTransform.localRotation = Quaternion.Euler(m_PickRotationOffset.x, m_PickRotationOffset.y, m_PickRotation - m_LockRotation + m_PickRotationOffset.z);

            }
        }

        IEnumerator BreakAndReplacePick()
        {
            m_PickRotationOffset = new Vector3(10f, 0f, 0f);
            yield return new WaitForSeconds(1f);
            m_Resetting = false;
        }
    }
}