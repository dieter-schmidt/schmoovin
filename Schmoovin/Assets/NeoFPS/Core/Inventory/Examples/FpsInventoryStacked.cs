using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoFPS.Constants;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using System;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-mb-fpsinventorystacked.html")]
    public class FpsInventoryStacked : FpsInventoryBase
    {
        [SerializeField, Tooltip("What to do when trying to add an item to the inventory that already exists.")]
        private DuplicateEntryBehaviour m_DuplicateBehaviour = DuplicateEntryBehaviour.Reject;

		private const int k_MaxStackSize = 10;

		private static readonly NeoSerializationKey k_StartIndexKey = new NeoSerializationKey("startIndex");
        private static readonly NeoSerializationKey k_CurrentItemKey = new NeoSerializationKey("currentItem");

		private Dictionary<int, FpsInventoryItemBase> m_Items = null;
		private IQuickSlotItem[] m_Slots = null;
        private bool m_Initialised = false;

        public Stack[] stacks
	    {
		    get; 
		    private set;
	    }
	    
	    public int maxStackSize
	    {
		    get { return k_MaxStackSize; }
	    }
	    
	    protected override DuplicateEntryBehaviour duplicateBehaviour
	    {
		    get { return m_DuplicateBehaviour; }
	    }
        
        protected override void Awake ()
        {
            base.Awake ();

            Initialise();
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
					item.OnRemoveFromInventory();
					OnItemRemoved(item);
					if (onClearAction != null)
						onClearAction(item);
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

	    public class Stack : INeoSerializableObject
	    {
		    private FpsInventoryStacked m_Inventory = null;
		    private int m_StartIndex = 0;
		    
		    public int currentItem
            {
                get;
                private set;
            }

		    public int currentItemLocal
		    {
			    get
			    {
				    if (currentItem == -1)
					    return -1;
				    else
					    return currentItem - m_StartIndex;
			    }
		    }

		    public int GetNextItem ()
		    {
			    int stackSize = k_MaxStackSize;
			    int start = (currentItem == -1) ? m_StartIndex : currentItem + 1;
			    for (int i = 0; i < stackSize; ++i)
			    {
				    int index = start + i;
				    if (index >= m_StartIndex + stackSize)
					    index -= stackSize;

				    IQuickSlotItem item = m_Inventory.m_Slots[index];
				    if (item != null && item.isSelectable)
					    return index;
			    }
			    return -1;
		    }

			public Stack(FpsInventoryStacked inventory, int startIndex)
			{
				m_Inventory = inventory;
				m_StartIndex = startIndex;
			    inventory.onSlotItemChanged += OnSlotItemChanged;
			    inventory.onSelectionChanged += OnSelectionChanged;
				currentItem = -1;
		    }

		    private void OnSelectionChanged (IQuickSlotItem item)
		    {
			    int index = item.quickSlot;
			    if (index < m_StartIndex || index >= m_StartIndex + k_MaxStackSize)
				    return;
			    currentItem = index;
		    }

		    private void OnSlotItemChanged (int slot, IQuickSlotItem item)
		    {
			    if (item == null)
			    {
				    if (currentItem == slot)
					    currentItem = GetNextItem ();
				    if (currentItem == slot)
					    currentItem = -1;
			    }
			    else
			    {
				    if (slot < m_StartIndex || slot >= m_StartIndex + k_MaxStackSize)
					    return;

				    if (currentItem == -1)
					    currentItem = slot;
			    }
		    }

            public void WriteProperties(INeoSerializer writer)
            {
                writer.WriteValue(k_StartIndexKey, m_StartIndex);
                writer.WriteValue(k_CurrentItemKey, currentItem);
            }

            public void ReadProperties(INeoDeserializer reader)
            {
                reader.TryReadValue(k_StartIndexKey, out m_StartIndex, m_StartIndex);

                int result = 0;
                if (reader.TryReadValue(k_CurrentItemKey, out result, currentItem))
                    currentItem = result;
            }
        }


        private void Initialise ()
        {
            if (!m_Initialised)
			{
				m_Items = new Dictionary<int, FpsInventoryItemBase>();
				m_Slots = new IQuickSlotItem[numSlots];

                int numStacks = numSlots / k_MaxStackSize;
                stacks = new Stack[numStacks];
                for (int i = 0; i < numStacks; ++i)
                {
                    stacks[i] = new Stack(this, i * k_MaxStackSize);
                }

                m_Initialised = true;
            }
	    }

	    protected override bool SelectSlotInternal(int slot)
        {
            // Properly wrap the number (should be 1-SlotCount)
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

        public override bool SelectSlot(int slot)
        {
            if (slot == -1)
                return SelectBackupItem(true, false);

            if (slot < 0 || slot >= stacks.Length)
                return false;

            int previous = -1;
            int previousStack = int.MaxValue;
            if (selected != null)
            {
                previous = selected.quickSlot;
                previousStack = previous / k_MaxStackSize;
            }

            if (slot != previousStack)
            {
                // Get stack's front item
                int index = stacks[slot].currentItem;
                if (index == -1)
                    index = stacks[slot].GetNextItem();

                // Try selecting
                return SelectSlotInternal(index);
            }
            else
            {
                int next = stacks[slot].GetNextItem();
                return SelectSlotInternal(next);
            }
        }

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

            int numStacks = numSlots / k_MaxStackSize;
            for (int i = 0; i < numStacks; ++i)
            {
                writer.PushContext(SerializationContext.ObjectNeoSerialized, i);
                stacks[i].WriteProperties(writer);
                writer.PopContext(SerializationContext.ObjectNeoSerialized);
            }
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            Initialise();

            base.ReadProperties(reader, nsgo);

            int numStacks = numSlots / k_MaxStackSize;
            for (int i = 0; i < numStacks; ++i)
            {
                if (reader.PushContext(SerializationContext.ObjectNeoSerialized, i))
                {
                    stacks[i].ReadProperties(reader);
                    reader.PopContext(SerializationContext.ObjectNeoSerialized, i);
                }
            }
        }
    }
}
