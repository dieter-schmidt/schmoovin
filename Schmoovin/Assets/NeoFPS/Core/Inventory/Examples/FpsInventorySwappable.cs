using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using NeoFPS.Constants;
using NeoSaveGames;
using NeoSaveGames.Serialization;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-mb-fpsinventoryswappable.html")]
    public class FpsInventorySwappable : FpsInventoryBase
    {
        [SerializeField, Tooltip("What to do when replacing an old item with a new one.")]
        private SwapAction m_SwapAction = SwapAction.Drop;

        [SerializeField, Range(1, 10), Tooltip("The number of quick slots available for each category.")]
        private int[] m_GroupSizes = new int[0];

		private Dictionary<int, FpsInventoryItemBase> m_Items = null;
		private IQuickSlotItem[] m_Slots = null;
        private ItemGroup[] m_ItemGroups = null;
        private bool m_Initialised = false;

        protected override DuplicateEntryBehaviour duplicateBehaviour
		{
			get { return DuplicateEntryBehaviour.Reject; }
		}
	    
		#if UNITY_EDITOR
	    protected override void OnValidate ()
	    {
		    base.OnValidate ();
            // Resize group count based on constant
            if (m_GroupSizes.Length != FpsSwappableCategory.count)
            {
                int[] newGroupSizes = new int[FpsSwappableCategory.count];
                int i = 0;
                for (; i < newGroupSizes.Length && i < m_GroupSizes.Length; ++i)
                    newGroupSizes[i] = m_GroupSizes[i];
                for (; i < newGroupSizes.Length; ++i)
                    newGroupSizes[i] = 1;
                m_GroupSizes = newGroupSizes;
            }
            // Clamp group sizes, as range does not work until edited in inspector
            for (int i = 0; i < m_GroupSizes.Length; ++i)
                m_GroupSizes[i] = Mathf.Clamp(m_GroupSizes[i], 1, 10);
        }
		#endif
	    

	    public SwapAction swapAction
	    {
		    get { return m_SwapAction; }
	    }

	    public class ItemGroup : INeoSerializableObject
        {
		    private FpsInventorySwappable m_Inventory;
		    public int startIndex { get; private set; }
		    public int count { get; private set; }
		    private int[] m_History;

		    public ItemGroup (FpsInventorySwappable inventory, int startIndex, int count)
		    {
			    m_Inventory = inventory;
			    this.startIndex = startIndex;
			    this.count = count;
			    m_History = new int[this.count];
			    for (int i = 0; i < this.count; ++i)
				    m_History[i] = -1;
		    }
		    
		    public int GetAvailableSlot ()
		    {
			    // Check for empty slots
			    for (int i = 0; i < count; ++i)
			    {
				    if (m_Inventory.m_Slots[startIndex + i] == null)
					    return startIndex + i;
			    }
			    
			    // Get last selected or first in list if none selected
			    return m_History[0] != -1 ? m_History[0] : startIndex;
		    }

		    public void OnSelectSlot (int slot)
            {
                // Get the current index of the slot
                int oldIndex = 0;
			    for (; oldIndex < count - 1; ++oldIndex)
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

		    public void OnClearSlot (int slot)
		    {
			    // Get the current index of the slot
			    int oldIndex = -1;
			    for (int i = 0; i < count; ++i)
			    {
				    if (m_History[i] == slot)
				    {
					    oldIndex = i;
					    break;
				    }
			    }
			    
			    // No change required if not found
			    if (oldIndex == -1)
				    return;

			    // Shift the other slots forward in the history
			    for (int i = oldIndex; i < count - 1; ++i)
				    m_History[i] = m_History[i + 1];
			    
			    // Clear the last slot
			    m_History[count - 1] = -1;
		    }

            public void WriteProperties(INeoSerializer writer)
            {
                writer.WriteValues(k_HistoryKey, m_History);
            }

            public void ReadProperties(INeoDeserializer reader)
            {
                int[] results = null;
                if (reader.TryReadValues(k_HistoryKey, out results, m_History))
                {
                    for (int i = 0; i < Mathf.Min(results.Length, m_History.Length); ++i)
                        m_History[i] = results[i];
                }
            }
        }

        private static readonly NeoSerializationKey k_HistoryKey = new NeoSerializationKey("history");

		protected override void Awake ()
		{
			base.Awake ();
            Initialise();
		}

        void Initialise()
        {
            if (!m_Initialised)
			{
				m_Items = new Dictionary<int, FpsInventoryItemBase>();
				m_Slots = new IQuickSlotItem[numSlots];

                m_ItemGroups = new ItemGroup[m_GroupSizes.Length];
                int slotIndex = 0;
                for (int i = 0; i < m_GroupSizes.Length; ++i)
                {
                    m_ItemGroups[i] = new ItemGroup(this, slotIndex, m_GroupSizes[i]);
                    slotIndex += m_GroupSizes[i];
                }

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

		public int GetSlotForItem (FpsInventoryWieldableSwappable item)
	    {
		    if (item.category < 0 || item.category >= m_ItemGroups.Length)
			    return -1;

			if (m_SaveSlotsMap != null && wieldableRootNsgo != null)
            {
				var nsgo = item.GetComponent<NeoSerializedGameObject>();
				if (nsgo != null)
                {
					int id = wieldableRootNsgo.serializedChildren.GetSerializationKeyForObject(nsgo);
					if (m_SaveSlotsMap.ContainsKey(id))
						return m_SaveSlotsMap[id];
				}
			}

		    return m_ItemGroups[item.category].GetAvailableSlot ();
	    }

	    private int GetGroupForSlot (int slot)
	    {
		    for (int i = 0; i < m_ItemGroups.Length - 1; ++i)
		    {
			    if (m_ItemGroups[i].startIndex <= slot && m_ItemGroups[i + 1].startIndex > slot)
				    return i;
		    }
		    return m_ItemGroups.Length - 1;
	    }
		
		public override void SetSlotItem(int slot, IQuickSlotItem item)
		{
			if (slot < 0 || slot >= m_Slots.Length || m_Slots[slot] == item)
				return;

			IQuickSlotItem old = m_Slots[slot];
			if (old != null)
			{
				if (item == null)
				{
					int groupIndex = GetGroupForSlot (slot);
					if (groupIndex != -1)
						m_ItemGroups[groupIndex].OnClearSlot (slot);
				}
				else
				{
					if (swapAction == SwapAction.Drop)
						DropItem (old);
					else
						Destroy (old.gameObject);
				}
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
            // Properly wrap the number (should be 1-SlotCount)
            if (slot < 0 || slot >= numSlots)
				return false;

			// Check if valid & set
			if (IsSlotSelectable(slot))
            {
                SetSelected(m_Slots[slot], false, false);
                int group = GetGroupForSlot (slot);
				if (group != -1)
					m_ItemGroups[group].OnSelectSlot (slot);
				return true;
			}
			else
				return false;
		}

        protected override IEnumerator AutoSwitchSlotDelayed(int slot)
        {
            yield return null;
            SelectSlotInternal(slot);

            // NB: Might be worth making a variant of the base class one that swaps
            // item priority for group priority instead. Only switch if group has
            // higher or equal priority (eg. gun > grenade)
        }

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

			// Write groups
			for (int i = 0; i < m_ItemGroups.Length; ++i)
			{
				writer.PushContext(SerializationContext.ObjectNeoFormatted, i);
				m_ItemGroups[i].WriteProperties(writer);
				writer.PopContext(SerializationContext.ObjectNeoFormatted);
			}

			// Write slot map
			if (wieldableRootNsgo != null)
			{
				var map = new List<Vector2Int>(m_Slots.Length);
				for (int i = 0; i < m_Slots.Length; ++i)
				{
					if (m_Slots[i] != null)
					{
						var itemNsgo = m_Slots[i].GetComponent<NeoSerializedGameObject>();
						if (itemNsgo != null)
						{
							int id = wieldableRootNsgo.serializedChildren.GetSerializationKeyForObject(itemNsgo);
							map.Add(new Vector2Int(id, m_Slots[i].quickSlot));
						}
					}
				}
				writer.WriteValues(k_SlotMapKey, map);
			}
        }

		public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
		{
			Initialise();

			// Read slot map
			Vector2Int[] mapEntries;
			if (reader.TryReadValues(k_SlotMapKey, out mapEntries, null))
            {
				m_SaveSlotsMap = new Dictionary<int, int>(mapEntries.Length);
				for (int i = 0; i < mapEntries.Length; ++i)
					m_SaveSlotsMap.Add(mapEntries[i].x, mapEntries[i].y);
            }

			base.ReadProperties(reader, nsgo);

			// Read groups
			for (int i = 0; i < m_ItemGroups.Length; ++i)
			{
				reader.PushContext(SerializationContext.ObjectNeoFormatted, i);
				m_ItemGroups[i].ReadProperties(reader);
				reader.PopContext(SerializationContext.ObjectNeoFormatted, i);
			}

			// Discard slot map. It's only needed during base.ReadProperties
			m_SaveSlotsMap = null;
		}

		private static readonly NeoSerializationKey k_SlotMapKey = new NeoSerializationKey("slotMap");

		private Dictionary<int, int> m_SaveSlotsMap = null;
    }
}