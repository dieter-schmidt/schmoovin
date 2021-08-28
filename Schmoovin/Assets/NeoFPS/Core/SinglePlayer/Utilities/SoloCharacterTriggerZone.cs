using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.SinglePlayer
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-solocharactertriggerzone.html")]
	public class SoloCharacterTriggerZone : MonoBehaviour
	{
        [SerializeField, Tooltip("The event that is fired when a character enters the trigger collider.")]
        private CharacterEvent m_OnTriggerEnter = new CharacterEvent();

        [SerializeField, Tooltip("The event that is fired when a character exits the trigger collider.")]
        private CharacterEvent m_OnTriggerExit = new CharacterEvent();
		
		[Serializable]
		public class CharacterEvent : UnityEvent<FpsSoloCharacter> {}

		void OnTriggerEnter (Collider other)
		{
			if (other.CompareTag ("Player"))
			{
				FpsSoloCharacter c = other.GetComponentInParent<FpsSoloCharacter>();
				if (c != null)
					OnCharacterEntered(c);
			}
		}

		void OnTriggerExit (Collider other)
		{
			if (other.CompareTag ("Player"))
			{
				FpsSoloCharacter c = other.GetComponentInParent<FpsSoloCharacter>();
				if (c != null)
					OnCharacterExited(c);
			}
		}

		protected virtual void OnCharacterEntered(FpsSoloCharacter c)
		{
			if (m_OnTriggerEnter != null)
				m_OnTriggerEnter.Invoke (c);
		}

		protected virtual void OnCharacterExited(FpsSoloCharacter c)
		{
			if (m_OnTriggerExit != null)
				m_OnTriggerExit.Invoke (c);
		}
    }
}