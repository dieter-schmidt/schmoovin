using System;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudhider.html")]
    public class HudHider : MonoBehaviour
    {
        private static HudHider s_Instance = null;

        private bool m_CameraHide = false;
        private bool m_ManualHide = false;

        private void Awake()
        {
            s_Instance = this;

            FirstPersonCamera.onCurrentCameraChanged += OnCurrentCameraChanged;
            OnCurrentCameraChanged(FirstPersonCamera.current);
        }

        private void OnDestroy()
        {
            FirstPersonCamera.onCurrentCameraChanged -= OnCurrentCameraChanged;

            if (s_Instance == this)
                s_Instance = null;
        }

        private void OnCurrentCameraChanged(FirstPersonCamera cam)
        {
            m_CameraHide = (cam == null);
            gameObject.SetActive(!m_CameraHide && !m_ManualHide);
        }

        public static void HideHUD()
        {
            if (s_Instance != null)
            {
                s_Instance.m_ManualHide = true;
                s_Instance.gameObject.SetActive(!s_Instance.m_CameraHide && !s_Instance.m_ManualHide);
            }
        }

        public static void ShowHUD()
        {
            if (s_Instance != null)
            {
                s_Instance.m_ManualHide = false;
                s_Instance.gameObject.SetActive(!s_Instance.m_CameraHide && !s_Instance.m_ManualHide);
            }
        }
    }
}
