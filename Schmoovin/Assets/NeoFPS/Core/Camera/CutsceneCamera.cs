using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-custscenecamera.html")]
    public class CutsceneCamera : FpsInput
    {
        [SerializeField, Tooltip("Can the cutscene be skipped by holding the use button")]
        private bool m_CanSkip = false;

        [SerializeField, Tooltip("The amount of time the use button must be held to skip the cutscene")]
        private float m_SkipHold = 2f;

        [SerializeField, Tooltip("An event fired when the cutscene is skipped")]
        private UnityEvent m_OnSkip = new UnityEvent();
        
        public static UnityAction<CutsceneCamera> onCutsceneCameraChanged;
        private static FirstPersonCamera m_FirstPersonCamera = null;
        private static CutsceneCamera m_Current = null;

        public static CutsceneCamera current
        {
            get { return m_Current; }
            private set
            {
                if (m_Current != value)
                {
                    m_Current = value;
                    if (onCutsceneCameraChanged != null)
                        onCutsceneCameraChanged(m_Current);
                }
            }
        }

        public event UnityAction<float> onSkipProgressChanged;

        private float m_SkipTimer = 0f;

        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Cutscene; }
        }
        
        public static void EndCutscene()
        {
            // Disable the current camera
            if (current != null)
                current.gameObject.SetActive(false);

            // Enable the first person camera
            if (m_FirstPersonCamera != null)
            {
                m_FirstPersonCamera.LookThrough(true);
                m_FirstPersonCamera = null;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Sort first person camera
            if (m_FirstPersonCamera == null)
            {
                m_FirstPersonCamera = FirstPersonCamera.current;
                if (m_FirstPersonCamera != null)
                    m_FirstPersonCamera.LookThrough(false);
            }

            // Record old camera
            var old = current;

            // Set as current
            current = this;

            // Disable old camera if required
            if (old != null && old != this)
                old.gameObject.SetActive(false);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            // Set current to null if required
            if (current == this)
                EndCutscene();
        }

        protected override void UpdateInput()
        {
            if (!m_CanSkip)
                return;

            if (m_SkipHold != 0f)
            {
                // Hold to skip
                if (GetButton(FpsInputButton.Use))
                {
                    m_SkipTimer += Time.unscaledDeltaTime / m_SkipHold;
                    if (m_SkipTimer > 1f)
                    {
                        // Reset skip timer
                        m_SkipTimer = 0f;
                        if (onSkipProgressChanged != null)
                            onSkipProgressChanged(0f);

                        // Fire event
                        m_OnSkip.Invoke();

                        // End the cutscene
                        EndCutscene();
                    }
                    else
                    {
                        if (onSkipProgressChanged != null)
                            onSkipProgressChanged(m_SkipTimer);
                    }
                }
                else
                {
                    // Reset skip timer
                    if (m_SkipTimer != 0f)
                    {
                        m_SkipTimer = 0f;
                        if (onSkipProgressChanged != null)
                            onSkipProgressChanged(0f);
                    }
                }
            }
            else
            {
                // Skip on interact
                if (GetButtonDown(FpsInputButton.Use))
                {
                    m_OnSkip.Invoke();
                    EndCutscene();
                }
            }
        }
    }
}