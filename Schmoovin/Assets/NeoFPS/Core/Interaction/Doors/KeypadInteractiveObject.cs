using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.Samples;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-keypadinteractiveobject.html")]
    public class KeypadInteractiveObject : InteractiveObject
    {
		[SerializeField, Tooltip("The door to open (will accept any door that inherits from `DoorBase`).")]
        private DoorBase m_Door = null;

        [SerializeField, Tooltip("The keypad popup to show.")]
        private KeypadPopup m_KeypadPopup = null;

        [SerializeField, Tooltip("The passcode of the door.")]
        private int[] m_PassCode = { 4, 5, 1 };

        [SerializeField, Tooltip("A unique ID for this lock. If the player knows this lock code then the digits will be shown with the popup.")]
        private string m_LockID = "demo_lock";

        [SerializeField, Tooltip("Should the door be locked on start.")]
        private bool m_StartLocked = true;

        protected override void Awake()
        {
            base.Awake();

            if (m_Door == null)
            {
                interactable = false;
            }
            else
            {
                m_Door.onIsLockedChanged += OnDoorIsLockedChanged;

                if (m_StartLocked)
                    m_Door.LockSilent();

                OnDoorIsLockedChanged();
            }
        }

        void OnDestroy()
        {
            if (m_Door != null)
                m_Door.onIsLockedChanged -= OnDoorIsLockedChanged;
        }

        void OnDoorIsLockedChanged()
        {
            interactable = m_Door.isLocked;
        }

        void UnlockDoor()
        {
            m_Door.Unlock();
        }

        public override void Interact(ICharacter character)
        {
            base.Interact(character);
            
            if (m_Door.isLocked)
            {
                // Check if the keycode is already known
                bool known = false;
                var inventory = character.GetComponent<IInventory>();
                if (inventory != null)
                {
                    var keyring = inventory.GetItem(FpsInventoryKey.KeyRing) as KeyRing;
                    if (keyring != null)
                        known = keyring.ContainsKey(m_LockID);
                }

                // Show the popup
                var popup = PrefabPopupContainer.ShowPrefabPopup(m_KeypadPopup);
                popup.Initialise(m_PassCode, UnlockDoor, null, known);
            }
        }
    }
}