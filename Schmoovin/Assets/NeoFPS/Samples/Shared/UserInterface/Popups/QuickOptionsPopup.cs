using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

namespace NeoFPS.Samples
{
	public class QuickOptionsPopup : BasePopup
	{
		[SerializeField] private MultiInputSlider m_MasterVolumeSlider = null;
		[SerializeField] private MultiInputSlider m_MusicVolumeSlider = null;
		[SerializeField] private MultiInputSlider m_HorizontalSensitivitySlider = null;
		[SerializeField] private MultiInputSlider m_VerticalSensitivitySlider = null;
		[SerializeField] private MultiInputToggle m_InvertMouseToggle = null;
		[SerializeField] private MultiInputMultiChoice m_VsyncChoice = null;
		[SerializeField] private MultiInputSlider m_FovSlider = null;
		[SerializeField] private MultiInputSlider m_HeadBobSlider = null;

		private static QuickOptionsPopup m_Instance = null;

		private bool m_AudioSettingsChanged = false;
		private bool m_GraphicsSettingsChanged = false;
		private bool m_GameplaySettingsChanged = false;
		private bool m_InputSettingsChanged = false;
		private float m_AspectRatio = 1.78f;

		public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);
			m_Instance = this;

			// Add listeners from code (saves user doing it as prefabs have a tendency to break)
			if (m_MasterVolumeSlider != null)
				m_MasterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
			if (m_MusicVolumeSlider != null)
				m_MusicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
			if (m_VsyncChoice != null)
				m_VsyncChoice.onIndexChanged.AddListener(OnVsyncChanged);
			if (m_HorizontalSensitivitySlider != null)
				m_HorizontalSensitivitySlider.onValueChanged.AddListener(OnHorizontalSensitivityChanged);
			if (m_VerticalSensitivitySlider != null)
				m_VerticalSensitivitySlider.onValueChanged.AddListener(OnVerticalSensitivityChanged);
			if (m_InvertMouseToggle != null)
				m_InvertMouseToggle.onValueChanged.AddListener(OnInvertMouseChanged);
			if (m_HeadBobSlider != null)
				m_HeadBobSlider.onValueChanged.AddListener(OnHeadBobChanged);

			if (m_FovSlider != null)
			{
				// Track resolution to convert vertical FoV to horizontal
				OnResolutionSettingsChanged();
				FpsSettings.graphics.onResolutionChanged += OnResolutionSettingsChanged;

				m_FovSlider.onValueChanged.AddListener(OnHorizontalFoVChanged);
			}
		}

		void OnDestroy ()
		{
			if (m_Instance == this)
				m_Instance = null;
		}

		public override void Back ()
		{
			OnOK ();
		}

		public void OnOK ()
		{
			if (m_AudioSettingsChanged)
				FpsSettings.audio.Save();
			if (m_GraphicsSettingsChanged)
				FpsSettings.graphics.Save();
			if (m_InputSettingsChanged)
				FpsSettings.input.Save();
			if (m_GameplaySettingsChanged)
				FpsSettings.gameplay.Save();

			m_Instance.menu.ShowPopup (null);
		}

		public static void ToggleVisible ()
		{
			if (m_Instance != null)
			{
				if (m_Instance.isActiveAndEnabled)
					m_Instance.OnOK();
				else
					m_Instance.ShowPopupInternal();
			}
		}

		private void OnResolutionSettingsChanged()
		{
			// Get aspect ratio
			m_AspectRatio = (float)Screen.height / Screen.width;

			// Get the min and max values
			int min = Mathf.FloorToInt(35f / (m_AspectRatio * 5f)) * 5;
			int max = Mathf.CeilToInt(67.5f / (m_AspectRatio * 5f)) * 5;

			// Change the slider
			m_FovSlider.SetLimits(min, max);
			m_FovSlider.value = Mathf.RoundToInt(FpsSettings.graphics.verticalFoV / m_AspectRatio);
		}

		void ShowPopupInternal()
		{
			m_AudioSettingsChanged = false;
			m_GraphicsSettingsChanged = false;
			m_InputSettingsChanged = false;
			m_GameplaySettingsChanged = false;

			if (m_MasterVolumeSlider != null)
				m_MasterVolumeSlider.value = Mathf.RoundToInt(FpsSettings.audio.masterVolume * 100f);
			if (m_MusicVolumeSlider != null)
				m_MusicVolumeSlider.value = Mathf.RoundToInt(FpsSettings.audio.musicVolume * 100f);
			if (m_VsyncChoice != null)
				m_VsyncChoice.index = FpsSettings.graphics.vSync;
			if (m_InvertMouseToggle != null)
				m_InvertMouseToggle.value = FpsSettings.input.invertMouse;
			if (m_HorizontalSensitivitySlider != null)
			{
				int current = Mathf.RoundToInt(FpsSettings.input.horizontalMouseSensitivity * 100f);
				m_HorizontalSensitivitySlider.value = current;
			}
			if (m_VerticalSensitivitySlider != null)
			{
				int current = Mathf.RoundToInt(FpsSettings.input.verticalMouseSensitivity * 100f);
				m_VerticalSensitivitySlider.value = current;
			}
			if (m_HeadBobSlider != null)
			{
				int current = Mathf.RoundToInt(FpsSettings.gameplay.headBob * 100f);
				m_HeadBobSlider.value = current;
			}
			if (m_FovSlider != null)
				m_FovSlider.value = Mathf.RoundToInt(FpsSettings.graphics.verticalFoV / m_AspectRatio);

			menu.ShowPopup(this);
		}

		public void OnMasterVolumeChanged(int value)
		{
			FpsSettings.audio.masterVolume = (float)value * 0.01f;
			m_AudioSettingsChanged = true;
		}
		public void OnMusicVolumeChanged(int value)
		{
			FpsSettings.audio.musicVolume = (float)value * 0.01f;
			m_AudioSettingsChanged = true;
		}

		public void OnHorizontalSensitivityChanged(int value)
		{
			FpsSettings.input.horizontalMouseSensitivity = (float)value * 0.01f;
			m_InputSettingsChanged = true;
		}
		public void OnVerticalSensitivityChanged(int value)
		{
			FpsSettings.input.verticalMouseSensitivity = (float)value * 0.01f;
			m_InputSettingsChanged = true;
		}
		public void OnInvertMouseChanged(bool value)
		{
			FpsSettings.input.invertMouse = value;
			m_InputSettingsChanged = true;
		}

		public void OnVsyncChanged(int index)
		{
			FpsSettings.graphics.vSync = index;
			m_GraphicsSettingsChanged = true;
		}
		private void OnHorizontalFoVChanged(int fov)
		{
			FpsSettings.graphics.verticalFoV = m_AspectRatio * fov;
			m_GraphicsSettingsChanged = true;
		}
		public void OnHeadBobChanged(int value)
		{
			FpsSettings.gameplay.headBob = value * 0.01f;
			m_GameplaySettingsChanged = true;
		}
	}
}