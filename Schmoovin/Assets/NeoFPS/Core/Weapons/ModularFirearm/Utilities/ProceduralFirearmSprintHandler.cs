using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [RequireComponent(typeof(ModularFirearm))]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-proceduralfirearmsprinthandler.html")]
    public class ProceduralFirearmSprintHandler : ProceduralSprintAnimationHandler
    {
        [Header("Firearms")]

        [SerializeField, Tooltip("What to do when the firearms enters / exits ADS. You can pause the animation while aiming, or block sprinting entirely.")]
        private SprintInterruptAction m_ActionOnAim = SprintInterruptAction.StopAnimation;
        [SerializeField, Tooltip("What to do when the firearm is reloaded. You can pause the animation while reloading, or block sprinting entirely.")]
        private SprintInterruptAction m_ActionOnReload = SprintInterruptAction.StopAnimation;
        [SerializeField, Tooltip("What to do when the firearm trigger is pulled while sprinting. You can pause the animation while, or stop sprinting until the trigger is released (both of these have a slight delay before firing to allow the weapon to be aligned). You can also block the firearm from firing at all while sprinting.")]
        private SprintFireAction m_ActionOnFire = SprintFireAction.StopAnimation;
        [SerializeField, Tooltip("The minimum amount of time the firearm sprint animation will be paused or sprinting blocked when the trigger is pulled. Prevents rapid tapping of the trigger popping in and out of sprint")]
        private float m_MinFireDuration = 0.5f;

        private ModularFirearm m_Firearm = null;
        private ITrigger m_Trigger = null;
        private IReloader m_Reloader = null;
        private IAimer m_Aimer = null;
        private bool m_TriggerPressed = false;
        private float m_TriggerTimer = 0f;
        private bool m_DeferredRaise = false;
        private bool m_Aiming = false;
        
        protected override void OnValidate()
        {
            base.OnValidate();

            m_MinFireDuration = Mathf.Clamp(m_MinFireDuration, 0f, 5f);
        }

        protected override void Awake()
        {
            base.Awake();

            // Get firearm
            m_Firearm = GetComponentInChildren<ModularFirearm>();
            m_Firearm.onTriggerChange += OnFirearmTriggerChanged;
            OnFirearmTriggerChanged(m_Firearm, m_Firearm.trigger);
            m_Firearm.onReloaderChange += OnFirearmReloaderChanged;
            OnFirearmReloaderChanged(m_Firearm, m_Firearm.reloader);
            m_Firearm.onAimerChange += OnFirearmAimerChanged;
            OnFirearmAimerChanged(m_Firearm, m_Firearm.aimer);
        }

        protected override void Update()
        {
            if (m_TriggerTimer > 0f)
            {
                m_TriggerTimer -= Time.deltaTime;
                if (m_TriggerTimer <= 0f)
                {
                    m_TriggerTimer = 0f;

                    if (m_DeferredRaise)
                    {
                        RemoveAnimationBlocker();
                        m_DeferredRaise = false;
                    }
                }
            }

            base.Update();
        }

        private void OnFirearmTriggerChanged(IModularFirearm firearm, ITrigger trigger)
        {
            if (m_Trigger != null && m_ActionOnFire != SprintFireAction.CannotFire)
                m_Trigger.onStateChanged -= OnTriggerStateChanged;

            m_Trigger = trigger;

            if (m_Trigger != null && m_ActionOnFire != SprintFireAction.CannotFire)
                m_Trigger.onStateChanged += OnTriggerStateChanged;
        }

        private void OnFirearmReloaderChanged(IModularFirearm firearm, IReloader reloader)
        {
            if (m_Reloader != null)
            {
                m_Reloader.onReloadStart -= OnReloadStart;
                m_Reloader.onReloadComplete -= OnReloadComplete;
                if (m_Reloader.isReloading)
                    OnReloadComplete(null);
            }

            m_Reloader = reloader;

            if (m_Reloader != null)
            {
                m_Reloader.onReloadStart += OnReloadStart;
                m_Reloader.onReloadComplete += OnReloadComplete;
                if (m_Reloader.isReloading)
                    OnReloadStart(null);
            }
        }

        private void OnFirearmAimerChanged(IModularFirearm firearm, IAimer aimer)
        {
            if (m_Aimer != null)
            {
                m_Aimer.onAimStateChanged -= OnAimStateChanged;
                if (m_Aimer.isAiming)
                    OnAimStateChanged(null, FirearmAimState.HipFire);
            }

            m_Aimer = aimer;

            if (m_Aimer != null)
            {
                m_Aimer.onAimStateChanged += OnAimStateChanged;
                if (m_Aimer.isAiming)
                    OnAimStateChanged(null, FirearmAimState.Aiming);
                else
                    OnAimStateChanged(null, FirearmAimState.HipFire);
            }
            else
                OnAimStateChanged(null, FirearmAimState.HipFire);
        }

        private void OnAimStateChanged(IModularFirearm firearm, FirearmAimState state)
        {
            if (m_Aiming)
            {
                if (state == FirearmAimState.HipFire || state == FirearmAimState.ExitingAim)
                {
                    m_Aiming = false;
                    switch (m_ActionOnAim)
                    {
                        case SprintInterruptAction.StopAnimation:
                            RemoveAnimationBlocker();
                            break;
                        case SprintInterruptAction.StopSprinting:
                            RemoveSprintBlocker();
                            break;
                    }
                }
            }
            else
            {
                if (state == FirearmAimState.Aiming || state == FirearmAimState.EnteringAim)
                {
                    m_Aiming = true;
                    switch (m_ActionOnAim)
                    {
                        case SprintInterruptAction.StopAnimation:
                            AddAnimationBlocker();
                            break;
                        case SprintInterruptAction.StopSprinting:
                            AddSprintBlocker();
                            break;
                    }
                }
            }
        }

        private void OnReloadStart(IModularFirearm firearm)
        {
            switch (m_ActionOnReload)
            {
                case SprintInterruptAction.StopAnimation:
                    AddAnimationBlocker();
                    break;
                case SprintInterruptAction.StopSprinting:
                    AddSprintBlocker();
                    break;
            }
        }

        private void OnReloadComplete(IModularFirearm firearm)
        {
            switch (m_ActionOnReload)
            {
                case SprintInterruptAction.StopAnimation:
                    RemoveAnimationBlocker();
                    break;
                case SprintInterruptAction.StopSprinting:
                    RemoveSprintBlocker();
                    break;
            }
        }

        private void OnTriggerStateChanged(bool pressed)
        {
            if (pressed)
            {
                if (!m_TriggerPressed)
                {
                    m_TriggerPressed = true;
                    switch (m_ActionOnFire)
                    {
                        case SprintFireAction.StopSprinting:
                            AddSprintBlocker();
                            break;
                        case SprintFireAction.StopAnimation:
                            {
                                if (m_TriggerTimer <= 0f)
                                    AddAnimationBlocker();

                                // Start a timer to prevent re-entering sprint animation too quickly
                                m_TriggerTimer = m_MinFireDuration;

                                m_DeferredRaise = false;
                            }
                            break;
                    }
                }
            }
            else
            {
                if (m_TriggerPressed)
                {
                    m_TriggerPressed = false;
                    switch (m_ActionOnFire)
                    {
                        case SprintFireAction.StopSprinting:
                            RemoveSprintBlocker();
                            break;
                        case SprintFireAction.StopAnimation:
                            {
                                // If enough time has passed, start entering the sprint animation again
                                if (m_TriggerTimer <= 0f)
                                    RemoveAnimationBlocker();
                                else
                                    m_DeferredRaise = true; // Wait of the timer to start again
                            }
                            break;
                    }
                }
            }
        }

        protected override void OnSprintStateChanged(SprintState s)
        {
            switch (s)
            {
                case SprintState.NotSprinting:
                    {
                        if (m_Firearm != null)
                            m_Firearm.RemoveTriggerBlocker(this);
                    }
                    break;
                case SprintState.EnteringSprint:
                    {
                        if (m_Firearm != null)
                            m_Firearm.AddTriggerBlocker(this);
                    }
                    break;
                case SprintState.Sprinting:
                    {
                        if (m_Firearm != null)
                            m_Firearm.AddTriggerBlocker(this);
                    }
                    break;
            }

            base.OnSprintStateChanged(s);
        }
    }
}