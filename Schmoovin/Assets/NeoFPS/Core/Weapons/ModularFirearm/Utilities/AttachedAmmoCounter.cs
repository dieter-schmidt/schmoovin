using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-attachedammocounter.html")]
    public class AttachedAmmoCounter : MonoBehaviour
    {
        [SerializeField, RequiredObjectProperty, Tooltip("The reloader module to track the current magazine from.")]
        private BaseReloaderBehaviour m_Reloader = null;
        [SerializeField, RequiredObjectProperty, Tooltip("The text output for the current magazine count.")]
        private Text m_AmmoText = null;

        private void OnValidate()
        {
            if (m_Reloader == null)
                m_Reloader = GetComponentInParent<BaseReloaderBehaviour>();
            if (m_AmmoText == null)
                m_AmmoText = GetComponentInChildren<Text>();
        }

        private void Awake()
        {
            if (m_Reloader != null)
            {
                m_Reloader.onCurrentMagazineChange += OnCurrentMagazineChanged;
                OnCurrentMagazineChanged(m_Reloader.firearm, m_Reloader.currentMagazine);
            }
        }

        private void OnCurrentMagazineChanged(IModularFirearm firearm, int count)
        {
            if (m_AmmoText != null)
                m_AmmoText.text = count.ToString();
        }
    }
}