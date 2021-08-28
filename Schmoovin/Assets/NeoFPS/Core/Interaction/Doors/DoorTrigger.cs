using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-doortrigger.html")]
    public class DoorTrigger : MonoBehaviour
    {
		[SerializeField, Tooltip("The door to open.")]
        private DoorBase m_Door = null;
        [SerializeField, Tooltip("Should the door only open for characters, not any collider.")]
        private bool m_CharactersOnly = true;

        int m_Count = 0;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_Door == null)
                m_Door = GetComponentInParent<DoorBase>();
        }
#endif

		void OnTriggerEnter(Collider other)
        {
            if (!m_CharactersOnly || other.CompareTag("Player"))
            {
                ++m_Count;
                if (m_Count == 1 && m_Door != null)
                    m_Door.Open();
            }
        }

		void OnTriggerExit(Collider other)
        {
            if (!m_CharactersOnly || other.CompareTag("Player"))
            {
                --m_Count;
                if (m_Count == 0 && m_Door != null)
                    m_Door.Close();
            }
        }
    }
}