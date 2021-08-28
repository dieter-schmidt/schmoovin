using System;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [RequireComponent(typeof(ModularFirearm))]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-reloadercountdown.html")]
    public class ReloaderCountdown : MonoBehaviour
    {
        [SerializeField, Tooltip("The audio clips to play as ammo is consumed. First element is for the last round in the magazine, second = penultimate, and so on.")]
        private CountdownAudio[] m_CountdownAudio = new CountdownAudio[0];

        [SerializeField, Tooltip("If set, then the last clip will be used for all ammo until the count is within the sequence.")]
        private bool m_ExtendSequence = false;

        [Serializable]
        public struct CountdownAudio
        {
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
        }

        ModularFirearm m_Firearm = null;
        private IReloader m_Reloader = null;

        private void Awake()
        {
            m_Firearm = GetComponent<ModularFirearm>();
            m_Firearm.onReloaderChange += OnReloaderChanged;
            OnReloaderChanged(m_Firearm, m_Firearm.reloader);
        }

        void OnReloaderChanged(IModularFirearm f, IReloader r)
        {
            if (m_Reloader != null)
                m_Reloader.onCurrentMagazineChange -= OnCurrentMagazineChanged;
            m_Reloader = r;
            if (m_Reloader != null)
                m_Reloader.onCurrentMagazineChange += OnCurrentMagazineChanged;
        }

        void OnCurrentMagazineChanged(IModularFirearm f, int count)
        {
            // Check the ammo count
            if (count >= m_CountdownAudio.Length)
            {
                if (m_ExtendSequence)
                    count = m_CountdownAudio.Length - 1;
                else
                    return;
            }

            // Play the clip
            var clip = m_CountdownAudio[count].clip;
            if (clip != null)
                m_Firearm.PlaySound(clip, m_CountdownAudio[count].volume);
        }
    }
}