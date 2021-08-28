using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-mb-keyringpickup.html")]
    [RequireComponent(typeof(AudioSource))]
    public class KeyRingPickup : Pickup
    {
        [SerializeField, Tooltip("The root object (destroyed when the item is picked up).")]
        private Transform m_Root = null;

        [SerializeField, Tooltip("What to do when the object is picked up.")]
        private PickUpAction m_PickUpAction = PickUpAction.Destroy;

        [SerializeField, Tooltip("The keyring prefab to add to the character inventory if not found.")]
        private KeyRing m_KeyRingPrefab = null;

        [SerializeField, Tooltip("The keys contained in this pickup.")]
        private string[] m_KeyCodes = { "demo_key" };

        private AudioSource m_AudioSource = null;

        enum PickUpAction
        {
            Destroy,
            DeactivateObject,
            DisableComponent
        }

        private static readonly NeoSerializationKey k_ItemKey = new NeoSerializationKey("item");
        
#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_Root == null)
            {
                Transform itr = transform;
                while (itr.parent != null)
                    itr = itr.parent;
                m_Root = itr;
            }
        }
#endif

        void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            if (m_KeyRingPrefab == null)
                Debug.LogError("Key ring pickup requires a keyring prefab");
        }

        public override void Trigger(ICharacter character)
        {
            base.Trigger(character);

            IInventory inventory = character.inventory;
            if (inventory != null)
            {
                int count = 0;

                // Get the keyring (merge with this or add if not found)
                var keyring = inventory.GetItem(FpsInventoryKey.KeyRing) as KeyRing;
                if (keyring == null)
                {
                    inventory.AddItemFromPrefab(m_KeyRingPrefab.gameObject);
                    keyring = inventory.GetItem(FpsInventoryKey.KeyRing) as KeyRing;
                    if (keyring == null)
                    {
                        Debug.LogError("Failed to get keyring in inventory");
                        return;
                    }
                    else
                        ++count;
                }
                else
                    keyring.Merge(m_KeyRingPrefab);

                // Add the keys
                for (int i = 0; i < m_KeyCodes.Length; ++i)
                {
                    if (!keyring.ContainsKey(m_KeyCodes[i]))
                    {
                        keyring.AddKey(m_KeyCodes[i]);
                        ++count;
                    }
                }

                // If anything was picked up, destroy and play sound
                if (count > 0)
                {
                    if (m_AudioSource != null && m_AudioSource.clip != null)
                        AudioSource.PlayClipAtPoint(m_AudioSource.clip, transform.position);

                    switch(m_PickUpAction)
                    {
                        case PickUpAction.Destroy:
                            Destroy(m_Root.gameObject);
                            break;
                        case PickUpAction.DeactivateObject:
                            gameObject.SetActive(false);
                            break;
                        case PickUpAction.DisableComponent:
                            enabled = false;
                            break;
                    }
                }
            }
        }
    }
}