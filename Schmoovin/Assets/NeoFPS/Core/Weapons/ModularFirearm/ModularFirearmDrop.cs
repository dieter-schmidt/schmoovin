using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-modularfirearmdrop.html")]
	public class ModularFirearmDrop : FpsInventoryWieldableDrop
	{
        [SerializeField, Tooltip("The ammo pickup for this weapon's magazine.")]
        private ModularFirearmAmmoPickup m_AmmoPickup = null;

        [SerializeField, Range(0.1f, 2f), Tooltip("The delay from dropping before the ammo pickup becomes active (prevents the dropper from instantly grabbing ammo)")]
        private float m_AmmoPickupDelay = 0.5f;

        private Coroutine m_InitialisationCoroutine = null;

        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_AmmoPickup == null)
                m_AmmoPickup = GetComponentInChildren<ModularFirearmAmmoPickup>();
        }
        #endif

        void Start()
        {
            if (m_InitialisationCoroutine == null)
                m_InitialisationCoroutine = StartCoroutine(Initialise(null));
        }

        public override void Drop(IInventoryItem item, Vector3 position, Vector3 forward, Vector3 velocity)
        {
            base.Drop(item, position, forward, velocity);

            if (m_InitialisationCoroutine != null)
                StopCoroutine(m_InitialisationCoroutine);
            m_InitialisationCoroutine = StartCoroutine(Initialise(item.GetComponent<IModularFirearm>()));
        }

        IEnumerator Initialise(IModularFirearm firearm)
        {
            float timer = m_AmmoPickupDelay;
            while (pickup.item == null || timer > 0f)
            {
                yield return null;
                timer -= Time.deltaTime;
            }

            if (firearm != null)
            {
                if (m_AmmoPickup != null && firearm != null)
                {
                    m_AmmoPickup.quantity = firearm.reloader.currentMagazine;
                    m_AmmoPickup.EnablePickup(true);
                }
                else
                {
                    var pickupFirearm = pickup.item.GetComponent<ModularFirearm>();
                    if (pickupFirearm != null)
                        pickupFirearm.reloader.startingMagazine = firearm.reloader.currentMagazine;
                }
            }
            else
            {
                if (m_AmmoPickup != null)
                    m_AmmoPickup.EnablePickup(true);
            }

            m_InitialisationCoroutine = null;
        }
    }
}