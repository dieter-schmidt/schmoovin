using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inputref-mb-inputinventory.html")]
    [RequireComponent(typeof(ICharacter))]
    public class InputInventory : CharacterInputBase
    {
        [Header("Input Properties")]

        [SerializeField, Range(0.1f, 1f), Tooltip("The delay between repeating input when holding the next or previous weapon buttons.")]
        private float m_RepeatDelay = 0.25f;

        [SerializeField, Range(0.01f, 1f), Tooltip("The delay between repeating input when rolling the mouse scroll wheel.")]
        private float m_ScrollDelay = 0.1f;

        private float m_WeaponCycleTimeout = 0f;
        private float m_ScrollTimer = 0f;

        protected override void UpdateInput()
        {
            // Switch weapons			
            if (m_Character.quickSlots != null)
            {
                int weaponCycle = 0;
                if (m_WeaponCycleTimeout == 0f)
                {
                    // Get cycle direction
                    if (GetButton(FpsInputButton.PrevWeapon))
                        weaponCycle -= 1;
                    if (GetButton(FpsInputButton.NextWeapon))
                        weaponCycle += 1;

                    // Cycle weapon
                    switch (weaponCycle)
                    {
                        case 1:
                            {
                                m_Character.quickSlots.SelectNextSlot();
                                m_WeaponCycleTimeout = m_RepeatDelay;
                                break;
                            }
                        case -1:
                            {
                                m_Character.quickSlots.SelectPreviousSlot();
                                m_WeaponCycleTimeout = m_RepeatDelay;
                                break;
                            }
                    }
                }
                else
                {
                    // Get cycle direction
                    if (GetButtonUp(FpsInputButton.PrevWeapon) || GetButtonUp(FpsInputButton.NextWeapon))
                        m_WeaponCycleTimeout = 0f;
                    else
                    {
                        // Reduce repeat timeout
                        m_WeaponCycleTimeout -= Time.deltaTime;
                        if (m_WeaponCycleTimeout < 0f)
                            m_WeaponCycleTimeout = 0f;
                    }
                }

                // Quick-switch
                if (GetButtonDown(FpsInputButton.SwitchWeapon))
                    m_Character.quickSlots.SwitchSelection();

                // Quickslots
                if (GetButtonDown(FpsInputButton.Quickslot1))
                    m_Character.quickSlots.SelectSlot(0);
                if (GetButtonDown(FpsInputButton.Quickslot2))
                    m_Character.quickSlots.SelectSlot(1);
                if (GetButtonDown(FpsInputButton.Quickslot3))
                    m_Character.quickSlots.SelectSlot(2);
                if (GetButtonDown(FpsInputButton.Quickslot4))
                    m_Character.quickSlots.SelectSlot(3);
                if (GetButtonDown(FpsInputButton.Quickslot5))
                    m_Character.quickSlots.SelectSlot(4);
                if (GetButtonDown(FpsInputButton.Quickslot6))
                    m_Character.quickSlots.SelectSlot(5);
                if (GetButtonDown(FpsInputButton.Quickslot7))
                    m_Character.quickSlots.SelectSlot(6);
                if (GetButtonDown(FpsInputButton.Quickslot8))
                    m_Character.quickSlots.SelectSlot(7);
                if (GetButtonDown(FpsInputButton.Quickslot9))
                    m_Character.quickSlots.SelectSlot(8);
                if (GetButtonDown(FpsInputButton.Quickslot10))
                    m_Character.quickSlots.SelectSlot(9);

                // Holster
                if (GetButtonDown(FpsInputButton.Holster))
                    m_Character.quickSlots.SelectSlot(-1);

                // Drop selected weapon
                if (GetButtonDown(FpsInputButton.DropWeapon))
                    m_Character.quickSlots.DropSelected();

                // Mouse scroll
                if (m_ScrollTimer == 0f)
                {
                    float scroll = GetAxis(FpsInputAxis.MouseScroll);
                    if (scroll > 0.075f)
                    {
                        m_Character.quickSlots.SelectNextSlot();
                        m_ScrollTimer = m_ScrollDelay;
                    }
                    if (scroll < -0.075f)
                    {
                        m_Character.quickSlots.SelectPreviousSlot();
                        m_ScrollTimer = m_ScrollDelay;
                    }
                }
                else
                {
                    m_ScrollTimer -= Time.unscaledDeltaTime;
                    if (m_ScrollTimer < 0f)
                        m_ScrollTimer = 0f;
                }
            }
        }
    }
}