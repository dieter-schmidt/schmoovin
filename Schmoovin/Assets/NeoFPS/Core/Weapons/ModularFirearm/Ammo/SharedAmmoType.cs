using UnityEngine;
using NeoFPS.Constants;
using UnityEngine.Serialization;

namespace NeoFPS.ModularFirearms
{
	[CreateAssetMenu(fileName = "AmmoType", menuName = "NeoFPS/Inventory/Ammo Type", order = NeoFpsMenuPriorities.inventory_ammoType)]
	[HelpURL("https://docs.neofps.com/manual/weaponsref-so-sharedammotype.html")]
	public class SharedAmmoType : ScriptableObject
	{
		[SerializeField, HideInInspector]
		private FpsInventoryKey m_ItemKey = FpsInventoryKey.Undefined;

		[SerializeField, FpsInventoryKey, Tooltip("The item key for this ammo type.")]
		private int m_InventoryID = 0;

		[SerializeField, Tooltip("The name to be printed on the HUD.")]
		private string m_PrintableName = string.Empty;

		[SerializeField, Tooltip("The maximum quantity a character can carry.")]
		private int m_MaxQuantity = 150;

#if UNITY_EDITOR

		protected virtual void OnValidate()
		{
			if (m_MaxQuantity < 1)
				m_MaxQuantity = 1;
			
            CheckID();
		}

#endif

		int CheckID()
		{
			if (m_ItemKey != FpsInventoryKey.Undefined)
			{
				if (m_InventoryID == 0)
					m_InventoryID = m_ItemKey;
				m_ItemKey = FpsInventoryKey.Undefined;
			};
			return m_InventoryID;
		}

		public int maxQuantity
		{
			get { return m_MaxQuantity; }
		}

		public int itemIdentifier
		{
			get { return CheckID(); }
		}

		public string printableName
		{
			get { return m_PrintableName; }
		}
	}
}