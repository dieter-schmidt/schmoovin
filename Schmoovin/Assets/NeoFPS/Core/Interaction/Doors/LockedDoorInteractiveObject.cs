using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-lockeddoorinteractiveobject.html")]
    public class LockedDoorInteractiveObject : InteractiveObject
    {
		[SerializeField, Tooltip("The door to open (will accept any door that inherits from `DoorBase`).")]
        private DoorBase m_Door = null;

        [SerializeField, Tooltip("A unique ID for this lock. The player must have an equivalent key in their inventory to unlock. If this is empty then the door must be unlocked via events or the API.")]
        private string m_LockID = "demo_lock";

        [SerializeField, Tooltip("Should the door be locked on start.")]
        private bool m_StartLocked = true;

        [SerializeField, Tooltip("Should the door be opened when it's unlocked.")]
        private bool m_OpenOnUnlock = false;

        [SerializeField, Tooltip("The tooltip action to use when the door is locked. Use the open action toolrip for the other tooltip action.")]
        private string m_TooltipLockedAction = "Unlock";

        private string m_TooltipOpenAction = string.Empty;
        private bool m_CanUnlock = true;

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

                m_CanUnlock = !string.IsNullOrWhiteSpace(m_LockID);

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
            if (m_Door.isLocked && m_CanUnlock)
            {
                m_TooltipOpenAction = tooltipAction;
                tooltipAction = m_TooltipLockedAction;
            }
            else
            {
                tooltipAction = m_TooltipOpenAction;
            }
        }

        public override void Interact(ICharacter character)
        {
            base.Interact(character);

            bool open = true;

            if (m_Door.isLocked && m_CanUnlock)
            {
                bool unlock = false;

                var inventory = character.GetComponent<IInventory>();
                if (inventory != null)
                {
                    var keyRing = inventory.GetItem(FpsInventoryKey.KeyRing) as IKeyRing;
                    if (keyRing != null && keyRing.ContainsKey(m_LockID))
                        unlock = true;
                }

                if (unlock)
                {
                    m_Door.Unlock();

                    tooltipAction = m_TooltipOpenAction;

                    if (!m_OpenOnUnlock)
                        open = false;
                }
            }

            if (open)
            {
                if (m_Door.state == DoorState.Closed || m_Door.state == DoorState.Closing)
                    m_Door.Open(m_Door.reversible && !m_Door.IsTransformInFrontOfDoor(character.motionController.localTransform));
                else
                    m_Door.Close();
            }
        }
    }
}