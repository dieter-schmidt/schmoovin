using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
	public class OptionsMenuInput : OptionsMenuPanel
	{
		[SerializeField] private MultiInputSlider m_HorizontalSensitivitySlider = null;
        [SerializeField] private MultiInputSlider m_VerticalSensitivitySlider = null;

        [SerializeField] private MultiInputToggle m_InvertMouseToggle = null;

        [SerializeField] private MultiInputToggle m_MouseSmoothingToggle = null;
        [SerializeField] private MultiInputSlider m_MouseSmoothingSlider = null;

        [SerializeField] private MultiInputToggle m_MouseAccelerationToggle = null;
        [SerializeField] private MultiInputSlider m_MouseAccelerationSlider = null;

        [SerializeField] private MultiInputLabel m_GamepadLabel = null;
        [SerializeField] private MultiInputToggle m_UseGamepadToggle = null;
        [SerializeField] private MultiInputSlider m_GamepadHSensitivitySlider = null;
        [SerializeField] private MultiInputSlider m_GamepadVSensitivitySlider = null;
        [SerializeField] private MultiInputMultiChoice m_GamepadProfilePicker = null;
        [SerializeField] private MultiInputToggle m_InvertGamepadToggle = null;

        private MultiInputWidgetList m_Controls = null;

        public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);

			m_Controls = GetComponentInParent<MultiInputWidgetList> ();

			// Add listeners from code (saves user doing it as prefabs have a tendency to break)
			m_HorizontalSensitivitySlider.onValueChanged.AddListener (OnHorizontalSensitivityChanged);
			m_VerticalSensitivitySlider.onValueChanged.AddListener (OnVerticalSensitivityChanged);
			m_InvertMouseToggle.onValueChanged.AddListener (OnInvertMouseChanged);
			m_MouseSmoothingToggle.onValueChanged.AddListener (OnEnableMouseSmoothingChanged);
			m_MouseSmoothingSlider.onValueChanged.AddListener (OnMouseSmoothingChanged);
			m_MouseAccelerationToggle.onValueChanged.AddListener (OnEnableMouseAccelerationChanged);
			m_MouseAccelerationSlider.onValueChanged.AddListener (OnMouseAccelerationChanged);
			m_UseGamepadToggle.onValueChanged.AddListener (OnUseGamepadChanged);
            m_GamepadProfilePicker.onIndexChanged.AddListener(OnGamepadProfileChanged);
            m_GamepadHSensitivitySlider.onValueChanged.AddListener(OnGamepadHSensitivityChanged);
            m_GamepadVSensitivitySlider.onValueChanged.AddListener(OnGamepadVSensitivityChanged);
            m_InvertGamepadToggle.onValueChanged.AddListener (OnInvertGamepadChanged);

            // Set up profile picker
            List<string> profileOptions = new List<string>();
            for (int i = 0; i < NeoFpsInputManager.gamepadProfiles.Length; ++i)
                profileOptions.Add(NeoFpsInputManager.gamepadProfiles[i].name);
            m_GamepadProfilePicker.options = profileOptions.ToArray();

            // Connect to NeoFpsInputManager
            NeoFpsInputManager.onIsGamepadConnectedChanged += OnIsGamepadConnectedChanged;
            OnIsGamepadConnectedChanged(NeoFpsInputManager.isGamepadConnected);
        }

        void OnIsGamepadConnectedChanged(bool connected)
        {
            // Activate / deactivate objects
            m_UseGamepadToggle.gameObject.SetActive(connected);
            m_GamepadHSensitivitySlider.gameObject.SetActive(connected);
            m_GamepadVSensitivitySlider.gameObject.SetActive(connected);
            m_GamepadProfilePicker.gameObject.SetActive(connected);
            m_InvertGamepadToggle.gameObject.SetActive(connected);

            // Reset navigation
            m_Controls.ResetWidgetNavigation();

            // Set description
            if (connected)
                m_GamepadLabel.description = NeoFpsInputManager.connectedGamepad.name + " Connected";
            else
            {
                m_GamepadLabel.description = "No Gamepad Connected";
                CheckAndResetSelection();
            }
        }

        void CheckAndResetSelection()
        {
            // If one of the gamepad controls is selected, select the first item
            var last = EventSystem.current.currentSelectedGameObject;
            if (last == m_UseGamepadToggle.gameObject ||
                last == m_GamepadProfilePicker.gameObject ||
                last == m_GamepadHSensitivitySlider.gameObject ||
                last == m_GamepadVSensitivitySlider.gameObject ||
                last == m_InvertGamepadToggle.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(m_GamepadLabel.gameObject);
            }
        }

        protected override void SaveOptions ()
		{
			FpsSettings.input.Save ();
		}

		protected override void ResetOptions ()
		{
			// Setup horizontal sensitivity UI from settings
			int current = Mathf.RoundToInt(FpsSettings.input.horizontalMouseSensitivity * 100f);
			m_HorizontalSensitivitySlider.value = current;

			// Setup vertical sensitivity UI from settings
			current = Mathf.RoundToInt(FpsSettings.input.verticalMouseSensitivity * 100f);
			m_VerticalSensitivitySlider.value = current;

			// Set invert mouse UI from settings
			m_InvertMouseToggle.value = FpsSettings.input.invertMouse;

			// Setup mouse smoothing UI from settings
			current = Mathf.RoundToInt(FpsSettings.input.mouseSmoothing * 100f);
			m_MouseSmoothingSlider.value = current;
			m_MouseSmoothingToggle.value = FpsSettings.input.enableMouseSmoothing;
			m_MouseSmoothingSlider.gameObject.SetActive (m_MouseSmoothingToggle.value);

			// Setup mouse acceleration UI from settings
			current = Mathf.RoundToInt(FpsSettings.input.mouseAcceleration * 100f);
			m_MouseAccelerationSlider.value = current;
			m_MouseAccelerationToggle.value = FpsSettings.input.enableMouseAcceleration;
			m_MouseAccelerationSlider.gameObject.SetActive (m_MouseAccelerationToggle.value);

			// Set gamepad UI from settings
			m_UseGamepadToggle.value = FpsSettings.gamepad.useGamepad;
			m_InvertGamepadToggle.value = FpsSettings.gamepad.invertLook;
            m_GamepadProfilePicker.index = FpsSettings.gamepad.profile;

            // Setup horizontal analog sensitivity UI from settings
            current = Mathf.RoundToInt(FpsSettings.gamepad.horizontalAnalogSensitivity * 100f);
            m_GamepadHSensitivitySlider.value = current;

            // Setup vertical analog sensitivity UI from settings
            current = Mathf.RoundToInt(FpsSettings.gamepad.verticalAnalogSensitivity * 100f);
            m_GamepadVSensitivitySlider.value = current;

            // Reset navigation
            m_Controls.ResetWidgetNavigation();
        }

		public void OnHorizontalSensitivityChanged (int value)
		{
			FpsSettings.input.horizontalMouseSensitivity = (float)value * 0.01f;
		}

		public void OnVerticalSensitivityChanged (int value)
		{
			FpsSettings.input.verticalMouseSensitivity = (float)value * 0.01f;
		}

        public void OnInvertMouseChanged (bool value)
		{
			FpsSettings.input.invertMouse = value;
		}

		public void OnEnableMouseSmoothingChanged (bool value)
		{
			FpsSettings.input.enableMouseSmoothing = value;
			m_MouseSmoothingSlider.gameObject.SetActive (value);
			m_Controls.ResetWidgetNavigation ();
		}

		public void OnMouseSmoothingChanged (int value)
		{
			FpsSettings.input.mouseSmoothing = (float)value * 0.01f;
		}

		public void OnEnableMouseAccelerationChanged (bool value)
		{
			FpsSettings.input.enableMouseAcceleration = value;
			m_MouseAccelerationSlider.gameObject.SetActive (value);
			m_Controls.ResetWidgetNavigation ();
		}

		public void OnMouseAccelerationChanged (int value)
		{
			FpsSettings.input.mouseAcceleration = (float)value * 0.01f;
		}

		public void OnUseGamepadChanged (bool toggle)
		{
			FpsSettings.gamepad.useGamepad = toggle;
        }

        public void OnGamepadProfileChanged(int index)
        {
            FpsSettings.gamepad.profile = index;
        }

        public void OnGamepadHSensitivityChanged(int value)
        {
            FpsSettings.gamepad.horizontalAnalogSensitivity = (float)value * 0.01f;
        }

        public void OnGamepadVSensitivityChanged(int value)
        {
            FpsSettings.gamepad.verticalAnalogSensitivity = (float)value * 0.01f;
        }

        public void OnInvertGamepadChanged (bool toggle)
		{
			FpsSettings.gamepad.invertLook = toggle;
		}
	}
}