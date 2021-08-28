using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Serialization;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-mb-fpsinventoryitem.html")]
	public class FpsInventoryItem : FpsInventoryItemBase
    {
        [SerializeField, HideInInspector]
        private FpsInventoryKey m_ItemKey = FpsInventoryKey.Undefined;

        [SerializeField, FpsInventoryKey, Tooltip("The item key for this object.")]
        private int m_InventoryID = 0;

        [SerializeField, Tooltip("The maximum amount of this item that the inventory can contain.")]
        private int m_MaxQuantity = 1;

        public override int itemIdentifier
        {
            get { return CheckID(); }
        }

        public override int maxQuantity
        {
            get { return m_MaxQuantity; }
        }

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

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();

            if (m_MaxQuantity < 1)
                m_MaxQuantity = 1;

            CheckID();
        }

#endif
    }
}