#if UNITY_STANDALONE // Should other platforms use Json text files saved to disk?
#define SETTINGS_USES_JSON
#endif

using System;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/neofpsref-mb-fpsgraphicssettings.html")]
    [CreateAssetMenu(fileName = "FpsSettings_Graphics", menuName = "NeoFPS/Settings/Graphics", order = NeoFpsMenuPriorities.settings_graphics)]
    public class FpsGraphicsSettings : SettingsContext<FpsGraphicsSettings>
	{
		protected override string contextName { get { return "Graphics"; } }

        public override string displayTitle { get { return "NeoFPS Graphics Settings"; } }

        public override string tocName { get { return "Graphics Settings"; } }

        public override string tocID { get { return "settings_graphics"; } }

        [SerializeField, HideInInspector]
        private bool m_SetResolution = false;

        [SerializeField, HideInInspector]
		private int m_ResolutionWidth = 1920;

		[SerializeField, HideInInspector]
		private int m_ResolutionHeight = 1080;

		[SerializeField, HideInInspector]
		private bool m_Fullscreen = true;

		[SerializeField, HideInInspector]
		private int m_VSync = 0;

        [SerializeField, HideInInspector]
        private int m_FrameRateCap = -1;

		[Header("FoV")]

		[SerializeField, Tooltip("The vertical FoV of the first person camera. In a 16:9 monitor, this is 9/16 * the horizontal FoV (90 -> 50.625). Using vertical allows for consistent settings for all portrait aspect ratios.")]
		private float m_VerticalFOV = 50.625f;

		public event UnityAction<float> onVerticalFoVChanged;
		public event UnityAction onResolutionChanged;

		public readonly int[] frameRateCapOptions =
        {
            30,
            60,
            75,
            120,
            144,
            150,
            165,
            180,
            200,
            240
        };

        public override void Load ()
		{
			base.Load ();

            if (m_SetResolution)
            {
                ApplyResolutionChanges();
                QualitySettings.vSyncCount = m_VSync;
            }
            else
            {
                // Get the starting settings if not in the editor
                if (!Application.isEditor)
                {
                    Resolution current = Screen.currentResolution;

                    m_ResolutionWidth = current.width;
                    m_ResolutionHeight = current.height;
                    m_Fullscreen = Screen.fullScreen;
                    m_VSync = QualitySettings.vSyncCount;
                    //m_FrameRateCap = current.refreshRate; - default to -1 (uncapped)

                    m_SetResolution = true;
                    Save();
                }
            }
		}

		public void ApplyResolutionChanges ()
        {
            if (!Application.isEditor)
            {
                Resolution target = resolution;
                Screen.SetResolution(target.width, target.height, fullscreen);
				if (onResolutionChanged != null)
					onResolutionChanged();
			}
		}

		protected override bool CheckIfCurrent ()
		{
			return FpsSettings.graphics == this;
		}

		#if SETTINGS_USES_JSON

		public Resolution resolution
		{
			get { return new Resolution { width = m_ResolutionWidth, height = m_ResolutionHeight }; }
			set
			{
				SetValue (ref m_ResolutionWidth, value.width);
				SetValue (ref m_ResolutionHeight, value.height);
			}
		}

		public bool fullscreen
		{
			get { return m_Fullscreen; }
			set 
			{
				SetValue (ref m_Fullscreen, value);
            }
		}

		public int vSync
		{
			get { return m_VSync; }
			set
			{
				SetValue (ref m_VSync, value);
				QualitySettings.vSyncCount = m_VSync;
			}
		}

        public int frameRateCap
        {
            get { return m_FrameRateCap; }
            set
            {
                SetValue(ref m_FrameRateCap, value);
                Application.targetFrameRate = m_FrameRateCap;
            }
        }

		public float verticalFoV
		{
			get { return m_VerticalFOV; }
			set
			{
				SetValue(ref m_VerticalFOV, value);
				if (onVerticalFoVChanged != null)
					onVerticalFoVChanged(m_VerticalFOV);
			}
		}

#else

		public Resolution resolution
		{
			get
			{
				return new Resolution
				{
					width = GetInt ("gs.resolutionWidth", m_ResolutionWidth),
					height = GetInt ("gs.resolutionHeight", m_ResolutionHeight)
				};
			}
			set
			{
				SetInt ("gs.resolutionWidth", value.width);
				SetInt ("gs.resolutionHeight", value.height);
				ApplyResolutionChanges ();
			}
		}

		public bool fullscreen
		{
			get { return GetBool ("gs.fullscreen", m_Fullscreen); }
			set
			{
				SetBool ("gs.fullscreen", value);
				Screen.fullScreen = value;
			}
		}

		public int vSync
		{
			get { return GetInt ("gs.vsync", m_VSync); }
			set 
			{
				SetInt ("gs.vsync", value);
				QualitySettings.vSyncCount = value;
			}
		}
        
        public int frameRateCap
        {
            get { return GetInt ("gs.fpsCap", m_FrameRateCap); }
            set
            {
				SetInt ("gs.fpsCap", value);
                Application.targetFrameRate = value;
            }
        }
        
        public float verticalFoV
        {
			get { return GetFloat("gs.verticalFoV", m_VerticalFOV); }
			set
			{
				SetFloat("gs.verticalFoV", value);
				if (onVerticalFoVChanged != null)
					onVerticalFoVChanged(m_VerticalFOV);
			}
        }
        
#endif
	}
}

