using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace NeoFPS
{
	public abstract class Pickup : MonoBehaviour, IPickup
	{
		public event UnityAction<ICharacter, IPickup> onPickupTriggered;

		public virtual void Trigger (ICharacter character)
		{
			if (onPickupTriggered != null)
				onPickupTriggered (character, this);
		}
	}
}