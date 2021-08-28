#if UNITY_STANDALONE 
// Should other platforms use Json text files saved to disk?
#define SETTINGS_USES_JSON
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoFPS.Constants;
using System.Collections;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/neofpsref-mb-fpsinputsettings.html")]
    [CreateAssetMenu(fileName = "FpsSettings_Input", menuName = "NeoFPS/Settings/Input", order = NeoFpsMenuPriorities.settings_input)]
    public class FpsInputSettings : SettingsContext<FpsInputSettings>
	{
		protected override string contextName { get { return "Input"; } }

        public override string displayTitle { get { return "NeoFPS Input Settings"; } }

        public override string tocName { get { return "Input Settings"; } }

        public override string tocID { get { return "settings_input"; } }

        [Header ("Mouse")]

		[SerializeField, Tooltip("The horizontal mouse look sensitivity.")]
		private float m_MouseSensitivityH = 0.5f;

		[SerializeField, Tooltip("The vertical mouse look sensitivity.")]
		private float m_MouseSensitivityV = 0.5f;

		[SerializeField, Tooltip("Invert the mouse vertical aim.")]
		private bool m_InvertMouse = false;

		[SerializeField, Tooltip("Mouse smoothing takes a weighted average of the mouse movement over time for a smoother effect.")]
		private bool m_EnableMouseSmoothing = false;

		[SerializeField, Tooltip("The amount of mouse smoothing to add.")]
		private float m_MouseSmoothing = 0.5f;

		[SerializeField, Tooltip("Mouse acceleration amplifies faster mouse movements.")]
		private bool m_EnableMouseAcceleration = false;

		[SerializeField, Tooltip("The amount of mouse acceleration to add.")]
		private float m_MouseAcceleration = 0.5f;

        public event UnityAction onMouseSettingsChanged;

		protected override bool CheckIfCurrent ()
		{
			return FpsSettings.input == this;
		}

		void OnValidate()
		{
			m_MouseSensitivityH = Mathf.Clamp(m_MouseSensitivityH, 0.1f, 5f);
			m_MouseSensitivityV = Mathf.Clamp(m_MouseSensitivityV, 0.1f, 5f);
			m_MouseAcceleration = Mathf.Clamp01(m_MouseAcceleration);
			m_MouseSmoothing = Mathf.Clamp01(m_MouseSmoothing);
		}
                
#if SETTINGS_USES_JSON

		public float horizontalMouseSensitivity
		{
			get { return m_MouseSensitivityH; }
			set
			{
				SetValue (ref m_MouseSensitivityH, Mathf.Clamp(value, 0.1f, 5f));
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public float verticalMouseSensitivity
		{
			get { return m_MouseSensitivityV; }
			set
			{
				SetValue (ref m_MouseSensitivityV, Mathf.Clamp(value, 0.1f, 5f));
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public bool invertMouse
		{
			get { return m_InvertMouse; }
			set
			{
				SetValue (ref m_InvertMouse, value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public bool enableMouseSmoothing
		{
			get { return m_EnableMouseSmoothing; }
			set
			{
				SetValue (ref m_EnableMouseSmoothing, value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public float mouseSmoothing
		{
			get { return m_MouseSmoothing; }
			set
			{
				SetValue (ref m_MouseSmoothing, Mathf.Clamp01(value));
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public bool enableMouseAcceleration
		{
			get { return m_EnableMouseAcceleration; }
			set
			{
				SetValue (ref m_EnableMouseAcceleration, value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public float mouseAcceleration
		{
			get { return m_MouseAcceleration; }
			set
			{
				SetValue (ref m_MouseAcceleration, Mathf.Clamp01(value));
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

#else

		public float horizontalMouseSensitivity
		{
			get { return GetFloat ("is.horizontalSensitivity", m_MouseSensitivityH); }
			set
			{
				SetFloat ("is.horizontalSensitivity", value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public float verticalMouseSensitivity
		{
			get { return GetFloat ("is.verticalSensitivity", m_MouseSensitivityV); }
			set 
			{
				SetFloat ("is.verticalSensitivity", value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public bool invertMouse
		{
			get { return GetBool ("is.invertMouse", m_InvertMouse); }
			set 
			{
				SetBool ("is.invertMouse", value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public bool enableMouseSmoothing
		{
			get { return GetBool ("is.enableMouseSmoothing", m_EnableMouseSmoothing); }
			set
			{
				SetBool ("is.enableMouseSmoothing", value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public float mouseSmoothing
		{
			get { return GetFloat ("is.mouseSmoothing", m_MouseSmoothing); }
			set
			{
				SetFloat ("is.mouseSmoothing", value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public bool enableMouseAcceleration
		{
			get { return GetBool ("is.enableMouseAcceleration", m_EnableMouseAcceleration); }
			set
			{
				SetBool ("is.enableMouseAcceleration", value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public float mouseAcceleration
		{
			get { return GetFloat ("is.mouseAcceleration", m_MouseAcceleration); }
			set
			{
				SetFloat ("is.mouseAcceleration", value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

#endif
	}
}

