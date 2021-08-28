using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class PlatformDependentObject : MonoBehaviour
    {
        [SerializeField] private bool m_StandaloneState = true;
        [SerializeField] private bool m_WebGLState = true;
        [SerializeField] private bool m_ConsoleState = true;

        enum PlatformGroup
        {
            Other,
            Standalone,
            WebGL,
            Console
        }

        private void Awake()
        {
            switch (GetPlatformGroup())
            {
                case PlatformGroup.Standalone:
                    gameObject.SetActive(m_StandaloneState);
                    break;
                case PlatformGroup.WebGL:
                    gameObject.SetActive(m_WebGLState);
                    break;
                case PlatformGroup.Console:
                    gameObject.SetActive(m_ConsoleState);
                    break;
            }
        }

        private PlatformGroup GetPlatformGroup()
        {
            var p = Application.platform;

            // Check for standalone
            if (p == RuntimePlatform.WindowsPlayer || p == RuntimePlatform.WindowsEditor ||
                p == RuntimePlatform.OSXPlayer || p == RuntimePlatform.OSXEditor ||
                p == RuntimePlatform.LinuxPlayer || p == RuntimePlatform.LinuxEditor)
                return PlatformGroup.Standalone;

            // Check for WebGL
            if (p == RuntimePlatform.WebGLPlayer)
                return PlatformGroup.WebGL;

            // Check for console
            if (p == RuntimePlatform.PS4 || p == RuntimePlatform.XboxOne || p == RuntimePlatform.Switch)
                return PlatformGroup.Console;

            return PlatformGroup.Other;
        }
    }
}