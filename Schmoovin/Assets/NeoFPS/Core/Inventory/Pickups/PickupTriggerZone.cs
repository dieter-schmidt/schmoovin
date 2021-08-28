using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-mb-pickuptriggerzone.html")]
	public class PickupTriggerZone : MonoBehaviour
	{
		IPickup m_Pickup = null;

        void Awake ()
		{
			m_Pickup = GetComponent<IPickup>();
			if (m_Pickup == null)
				Debug.LogError ("ZonePickupTrigger requires IPickup inherited behaviour attached to game object");
			
			Collider c = GetComponent<Collider> ();
			c.isTrigger = true;
			gameObject.layer = PhysicsFilter.LayerIndex.TriggerZones;
		}

		void OnTriggerEnter (Collider other)
		{
			ICharacter character = other.GetComponent<ICharacter>();
			if (character != null)
				m_Pickup.Trigger (character);
		}
	}
}