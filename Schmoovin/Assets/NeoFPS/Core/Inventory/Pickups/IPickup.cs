using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
	// NB: Does not actally handle trigger detection, etc. That should be handled by a separate behaviour due to multiple types
	// (OnTrigger, Interact, uNetServer/Client, etc)
	public interface IPickup
	{
		void Trigger (ICharacter character);
	}
}