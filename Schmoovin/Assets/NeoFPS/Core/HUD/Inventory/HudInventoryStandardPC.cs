using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudinventorystandardpc.html")]
	[RequireComponent (typeof (CanvasGroup))]
	public class HudInventoryStandardPC : HudInventory
    {
		[SerializeField, Tooltip("A prototype of a single quick-slot item for duplicating.")]
        private HudInventoryItemStandard m_ItemPrototype = null;

		[SerializeField, Tooltip("The padding transform to pad the layout group and push the item slots together.")]
        private Transform m_EndPadding = null;

        private HudInventoryItemStandard[] m_Slots = null;
        private int m_SelectedIndex = -1;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_ItemPrototype == null)
                m_ItemPrototype = GetComponentInChildren<HudInventoryItemStandard>();
        }

        protected override void Start()
        {
            base.Start();
            if (m_ItemPrototype == null)
                Debug.LogError("Inventory HUD has no slot prototype set up.");
        }

        protected override bool InitialiseSlots()
        {
			if (m_ItemPrototype == null)
                return false;

            if (m_Slots == null)
            {
                m_Slots = new HudInventoryItemStandard[inventory.numSlots];

                m_Slots[0] = m_ItemPrototype;
                m_Slots[0].slotIndex = 0;
                for (int i = 1; i < m_Slots.Length; ++i)
                {
                    m_Slots[i] = Instantiate (m_ItemPrototype, m_ItemPrototype.transform.parent);
                    m_Slots[i].slotIndex = i;
                }

                // Reset padding (for layout group)
                if (m_EndPadding != null)
                        m_EndPadding.SetAsLastSibling();

                return true;
            }

            if (m_Slots.Length != inventory.numSlots)
            {
                HudInventoryItemStandard[] oldSlots = m_Slots;
                m_Slots = new HudInventoryItemStandard[inventory.numSlots];

                m_Slots[0] = m_ItemPrototype;

                int swapped = 1;
                for (; swapped < m_Slots.Length && swapped < oldSlots.Length; ++swapped)
                    m_Slots[swapped] = oldSlots[swapped];
                for (int i = swapped; i < m_Slots.Length; ++i)
                {
                    m_Slots[i] = Instantiate (m_ItemPrototype, m_ItemPrototype.transform.parent);
                    m_Slots[i].slotIndex = i;
                }
                for (int i = swapped; i < oldSlots.Length; ++i)
                    Destroy(oldSlots[i].gameObject);

                // Reset padding (for layout group)
                if (m_EndPadding != null)
                        m_EndPadding.SetAsLastSibling();

                return true;
            }

            return true;
        }

        protected override void SetSlotItem(int slot, IQuickSlotItem item)
        {
            m_Slots[slot].SetItem(item);
        }

        protected override void ClearContents()
        {
            for (int i = 0; i < m_Slots.Length; ++i)
            {
                m_Slots[i].SetItem(null);
                m_Slots[i].selected = false;
            }
            m_SelectedIndex = -1;
            TriggerTimeout();
        }

        protected override void OnSelectSlot(int index)
        {
            // Check if it's an actual change
            if (index == m_SelectedIndex)
                return;

            // Deselect old
            if (m_SelectedIndex != -1)
                m_Slots[m_SelectedIndex].selected = false;

            // Set new
            m_SelectedIndex = index;

            // Select new
            if (m_SelectedIndex != -1)
                m_Slots[m_SelectedIndex].selected = true;

            base.OnSelectSlot(index);
        }
    }
}