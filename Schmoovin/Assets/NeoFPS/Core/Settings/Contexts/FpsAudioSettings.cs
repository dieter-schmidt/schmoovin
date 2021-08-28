#if UNITY_STANDALONE // Should other platforms use Json text files saved to disk?
#define SETTINGS_USES_JSON
#endif

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/neofpsref-mb-fpsaudiosettings.html")]
    [CreateAssetMenu(fileName = "FpsSettings_Audio", menuName = "NeoFPS/Settings/Audio", order = NeoFpsMenuPriorities.settings_audio)]
    public class FpsAudioSettings : SettingsContext<FpsAudioSettings>
	{
		protected override string contextName { get { return "Audio"; } }

        public override string displayTitle { get { return "NeoFPS Audio Settings"; } }

        public override string tocName { get { return "Audio Settings"; } }

        public override string tocID { get { return "settings_audio"; } }

        [SerializeField, Tooltip("The overall game volume.")]
		private float m_MasterVolume = 1f;

        [SerializeField, Tooltip("The volume for in game effects.")]
		private float m_EffectsVolume = 1f;

        [SerializeField, Tooltip("The volume for ambience effects.")]
        private float m_AmbienceVolume = 1f;

        [SerializeField, Tooltip("The volume for music.")]
		private float m_MusicVolume = 1f;

        public event UnityAction<float> onMasterVolumeChanged;
        public event UnityAction<float> onEffectsVolumeChanged;
        public event UnityAction<float> onAmbienceVolumeChanged;
        public event UnityAction<float> onMusicVolumeChanged;

        protected override bool CheckIfCurrent ()
		{
			return FpsSettings.audio == this;
		}

#if SETTINGS_USES_JSON

        public float masterVolume
		{
			get { return m_MasterVolume; }
			set
            {
                SetValue (ref m_MasterVolume, Mathf.Clamp01(value));
                if (onMasterVolumeChanged != null)
                    onMasterVolumeChanged(m_MasterVolume);
            }
		}

		public float effectsVolume
		{
			get { return m_EffectsVolume; }
			set
            {
                SetValue (ref m_EffectsVolume, Mathf.Clamp01(value));
                if (onEffectsVolumeChanged != null)
                    onEffectsVolumeChanged(m_EffectsVolume);
            }
        }

        public float ambienceVolume
        {
            get { return m_AmbienceVolume; }
            set
            {
                SetValue(ref m_AmbienceVolume, Mathf.Clamp01(value));
                if (onAmbienceVolumeChanged != null)
                    onAmbienceVolumeChanged(m_AmbienceVolume);
            }
        }

        public float musicVolume
		{
			get { return m_MusicVolume; }
			set
            {
                SetValue (ref m_MusicVolume, Mathf.Clamp01(value));
                if (onMusicVolumeChanged != null)
                    onMusicVolumeChanged(m_MusicVolume);
            }
		}

#else

		public float masterVolume
		{
			get { return GetFloat ("as.masterVolume", m_MasterVolume); }
			set
            {
                value = Mathf.Clamp01(value);
                SetFloat ("as.masterVolume", value);
                if (onMasterVolumeChanged != null)
                    onMasterVolumeChanged(value);
            }
		}

		public float effectsVolume
		{
			get { return GetFloat ("as.effectsVolume", m_EffectsVolume); }
			set
            {
                value = Mathf.Clamp01(value);
                SetFloat ("as.effectsVolume", value);
                if (onEffectsVolumeChanged != null)
                    onEffectsVolumeChanged(value);
            }
		}
        
		public float ambienceVolume
		{
			get { return GetFloat ("as.ambienceVolume", m_AmbienceVolume); }
			set
            {
                value = Mathf.Clamp01(value);
                SetFloat ("as.ambienceVolume", value);
                if (onAmbienceVolumeChanged != null)
                    onAmbienceVolumeChanged(value);
            }
		}

		public float musicVolume
		{
			get { return GetFloat ("as.musicVolume", m_MusicVolume); }
			set
            {
                value = Mathf.Clamp01(value);
                SetFloat ("as.musicVolume", value);
                if (onMusicVolumeChanged != null)
                    onMusicVolumeChanged(value);
            }
		}

#endif
    }
}

