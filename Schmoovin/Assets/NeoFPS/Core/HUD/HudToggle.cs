using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class HudToggle : MonoBehaviour
    {
        [SerializeField, Tooltip("The keycode to use when toggling the HUD")]
        private KeyCode m_ToggleKey = KeyCode.Quote;

        [SerializeField, Tooltip("The gameobject containing the HUD UI. Must be a child of this object")]
        private GameObject m_HudObject = null;

        private static bool s_HudVisible = true;

        private void OnValidate()
        {
            if (m_HudObject != null && (!m_HudObject.transform.IsChildOf(transform) || m_HudObject.transform == transform))
            {
                Debug.Log("Hud Object should be a child of the Hud Toggle");
                m_HudObject = null;
            }
        }

        private void Start()
        {
            if (m_HudObject != null)
                m_HudObject.SetActive(s_HudVisible);
        }

        private void Update()
        {
            if (Input.GetKeyDown(m_ToggleKey))
            {
                s_HudVisible = !s_HudVisible;
                if (m_HudObject != null)
                    m_HudObject.SetActive(s_HudVisible);
            }
        }
    }
}