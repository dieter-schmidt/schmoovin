using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-modularfirearmammopickup.html")]
    [RequireComponent(typeof(AudioSource))]
    public class ModularFirearmAmmoPickup : Pickup
    {
		[SerializeField, Tooltip("The pickup attached to the weapon drop.")]
        private InteractivePickup m_WeaponPickup = null;

        [SerializeField, Tooltip("The ammo inventory item.")]
        private FpsInventoryAmmo m_AmmoObject = null;

        [SerializeField, Tooltip("The display mesh for the weapon magazine. Will be hidden when the ammo is collected.")]
        private GameObject m_DisplayMesh = null;

        private AudioSource m_AudioSource = null;
        private Collider m_Collider = null;

        public FpsInventoryAmmo ammo
        {
            get { return m_AmmoObject; }
        }

        public int quantity
        {
            get
            {
                if (m_WeaponPickup == null)
                    return 0;
                ModularFirearm firearm = m_WeaponPickup.item.GetComponent<ModularFirearm>();
                if (firearm != null)
                    return firearm.reloader.startingMagazine;
                else
                    return 0;
            }
            set
            {
                if (m_WeaponPickup == null)
                    return;
                ModularFirearm firearm = m_WeaponPickup.item.GetComponent<ModularFirearm>();
                if (firearm != null)
                {
                    firearm.reloader.startingMagazine = value;
                    EnablePickup(value > 0);
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_WeaponPickup == null && transform.parent != null)
                m_WeaponPickup = transform.parent.GetComponentInChildren<InteractivePickup>();
            if (m_AmmoObject == null)
                m_AmmoObject = GetComponentInChildren<FpsInventoryAmmo>();
        }
        #endif

        void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
            m_Collider = GetComponent<Collider>();
            m_Collider.enabled = false;
        }

        public override void Trigger(ICharacter character)
        {
            base.Trigger(character);
            if (m_AmmoObject != null)
            {
                m_AmmoObject.quantity = quantity;
                IInventory inventory = character.inventory;
                switch (inventory.AddItem(m_AmmoObject))
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

        protected virtual void OnPickedUp()
        {
            // NB: The item will have been moved into the inventory heirarchy
            AudioSource.PlayClipAtPoint(m_AudioSource.clip, transform.position);
            quantity = 0;
            m_AmmoObject = null;
            // Disable the mesh
            if (m_DisplayMesh != null)
                m_DisplayMesh.SetActive(false);
        }

        protected virtual void OnPickedUpPartial()
        {
            m_AudioSource.Play();
            quantity = m_AmmoObject.quantity;
        }

        public virtual void EnablePickup(bool value)
        {
            // Enable the mesh
            if (m_DisplayMesh != null)
                m_DisplayMesh.SetActive(value);

            // Enable the collider
            if (value)
                m_Collider.enabled = true;
            else
                m_Collider.enabled = false;

            gameObject.SetActive(value);
        }
    }
}