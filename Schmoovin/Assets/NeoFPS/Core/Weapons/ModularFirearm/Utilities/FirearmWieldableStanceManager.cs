using NeoFPS.ModularFirearms;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-firearmwieldablestancemanager.html")]
    public class FirearmWieldableStanceManager : BaseWieldableStanceManager
    {
        private ModularFirearm m_Firearm = null;
        private IReloader m_Reloader = null;
        private IAimer m_Aimer = null;
        private bool m_Aiming = false;

        protected override void Awake()
        {
            base.Awake();

            // Get firearm
            m_Firearm = GetComponentInChildren<ModularFirearm>();
            m_Firearm.onReloaderChange += OnFirearmReloaderChanged;
            OnFirearmReloaderChanged(m_Firearm, m_Firearm.reloader);
            m_Firearm.onAimerChange += OnFirearmAimerChanged;
            OnFirearmAimerChanged(m_Firearm, m_Firearm.aimer);
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
                    RemoveBlocker();
                }
            }
            else
            {
                if (state == FirearmAimState.Aiming || state == FirearmAimState.EnteringAim)
                {
                    m_Aiming = true;
                    AddBlocker();
                }
            }
        }

        private void OnReloadStart(IModularFirearm firearm)
        {
            AddBlocker();
        }

        private void OnReloadComplete(IModularFirearm firearm)
        {
            RemoveBlocker();
        }
    }
}