using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-mb-interactivemultipickup.html")]
	[RequireComponent (typeof (AudioSource))]
	public class InteractiveMultiPickup : InteractiveObject
    {
        [SerializeField, Tooltip("The item prefabs to add to the character inventory.")]
        private FpsInventoryItemBase[] m_Items = new FpsInventoryItemBase[0];

        [SerializeField, Tooltip("Do the inventory items replenish. If the items are removed new items will be instantiated. If they are partially removed (to top up an existing item) the quantity will be reset afterwards.")]
        private bool m_Replenish = true;
		
		private AudioSource m_AudioSource = null;
        private NeoSerializedGameObject m_Nsgo = null;

        public FpsInventoryItemBase[] items
        {
            get;
            private set;
        }

        protected override void Awake ()
		{
            base.Awake();
            
            m_Nsgo = GetComponent<NeoSerializedGameObject>();
            m_AudioSource = GetComponent<AudioSource>();
        }

        protected override void Start()
        {
            base.Start();

            items = new FpsInventoryItemBase[m_Items.Length];
            for (int i = 0; i < m_Items.Length; ++i)
            {
                // Unlike interactable pickup, always instantiate as it needs
                // to duplicate when repleneshing
                items[i] = InstantiateItem(m_Items[i]);
            }
        }

        public override void Interact (ICharacter character)
		{
			base.Interact (character);
            IInventory inventory = character.inventory;

            int full = 0;
            int partial = 0;
            for (int i = 0; i < items.Length; ++i)
            {
				if (items[i] == null)
                    continue;

                int oldQuantity = items[i].quantity;
                switch (inventory.AddItem(items[i]))
                {
                    case InventoryAddResult.Full:
                        {
                            if (m_Replenish)
                                items[i] = InstantiateItem(m_Items[i]);
                            else
                                items[i] = null;
                            ++full;
                        }
                        break;
                    case InventoryAddResult.AppendedFull:
                        {
                            if (m_Replenish)
                                items[i].quantity = oldQuantity;
                            ++full;
                        }
                        break;
                    case InventoryAddResult.Partial:
                        {
                            if (m_Replenish)
                                items[i].quantity = oldQuantity;
                            ++partial;
                        }
                        break;
                }
            }

            if (full > 0 || partial > 0)
                OnPickedUp(full, partial);
        }

        FpsInventoryItemBase InstantiateItem(FpsInventoryItemBase original)
        {
			if (original == null)
                return null;

            FpsInventoryItemBase result = (m_Nsgo != null) ?
                m_Nsgo.InstantiatePrefab(original, Vector3.zero, Quaternion.identity) :
                Instantiate(original, Vector3.zero, Quaternion.identity, transform);
            result.gameObject.SetActive(false);
            result.transform.localScale = Vector3.one;

            return result;
        }

        protected virtual void OnPickedUp(int full, int partial)
        {
            if (m_AudioSource != null)
                m_AudioSource.Play();
        }
    }
}