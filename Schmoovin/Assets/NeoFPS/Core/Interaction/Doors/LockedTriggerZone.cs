using NeoFPS.Constants;
using NeoFPS.SinglePlayer;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-lockedtriggerzone.html")]
	public class LockedTriggerZone : MonoBehaviour
	{
		[SerializeField, Tooltip("A unique ID for this lock. The player must have an equivalent key in their inventory to unlock.")]
		private string m_LockID = "demo_lock";

		[SerializeField, Tooltip("The event that is fired when a character enters the trigger collider.")]
		private CharacterEvent m_OnTriggerEnter = new CharacterEvent();

		[SerializeField, Tooltip("The event that is fired when a character exits the trigger collider.")]
		private CharacterEvent m_OnTriggerExit = new CharacterEvent();

		[Serializable]
		public class CharacterEvent : UnityEvent<BaseCharacter> { }

		private BaseCharacter m_Character = null;

		void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player"))
			{
				BaseCharacter c = other.GetComponentInParent<BaseCharacter>();
				if (c != null)
				{
					var inventory = c.GetComponent<IInventory>();
					if (inventory != null)
					{
						var keyRing = inventory.GetItem(FpsInventoryKey.KeyRing) as IKeyRing;
						if (keyRing != null && keyRing.ContainsKey(m_LockID))
                        {
							m_Character = c;
							OnCharacterEntered(c);
						}
					}
				}
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Player"))
			{
				BaseCharacter c = other.GetComponentInParent<BaseCharacter>();
				if (c != null && c == m_Character)
					OnCharacterExited(c);
			}
		}

		protected virtual void OnCharacterEntered(BaseCharacter c)
		{
			if (m_OnTriggerEnter != null)
				m_OnTriggerEnter.Invoke(c);
		}

		protected virtual void OnCharacterExited(BaseCharacter c)
		{
			if (m_OnTriggerExit != null)
				m_OnTriggerExit.Invoke(c);
		}
	}
}