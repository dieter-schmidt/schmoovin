using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-thrownweapon.html")]
    public class ThrownWeapon : BaseThrownWeapon
    {
        private IInventoryItem m_InventoryItem = null;

        protected override void Awake()
        {
            m_InventoryItem = GetComponent<IInventoryItem>();
            base.Awake();
        }

        protected override void DecrementQuantity()
        {
            if (m_InventoryItem != null)
                --m_InventoryItem.quantity;
        }
    }
}