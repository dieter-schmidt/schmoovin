using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-pickablelockeddoorinteractiveobject.html")]
    public class PickableLockedDoorInteractiveObject : InteractiveObject
    {
		[SerializeField, Tooltip("The door to open (will accept any door that inherits from `DoorBase`).")]
        private DoorBase m_Door = null;

        [SerializeField, Tooltip("A unique ID for this lock. The player must have an equivalent key in their inventory to unlock.")]
        private string m_LockID = "demo_lock";

        [SerializeField, Tooltip("The ID for the lockpick to use. This allows for multiple lockpick minigame styles in a single scene.")]
        private string m_LockpickID = "default";

        [SerializeField, Tooltip("The difficulty of this specific lock.")]
        private float m_LockpickDifficulty = 0.5f;

        [SerializeField, Tooltip("Does the character require a lockpick item in their inventory.")]
        private bool m_RequiresPickItem = true;

        [SerializeField, Tooltip("Should the door be locked on start.")]
        private bool m_StartLocked = true;

        [SerializeField, Tooltip("Should the door be opened when it's unlocked.")]
        private bool m_OpenOnUnlock = false;

        [SerializeField, Tooltip("Should the door be locked on start.")]
        private string m_TooltipLockedAction = "Unlock";

        private string m_TooltipOpenAction = string.Empty;

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
            if (m_Door.isLocked)
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

            if (m_Door.isLocked)
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
                else
                {
                    // Check for lockpick item if required
                    bool pick = false;
                    if (m_RequiresPickItem && inventory != null)
                    {
                        var lockPickItem = inventory.GetItem(FpsInventoryKey.Lockpick);
                        if (lockPickItem != null)
                            pick = true;
                    }

                    // Open lockpick popup
                    if (pick && LockpickPopup.ShowLockpickPopup(m_LockpickID, GetLockpickDifficulty(), character, LockPickSuccess, LockPickCancelled))
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

        protected virtual float GetLockpickDifficulty()
        {
            return m_LockpickDifficulty;
        }

        void LockPickSuccess()
        {
            m_Door.Unlock();
            tooltipAction = m_TooltipOpenAction;
        }

        void LockPickCancelled()
        {
        }
    }
}