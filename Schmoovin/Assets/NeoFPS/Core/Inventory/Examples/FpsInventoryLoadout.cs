using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-so-fpsinventoryloadout.html")]
    [CreateAssetMenu(fileName = "FpsInventoryLoadout", menuName = "NeoFPS/Inventory/Loadout", order = NeoFpsMenuPriorities.inventory_database)]
    public class FpsInventoryLoadout : ScriptableObject
    {
        [SerializeField, Tooltip("The items the character inventory should contain on spawn")]
        private FpsInventoryItemBase[] m_Items = null;

        public FpsInventoryItemBase[] items
        {
            get { return m_Items; }
        }
    }
}
