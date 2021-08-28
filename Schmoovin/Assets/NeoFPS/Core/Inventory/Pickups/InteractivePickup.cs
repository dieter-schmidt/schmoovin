using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-mb-interactivepickup.html")]
	[RequireComponent (typeof (AudioSource))]
	public class InteractivePickup : InteractiveObject
    {
		[SerializeField, Tooltip("The root object (destroyed when the item is picked up).")]
        private Transform m_Root = null;

        [SerializeField, Tooltip("The item prefab to add to the character inventory.")]
		private FpsInventoryItemBase m_Item = null;

        private AudioSource m_AudioSource = null;
        private NeoSerializedGameObject m_Nsgo = null;

        private static readonly NeoSerializationKey k_ItemKey = new NeoSerializationKey("item");

        public FpsInventoryItemBase item
        {
            get;
            private set;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
		{
            base.OnValidate();
            if (m_Root == null)
            {
                Transform itr = transform;
                while (itr.parent != null)
                    itr = itr.parent;
                m_Root = itr;
            }
        }
		#endif

        protected override void Awake ()
		{
            base.Awake();

            m_Nsgo = GetComponent<NeoSerializedGameObject>();
            m_AudioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (item == null)
            {
                // Instantiate from prefab if not in scene
                if (m_Item.gameObject.scene.rootCount == 0)
                {
                    if (m_Nsgo != null)
                        item = m_Nsgo.InstantiatePrefab(m_Item, Vector3.zero, Quaternion.identity);
                    else
                        item = Instantiate(m_Item, Vector3.zero, Quaternion.identity, transform);
                    item.gameObject.SetActive(false);
                    item.transform.localScale = Vector3.one;
                }
                else
                    item = m_Item;
            }
            else
                item.quantity = m_Item.quantity;
        }

        public override void Interact (ICharacter character)
		{
			base.Interact (character);

			IInventory inventory = character.inventory;
            if (inventory != null)
            {
                switch (inventory.AddItem(item))
                {
                    case InventoryAddResult.Full:
                        OnPickedUp();
                        break;
                    case InventoryAddResult.AppendedFull:
                        OnPickedUp();
                        break;
                    case InventoryAddResult.Partial:
                        OnPickedUpPartial();
                        break;
                }
            }                
		}

		protected virtual void OnPickedUp ()
        {
            // NB: The item will have been moved into the inventory heirarchy
			if (m_AudioSource != null && m_AudioSource.clip != null)
                AudioSource.PlayClipAtPoint(m_AudioSource.clip, transform.position);

            var pooled = m_Root.GetComponent<PooledObject>();
            if (pooled != null)
                pooled.ReturnToPool();
            else
                Destroy(m_Root.gameObject);
		}

		protected virtual void OnPickedUpPartial ()
		{
			m_AudioSource.Play ();
		}

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);
            writer.WriteComponentReference(k_ItemKey, item, nsgo);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);
            FpsInventoryItemBase result = null;
            if (reader.TryReadComponentReference(k_ItemKey, out result, nsgo))
                item = result;
        }
    }
}