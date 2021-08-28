#if UNITY_STANDALONE 
// Should other platforms use Json text files saved to disk?
#define SETTINGS_USES_JSON
using System.IO;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/neofpsref-mb-fpsgamepadsettings.html")]
    [CreateAssetMenu(fileName = "FpsSettings_Gamepad", menuName = "NeoFPS/Settings/Gamepad", order = NeoFpsMenuPriorities.settings_gamepad)]
    public class FpsGamepadSettings : SettingsContext<FpsGamepadSettings>
    {
        protected override string contextName { get { return "GamepadSettings"; } }

        public override string displayTitle { get { return "NeoFPS Gamepad Settings"; } }

        public override string tocName { get { return "Gamepad Settings"; } }

        public override string tocID { get { return "settings_gamepad"; } }

        [SerializeField, Tooltip("Should gamepad input be registered.")]
        private bool m_UseGamepad = true;

        [SerializeField, Tooltip("The gamepad profile to use.")]
        private int m_ProfileIndex = 0;

        [SerializeField, Tooltip("The horizontal gamepad aim sensitivity.")]
        private float m_AnalogSensitivityH = 0.75f;

        [SerializeField, Tooltip("The vertical gamepad aim sensitivity.")]
        private float m_AnalogSensitivityV = 0.75f;

        [SerializeField, Tooltip("Invert the gammepad vertical aim.")]
        private bool m_InvertLook = false;

        public event UnityAction onSettingsChanged;

        private void OnValidate()
        {
            m_AnalogSensitivityH = Mathf.Clamp(m_AnalogSensitivityH, 0.1f, 5f);
            m_AnalogSensitivityV = Mathf.Clamp(m_AnalogSensitivityV, 0.1f, 5f);
        }

#if !NEOFPS_LOAD_ON_DEMAND
        public override void OnLoad()
        {
            // Clamp profiles to available
            m_ProfileIndex = Mathf.Clamp(m_ProfileIndex, 0, NeoFpsInputManager.gamepadProfiles.Length);
        }
#endif

        protected override bool CheckIfCurrent()
        {
            return FpsSettings.gamepad == this;
        }

#if SETTINGS_USES_JSON

        public bool useGamepad
        {
            get { return m_UseGamepad; }
            set
            {
                SetValue(ref m_UseGamepad, value);
                if (onSettingsChanged != null)
                    onSettingsChanged();
            }
        }

        public int profile
        {
#if NEOFPS_LOAD_ON_DEMAND
            get { return Mathf.Clamp(m_ProfileIndex, 0, NeoFpsInputManager.gamepadProfiles.Length); }
#else
            get { return m_ProfileIndex; }
#endif
            set
            {
                SetValue(ref m_ProfileIndex, Mathf.Clamp(value, 0, NeoFpsInputManager.gamepadProfiles.Length));
                if (onSettingsChanged != null)
                    onSettingsChanged();
            }
        }

        public bool invertLook
        {
            get { return m_InvertLook; }
            set
            {
                SetValue(ref m_InvertLook, value);
                if (onSettingsChanged != null)
                    onSettingsChanged();
            }
        }

        public float horizontalAnalogSensitivity
        {
            get { return m_AnalogSensitivityH; }
            set
            {
                SetValue(ref m_AnalogSensitivityH, Mathf.Clamp(value, 0.1f, 5f));
                if (onSettingsChanged != null)
                    onSettingsChanged();
            }
        }

        public float verticalAnalogSensitivity
        {
            get { return m_AnalogSensitivityV; }
            set
            {
                SetValue(ref m_AnalogSensitivityV, Mathf.Clamp(value, 0.1f, 5f));
                if (onSettingsChanged != null)
                    onSettingsChanged();
            }
        }

#else

        public bool useGamepad
        {
            get { return GetBool ("gpad.usegpad", m_UseGamepad); }
			set
            {
                SetBool("gpad.usegpad", value);
                if (onSettingsChanged != null)
                    onSettingsChanged();
            }
        }
        
        public int profile
        {
            get { return Mathf.Clamp (GetInt ("gpad.profile", m_ProfileIndex), 0, NeoFpsInputManager.gamepadProfiles.Length); }
			set
            {
                SetInt("gpad.profile", value);
                if (onSettingsChanged != null)
                    onSettingsChanged();
            }
        }
        
        public bool invertLook
        {
            get { return GetBool ("gpad.invertLook", m_InvertLook); }
			set
            {
                SetBool("gpad.invertLook", value);
                if (onSettingsChanged != null)
                    onSettingsChanged();
            }
        }
        
        public float horizontalAnalogSensitivity
        {
            get { return GetFloat("gpad.analogSensitivityH", m_AnalogSensitivityH); }
            set
            {
                SetFloat("gpad.analogSensitivityH", value);
                if (onSettingsChanged != null)
                    onSettingsChanged();
            }
        }

        public float verticalAnalogSensitivity
        {
            get { return GetFloat("gpad.analogSensitivityV", m_AnalogSensitivityV); }
            set
            {
                SetFloat("gpad.analogSensitivityV", value);
                if (onSettingsChanged != null)
                    onSettingsChanged();
            }
        }

#endif
        }
}

