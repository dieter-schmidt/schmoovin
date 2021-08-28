using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
	public class OptionsMenuAudio : OptionsMenuPanel
	{
		[SerializeField] private MultiInputSlider m_MasterVolumeSlider = null;
        [SerializeField] private MultiInputSlider m_EffectsVolumeSlider = null;
        [SerializeField] private MultiInputSlider m_AmbienceVolumeSlider = null;
        [SerializeField] private MultiInputSlider m_MusicVolumeSlider = null;

        public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);

            // Add listeners from code (saves user doing it as prefabs have a tendency to break)
            if (m_MasterVolumeSlider != null)
                m_MasterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (m_EffectsVolumeSlider != null)
                m_EffectsVolumeSlider.onValueChanged.AddListener (OnEffectsVolumeChanged);
            if (m_AmbienceVolumeSlider != null)
                m_AmbienceVolumeSlider.onValueChanged.AddListener(OnAmbienceVolumeChanged);
            if (m_MusicVolumeSlider != null)
                m_MusicVolumeSlider.onValueChanged.AddListener (OnMusicVolumeChanged);
		}

		protected override void SaveOptions ()
		{
			FpsSettings.audio.Save ();
		}

		protected override void ResetOptions ()
        {
            if (m_MasterVolumeSlider != null)
                m_MasterVolumeSlider.value = Mathf.RoundToInt(FpsSettings.audio.masterVolume * 100f);
            if (m_EffectsVolumeSlider != null)
                m_EffectsVolumeSlider.value = Mathf.RoundToInt(FpsSettings.audio.effectsVolume * 100f);
            if (m_AmbienceVolumeSlider != null)
                m_AmbienceVolumeSlider.value = Mathf.RoundToInt(FpsSettings.audio.ambienceVolume * 100f);
            if (m_MusicVolumeSlider != null)
                m_MusicVolumeSlider.value = Mathf.RoundToInt(FpsSettings.audio.musicVolume * 100f);
		}

		public void OnMasterVolumeChanged (int value)
		{
			FpsSettings.audio.masterVolume = (float)value * 0.01f;
		}

		public void OnEffectsVolumeChanged (int value)
		{
			FpsSettings.audio.effectsVolume = (float)value * 0.01f;
        }

        public void OnAmbienceVolumeChanged(int value)
        {
            FpsSettings.audio.ambienceVolume = (float)value * 0.01f;
        }

        public void OnMusicVolumeChanged (int value)
		{
			FpsSettings.audio.musicVolume = (float)value * 0.01f;
		}
	}
}