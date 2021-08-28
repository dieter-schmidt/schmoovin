using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-charactertriggerzone.html")]
	public class CharacterTriggerZone : MonoBehaviour
	{
		public event UnityAction<ICharacter> onTriggerEnter;
		public event UnityAction<ICharacter> onTriggerExit;

		void OnTriggerEnter (Collider other)
		{
			if (other.CompareTag ("Player"))
			{
				ICharacter c = other.GetComponentInParent<ICharacter>();
				if (c != null)
					OnCharacterEntered (c);
			}
		}

		void OnTriggerExit (Collider other)
		{
			if (other.CompareTag ("Player"))
			{
				ICharacter c = other.GetComponentInParent<ICharacter>();
				if (c != null)
					OnCharacterExited (c);
			}
		}

		protected virtual void OnCharacterEntered (ICharacter c)
		{
			if (onTriggerEnter != null)
				onTriggerEnter.Invoke (c);
		}

		protected virtual void OnCharacterExited (ICharacter c)
		{
			if (onTriggerExit != null)
				onTriggerExit.Invoke (c);
		}
	}
}