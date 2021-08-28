using UnityEngine;
using UnityEngine.Events;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using System.Collections.Generic;
using System;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-mb-fpsinventoryquickswitch.html")]
	public class FpsInventoryQuickSwitch : FpsInventoryBase
	{
        [SerializeField, Tooltip("What to do when trying to add an item to the inventory that already exists.")]
        private DuplicateEntryBehaviour m_DuplicateBehaviour = DuplicateEntryBehaviour.Reject;

        private static readonly NeoSerializationKey k_HistoryKey = new NeoSerializationKey("history");

        private Dictionary<int, FpsInventoryItemBase> m_Items = null;
        private int[] m_History = null;
        private IQuickSlotItem[] m_Slots = null;
        private bool m_Initialised = false;

		protected override DuplicateEntryBehaviour duplicateBehaviour
		{
			get { return m_DuplicateBehaviour; }
		}

		protected override void Awake ()
		{
			base.Awake ();
            InitialiseArrays();
        }

        void InitialiseArrays()
        {
            if (!m_Initialised)
            {
				m_Items = new Dictionary<int, FpsInventoryItemBase>();
                m_Slots = new IQuickSlotItem[numSlots];

                // Assign history and populate (swapped on start)
                m_History = GetStartingSlotOrder();

                m_Initialised = true;
            }
        }

		protected override void AddItemReference(FpsInventoryItemBase item)
		{
			m_Items.Add(item.itemIdentifier, item);
		}

		protected override void RemoveItemReference(FpsInventoryItemBase item)
		{
			m_Items.Remove(item.itemIdentifier);
		}

		public override void ClearAllItems(UnityAction<IInventoryItem> onClearAction)
		{
			foreach (var item in m_Items.Values)
			{
				if (item != null)
				{
					// Callbacks
					item.OnRemoveFromInventory ();
					OnItemRemoved (item);
					if (onClearAction != null)
						onClearAction (item);
				}
			}
			m_Items.Clear();
		}

		public override IInventoryItem GetItem(int itemIdentifier)
		{
			FpsInventoryItemBase result;
			if (m_Items.TryGetValue(itemIdentifier, out result))
				return result;
			else
				return null;
        }

        public override void GetItemsSorted(List<IInventoryItem> output, InventoryCallbacks.FilterItem filter, Comparison<IInventoryItem> compare)
        {
            if (output == null)
                return;

            output.Clear();

            // Get all items (filtered if a filter is provided)
            foreach (var item in m_Items.Values)
            {
                if (item == null)
                    continue;
                if (filter != null && !filter(item))
                    continue;
                output.Add(item);
            }

            // Sort the results if a comparison is provided
            if (compare != null)
                output.Sort(compare);
        }

        public override void SetSlotItem(int slot, IQuickSlotItem item)
		{
            if (slot == -1 || m_Slots[slot] == item)
				return;

            if (slot < -1 || slot >= numSlots)
            {
                Debug.LogError("Attempting to set quickslot item. Slot index out of range (should start at 0).");
                return;
            }
			
			m_Slots [slot] = item;
			OnSlotItemChanged(slot, item);
		}

		public override IQuickSlotItem GetSlotItem(int slot)
		{
			return m_Slots [slot];
		}

		public override void ClearSlots()
		{
			for (int i = 0; i < m_Slots.Length; ++i)
				SetSlotItem (i, null);
		}

		public override bool IsSlotSelectable(int index)
		{
			if (index < 0 || index >= numSlots)
				return false;
			if (selected == m_Slots[index])
				return false;
			if (m_Slots [index] == null)
				return false;
			return m_Slots [index].isSelectable;
		}
		
		protected override bool SelectSlotInternal(int slot)
        {
            // Properly wrap the number (should be 0 to SlotCount-1)
            if (slot < 0 || slot >= numSlots)
				return false;
            
            // Check if valid & set
            if (IsSlotSelectable(slot))
            {
                SetSelected(m_Slots[slot], false, false);
				return true;
			}
			else
				return false;
		}

		public bool QuickSwapSlot ()
		{
			// Keep cycling until a valid slot is found (limited to number of slots)
			for (int i = 1; i < numSlots; ++i)
			{				
				// Select the slot if possible
				if (SelectSlot (m_History[i]))
					return true;
			}

			// No valid slots found
			return false;
		}

		public override bool SelectSlot (int slot)
		{
			// Check if succesful
			bool success = base.SelectSlot (slot);

			// Move the slot to the front of the history queue
			if (success)				
				MoveToFrontOfHistory (selected.quickSlot);

			return success;
		}

		public override void SwitchSelection ()
		{
			if (!QuickSwapSlot ())
				base.SwitchSelection ();
		}

		void MoveToFrontOfHistory (int slot)
		{
            if (slot == -1)
                return;

			// Get the current index of the slot
			int oldIndex = 0;
			for (; oldIndex < numSlots; ++oldIndex)
			{
				if (m_History [oldIndex] == slot)
					break;
			}

			// No change required if first in history
			if (oldIndex == 0)
				return;

			// Shift the other slots back in the history
			for (int i = oldIndex; i > 0; --i)
				m_History [i] = m_History [i - 1];

			// Set the slot as first
			m_History [0] = slot;
		}

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);
            writer.WriteValues(k_HistoryKey, m_History);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            InitialiseArrays();

            base.ReadProperties(reader, nsgo);
            
            reader.TryReadValues(k_HistoryKey, out m_History, m_History);
            if (m_History == null)
                m_History = new int[numSlots];
        }
    }
}