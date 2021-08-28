using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-charactertriggerzonepersistant.html")]
	public class CharacterTriggerZonePersistant : MonoBehaviour
	{
		public event UnityAction<ICharacter> onTriggerEnter;
		public event UnityAction<ICharacter> onTriggerExit;
        public event UnityAction<ICharacter> onTriggerStay;

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

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                ICharacter c = other.GetComponentInParent<ICharacter>();
                if (c != null)
					OnCharacterStay(c);
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

		protected virtual void OnCharacterStay (ICharacter c)
        {
            if (onTriggerStay != null)
                onTriggerStay.Invoke(c);
        }
    }
}