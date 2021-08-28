using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.ModularFirearms;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-mb-fpsinventoryammo.html")]
	public class FpsInventoryAmmo : FpsInventoryItemBase
	{
        [Header("Ammo Properties")]

		[SerializeField, Tooltip("The type of ammo.")]
		private SharedAmmoType m_AmmoType = null;

        public override int itemIdentifier
		{
			get
			{
				if (m_AmmoType != null)
					return m_AmmoType.itemIdentifier;
				else
					return 0;
			}
		}

		public override int maxQuantity
		{
			get
			{
				if (m_AmmoType != null)
					return m_AmmoType.maxQuantity;
				else
					return 1;
			}
		}
	}
}