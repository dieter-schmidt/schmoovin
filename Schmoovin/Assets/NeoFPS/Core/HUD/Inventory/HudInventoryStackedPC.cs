using System.Collections;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudinventorystackedpc.html")]
	[RequireComponent (typeof (CanvasGroup))]
	public class HudInventoryStackedPC : HudInventory
	{
		[SerializeField, Tooltip("A prototype of a single quick-slot stack for duplicating.")]
        private HudInventoryStackedSlot m_StackPrototype = null;

		[SerializeField, Tooltip("The padding transform to pad the layout group and push the item slots together.")]
	    private Transform m_EndPadding = null;

	    private HudInventoryStackedSlot[] m_Stacks = null;
        private int m_SelectedIndex = -1;
	    private int m_StackSize = 0;
	    private int m_NumStacks = 0;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_StackPrototype == null)
                m_StackPrototype = GetComponentInChildren<HudInventoryStackedSlot>();
        }

        protected override void Start()
        {
            base.Start();
            if (m_StackPrototype == null)
                Debug.LogError("Inventory HUD has no slot prototype set up.");
        }

        protected override bool InitialiseSlots()
        {
            if (m_StackPrototype == null)
                return false;

            var castInventory = inventory as FpsInventoryStacked;
            if (castInventory == null)
            {
                Debug.Log ("Inventory won't cast");
                return false;
            }

            m_StackSize = castInventory.maxStackSize;
            m_NumStacks = castInventory.numSlots / m_StackSize;

            // First setup
            if (m_Stacks == null)
            {
                m_Stacks = new HudInventoryStackedSlot[m_NumStacks];

                for (int i = 0; i < m_Stacks.Length; ++i)
                {
                    if (i == 0)
                        m_Stacks[0] = m_StackPrototype;
                    else
                        m_Stacks[i] = Instantiate(m_StackPrototype, m_StackPrototype.transform.parent);
                    m_Stacks[i].Initialise(castInventory, i, persistent);
                }

                // Reset padding (for layout group)
                if (m_EndPadding != null)
                    m_EndPadding.SetAsLastSibling();

                return true;
            }

            // No change
            if (m_Stacks.Length == m_NumStacks)
                return true;
            
            // Reuse old stacks
            var oldStacks = m_Stacks;
            m_Stacks = new HudInventoryStackedSlot[m_NumStacks];

            // Swap old to new 
            int swapped = 0;
            for (; swapped < m_Stacks.Length && swapped < oldStacks.Length; ++swapped)
            {
                m_Stacks[swapped] = oldStacks[swapped];
                m_Stacks[swapped].Initialise(castInventory, swapped, persistent);
            }
            
            // Fill remaining gaps if not enough
            for (int i = swapped; i < m_Stacks.Length; ++i)
            {
                m_Stacks[i] = Instantiate(m_StackPrototype, m_StackPrototype.transform.parent);
                m_Stacks[i].Initialise(castInventory, i, persistent);
            }
            
            // Destroy old if too many
            for (int i = swapped; i < oldStacks.Length; ++i)
                Destroy(oldStacks[i].gameObject);

            // Reset padding (for layout group)
            if (m_EndPadding != null)
                m_EndPadding.SetAsLastSibling();

            return true;
        }

        protected override void SetSlotItem(int slot, IQuickSlotItem item)
        {
            int stackIndex = slot / m_StackSize;
            int itemIndex = slot - (m_StackSize * stackIndex);
            m_Stacks[stackIndex].items[itemIndex].SetItem(item);
        }

        protected override void ClearContents()
        {
            for (int i = 0; i < m_Stacks.Length; ++i)
                m_Stacks[i].ClearStack();
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
            {
                int stackIndex = m_SelectedIndex / m_StackSize;
                int itemIndex = m_SelectedIndex - (m_StackSize * stackIndex);
                m_Stacks[stackIndex].items[itemIndex].selected = false;
                m_Stacks[stackIndex].selected = false;
            }

            // Set new
            m_SelectedIndex = index;
            
            // Select new
            if (m_SelectedIndex != -1)
            {
                int stackIndex = index / m_StackSize;
                int itemIndex = index - (m_StackSize * stackIndex);
                m_Stacks[stackIndex].items[itemIndex].selected = true;
                m_Stacks[stackIndex].RefreshItemOrder ();
                m_Stacks[stackIndex].selected = true;
            }

            base.OnSelectSlot(index);
        }
    }
}