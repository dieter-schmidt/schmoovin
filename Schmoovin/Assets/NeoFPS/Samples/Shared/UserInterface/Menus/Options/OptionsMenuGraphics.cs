using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
	public class OptionsMenuGraphics : OptionsMenuPanel
	{
		// NB: For custom, game does not remember changes to quality levels between runs (but does remember which quality level).
		// Can apply an existing and just write out settings to file

		[SerializeField] private MultiInputMultiChoice m_ResolutionChoice = null;
        [SerializeField] private MultiInputMultiChoice m_VsyncChoice = null;
        [SerializeField] private MultiInputMultiChoice m_FpsCapChoice = null;
        [SerializeField] private MultiInputButton m_ApplyResolutionButton = null;
        [SerializeField] private MultiInputButton m_ResetResolutionButton = null;
        [SerializeField] private MultiInputToggle m_FullscreenToggle = null;
        [SerializeField] private MultiInputSlider m_FovSlider = null;

        private int m_ResolutionIndex = -1;
        private bool m_Fullscreen = false;
        private MultiInputWidgetList m_Controls = null;
        private List<ResolutionOption> m_ResolutionOptions = new List<ResolutionOption>();
        private int[] m_FpsCapOptions = null;
        private float m_AspectRatio = 1.78f;

        class ResolutionOption
        {
            public int width;
            public int height;
            public string title;
            public int index;
        }

        private bool m_ResolutionChanged = false;
        public bool resolutionChanged
        {
            get { return m_ResolutionChanged; }
            set
            {
                m_ResolutionChanged = value;
                m_ApplyResolutionButton.interactable = value;
                m_ApplyResolutionButton.RefreshInteractable();
                m_ResetResolutionButton.interactable = value;
                m_ResetResolutionButton.RefreshInteractable();
            }
        }

        public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);

            m_Controls = GetComponentInParent<MultiInputWidgetList>();

            // Add listeners from code (saves user doing it as prefabs have a tendency to break)
            m_ResolutionChoice.onIndexChanged.AddListener (OnResolutionChanged);
			m_VsyncChoice.onIndexChanged.AddListener (OnVsyncChanged);
            m_FpsCapChoice.onIndexChanged.AddListener(OnFrameRateCapChanged);
            m_FullscreenToggle.onValueChanged.AddListener (OnFullScreenChanged);
            m_ApplyResolutionButton.onClick.AddListener(OnApplyResolutionButtonPressed);
            m_ResetResolutionButton.onClick.AddListener(OnResetResolutionButtonPressed);

            // Fill resolution options
            var resolutions = Screen.resolutions;
            if (Application.isEditor)
			{
				string[] options = new string[1];
				options [0] = "Not Available In Editor";
				m_ResolutionChoice.options = options;
			}
			else
			{
                for (int i = 0; i < resolutions.Length; ++i)
                {
                    if (m_ResolutionOptions.Count > 0)
                    {
                        var lastOption = m_ResolutionOptions[m_ResolutionOptions.Count - 1];
                        if (resolutions[i].width != lastOption.width || resolutions[i].height != lastOption.height)
                        {
                            ResolutionOption option = new ResolutionOption();
                            option.width = resolutions[i].width;
                            option.height = resolutions[i].height;
                            option.title = string.Format("{0} x {1}", resolutions[i].width, resolutions[i].height);
                            option.index = i;
                            m_ResolutionOptions.Add(option);
                        }
                    }
                    else
                    {
                        ResolutionOption option = new ResolutionOption();
                        option.width = resolutions[i].width;
                        option.height = resolutions[i].height;
                        option.title = string.Format("{0} x {1}", resolutions[i].width, resolutions[i].height);
                        option.index = i;
                        m_ResolutionOptions.Add(option);
                    }
                }

                // Create options strings for UI widget
                string[] options = new string[m_ResolutionOptions.Count];
                for (int i = 0; i < m_ResolutionOptions.Count; ++i)
                    options[i] = m_ResolutionOptions[i].title;
                m_ResolutionChoice.options = options;
			}

            // Fill frame rate cap options
            int currentFpsCap = FpsSettings.graphics.frameRateCap;

            var fpsCapOptions = new List<KeyValuePair<string, int>>();
            fpsCapOptions.Add(new KeyValuePair<string, int>("Uncapped", -1));

            bool currentFound = currentFpsCap == -1;
            for (int i = 0; i < FpsSettings.graphics.frameRateCapOptions.Length; ++i)
            {
                int cap = FpsSettings.graphics.frameRateCapOptions[i];
                fpsCapOptions.Add(new KeyValuePair<string, int>(cap.ToString(), cap));
                if (currentFpsCap == cap)
                    currentFound = true;
            }

            if (!currentFound)
            {
                fpsCapOptions.Add(new KeyValuePair<string, int>(currentFpsCap.ToString(), currentFpsCap));
                fpsCapOptions.Sort((lhs, rhs) => { return lhs.Value.CompareTo(rhs.Value); });
            }

            string[] fpsCapEntries = new string[fpsCapOptions.Count];
            m_FpsCapOptions = new int[fpsCapOptions.Count];
            for (int i = 0; i < fpsCapOptions.Count; ++i)
            {
                fpsCapEntries[i] = fpsCapOptions[i].Key;
                m_FpsCapOptions[i] = fpsCapOptions[i].Value;
            }

            m_FpsCapChoice.options = fpsCapEntries;

            // Initialise FoV settings
            if (m_FovSlider != null)
            {
                // Track resolution to convert vertical FoV to horizontal
                OnResolutionSettingsChanged();
                FpsSettings.graphics.onResolutionChanged += OnResolutionSettingsChanged;

                m_FovSlider.onValueChanged.AddListener(OnHorizontalFoVChanged);
            }

            ResetOptions();
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

        private void OnHorizontalFoVChanged(int fov)
        {
            FpsSettings.graphics.verticalFoV = m_AspectRatio * fov;
        }

        protected override void SaveOptions ()
		{
			FpsSettings.graphics.Save ();
		}

		protected override void ResetOptions ()
		{
            // Set resolution outside editor only
            if (!Application.isEditor)
            {
                m_ResolutionChoice.index = m_ResolutionIndex = GetResolutionIndex(FpsSettings.graphics.resolution);
                m_FpsCapChoice.index = GetFpsCapIndex(FpsSettings.graphics.frameRateCap);
            }

			// Set UI elements
			m_FullscreenToggle.value = m_Fullscreen = FpsSettings.graphics.fullscreen;
			m_VsyncChoice.index = FpsSettings.graphics.vSync;
            resolutionChanged = false;

            // Show / hide frame rate cap
            m_FpsCapChoice.gameObject.SetActive(m_VsyncChoice.index == 0);
            m_Controls.ResetWidgetNavigation();

            // Reset FoV
            if (m_FovSlider != null)
                m_FovSlider.value = Mathf.RoundToInt(FpsSettings.graphics.verticalFoV / m_AspectRatio);
        }

        private int GetResolutionIndex (Resolution res)
		{
			for (int i = 0; i < m_ResolutionOptions.Count; ++i)
			{
				if (m_ResolutionOptions[i].width == res.width && m_ResolutionOptions[i].height == res.height)
					return i;
			}
			return 0;
		}

        private int GetFpsCapIndex(int cap)
        {
            for (int i = 0; i < m_FpsCapOptions.Length; ++i)
            {
                if (m_FpsCapOptions[i] == cap)
                    return i;
            }
            return 0;
        }

        public void OnResolutionChanged (int index)
		{
            // Store settings
            if (!Application.isEditor)
                m_ResolutionIndex = index;
            resolutionChanged = true;
        }

		public void OnFullScreenChanged (bool value)
        {
            // Store settings
            if (!Application.isEditor)
                m_Fullscreen = value;
            resolutionChanged = true;
        }

		public void OnVsyncChanged (int index)
		{
			// Store setting
			FpsSettings.graphics.vSync = index;

            // Show / hide frame rate cap
            m_FpsCapChoice.gameObject.SetActive(m_VsyncChoice.index == 0);
            m_Controls.ResetWidgetNavigation();
        }

        public void OnFrameRateCapChanged(int index)
        {
            FpsSettings.graphics.frameRateCap = m_FpsCapOptions[index];
        }

        public void OnApplyResolutionButtonPressed ()
        {
            // Apply resolution
            FpsSettings.graphics.fullscreen = m_Fullscreen;
            FpsSettings.graphics.resolution = Screen.resolutions[m_ResolutionOptions[m_ResolutionIndex].index];
            FpsSettings.graphics.ApplyResolutionChanges();

            // Reset mouse
            NeoFpsInputManager.captureMouseCursor = true;
            NeoFpsInputManager.captureMouseCursor = false;

            ResetOptions();
        }

        public void OnResetResolutionButtonPressed ()
        {
            // Reset resolution
            ResetOptions();
        }
	}
}