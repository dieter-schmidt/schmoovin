using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NeoFPS.Constants;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
	/// <summary>
	/// The base class for all player inventory behaviours
	/// </summary>
	/// <seealso cref="IInventory"/>
	/// <seealso cref="IQuickSlots"/>
	public abstract class FpsInventoryBase : MonoBehaviour, IInventory, IQuickSlots, INeoSerializableComponent
	{
		protected virtual void Awake ()
		{
            if (m_WieldableRoot != null)
            {
                m_WieldableRoot.localScale = new Vector3(m_WieldableRootScale, m_WieldableRootScale, m_WieldableRootScale);
                wieldableRootNsgo = m_WieldableRoot.GetComponent<NeoSerializedGameObject>();
            }
        }

		#if UNITY_EDITOR
		protected virtual void OnValidate ()
		{
			if (m_DropTransform == null && m_WieldableRoot != null)
				m_DropTransform = m_WieldableRoot;

            if (m_StartingOrder.Length != m_SlotCount)
            {
                int[] newStartingOrder = new int[m_SlotCount];

                if (m_SlotCount > m_StartingOrder.Length)
                {
                    // Copy old values
                    int i = 0;
                    for (; i < m_StartingOrder.Length; ++i)
                        newStartingOrder[i] = m_StartingOrder[i];
                    // Set remaining values
                    for (; i < newStartingOrder.Length; ++i)
                        newStartingOrder[i] = i;
                }
                else
                {
                    // Pool available indices
                    List<int> unClaimed = new List<int>(m_SlotCount);
                    for (int i = 0; i < m_SlotCount; ++i)
                        unClaimed.Add(i);
                    // Store clashes
                    List<int> clashes = new List<int>(m_SlotCount);
                    // Iterate through and check/copy
                    for (int i = 0; i < m_SlotCount; ++i)
                    {
                        // Check if the old entry is valid
                        if (m_StartingOrder[i] < m_SlotCount && unClaimed.Contains(m_StartingOrder[i]))
                        {
                            // Apply and remove from unclaimed
                            newStartingOrder[i] = m_StartingOrder[i];
                            unClaimed.Remove(m_StartingOrder[i]);
                        }
                        else
                        {
                            // Store the clashing index
                            clashes.Add(i);
                        }
                    }
                    // Resolve the clashes using the unclaimed values
                    for (int i = 0; i < clashes.Count; ++i)
                    {
                        newStartingOrder[clashes[i]] = unClaimed[0];
                        unClaimed.RemoveAt(0);
                    }
                }

                m_StartingOrder = newStartingOrder;
            }
        }
		#endif

		#region IInventory implementation

		[SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The transform to set as the parent of any objects added to the inventory.")]
		private Transform m_WieldableRoot = null;

        [SerializeField, Range(0.1f, 1f), Tooltip("A scale value for the wieldable root and any child items. Used to prevent weapons clipping into the scenery.")]
        private float m_WieldableRootScale = 0.5f;

		[SerializeField, NeoObjectInHierarchyField(true), Tooltip("A proxy transform used to set the drop position and rotation when a wieldable item is dropped.")]
        private Transform m_DropTransform = null;

        [SerializeField, Tooltip("The velocity of any dropped items relative to the character forward direction.")]
		private Vector3 m_DropVelocity = new Vector3 (0f, 2f, 3f);
		
		protected abstract DuplicateEntryBehaviour duplicateBehaviour { get; }
		
		public event UnityAction<IInventoryItem> onItemAdded;
		public event UnityAction<IInventoryItem> onItemRemoved;
		
		protected void OnItemRemoved(IInventoryItem item)
		{
			if (onItemRemoved != null)
				onItemRemoved(item);
		}

		public abstract void ClearAllItems(UnityAction<IInventoryItem> onClearAction);
		public abstract IInventoryItem GetItem(int itemIdentifier);

		protected abstract void AddItemReference(FpsInventoryItemBase item);
		protected abstract void RemoveItemReference(FpsInventoryItemBase item);

		public InventoryAddResult AddItem(IInventoryItem item)
        {
            // Check if item is valid
            if (item.itemIdentifier == 0)
            {
                Debug.LogError("Attempting to add inventory item with no ID set: " + (item as UnityEngine.Object));
                return InventoryAddResult.Rejected;
            }

            // Check if already in inventory
            var existing = (FpsInventoryItemBase)GetItem(item.itemIdentifier);
            if (existing != null)
            {
                // In and same type
                if (existing.itemIdentifier == item.itemIdentifier && existing.maxQuantity > 1)
                {
                    return AppendItem(existing, item);
                }
                else
                {
                    switch (duplicateBehaviour)
                    {
                        case DuplicateEntryBehaviour.Reject:
                            return InventoryAddResult.Rejected;
                        case DuplicateEntryBehaviour.DestroyOld:
                            RemoveItem(existing);
                            Destroy(existing.gameObject);
                            break;
                        case DuplicateEntryBehaviour.DropOld:
                            var qsi = existing as IQuickSlotItem;
                            if (qsi != null)
                                DropItem(qsi);
                            else
                            {
                                RemoveItem(existing);
                                Destroy(existing.gameObject);
                            }
                            break;
                    }
                }
			}

            FpsInventoryItemBase fpsItem = item as FpsInventoryItemBase;
			if (fpsItem != null)
			{
                // Needs a root transform for GameObject inventory items
                if (m_WieldableRoot == null)
                {
                    return InventoryAddResult.Rejected;
                }

                // Get transform
                Transform t = fpsItem.transform;

                // Attach using serialization system
                bool attached = false;
                if (wieldableRootNsgo != null)
                {
                    var itemNsgo = fpsItem.GetComponent<NeoSerializedGameObject>();
                    if (itemNsgo != null)
                    {
                        attached = true;
                        itemNsgo.SetParent(wieldableRootNsgo);
                    }
                }

                // Attach using transform if failed
                if (!attached)
                    t.SetParent(m_WieldableRoot);

                // Localise transform
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;

                // Callbacks
                item.OnAddToInventory (this, InventoryAddResult.Full);
				if (onItemAdded != null)
					onItemAdded(item);

                // Deactivate the object
                fpsItem.gameObject.SetActive (false);

                // Set item
                AddItemReference(fpsItem);

                return InventoryAddResult.Full;
			}
			else
			{
				Debug.LogError ("FpsInventoryQuickSwitch can only accept FpsInventoryItem components or components inherited from it.");
				return InventoryAddResult.Rejected;
			}
        }

        public InventoryAddResult AddItemFromPrefab(GameObject prefab)
        {
            if (prefab == null || m_WieldableRoot == null)
                return InventoryAddResult.Rejected;

            // Check if item is valid
            var prefabItem = prefab.GetComponent<IInventoryItem>();
            if (prefabItem == null || prefabItem.itemIdentifier == 0)
                return InventoryAddResult.Rejected;

            // Check if already in inventory
            var existing = (FpsInventoryItemBase)GetItem(prefabItem.itemIdentifier);
            if (existing != null)
            {
                // In and same type
                if (existing.itemIdentifier == prefabItem.itemIdentifier && existing.maxQuantity > 1)
                {
                    return AppendItem(existing, prefabItem);
                }
                else
                {
                    switch (duplicateBehaviour)
                    {
                        case DuplicateEntryBehaviour.Reject:
                            return InventoryAddResult.Rejected;
                        case DuplicateEntryBehaviour.DestroyOld:
                            RemoveItem(existing);
                            Destroy(existing.gameObject);
                            break;
                        case DuplicateEntryBehaviour.DropOld:
                            var qsi = existing as IQuickSlotItem;
                            if (qsi != null)
                                DropItem(qsi);
                            else
                            {
                                RemoveItem(existing);
                                Destroy(existing.gameObject);
                            }
                            break;
                    }
                }
            }
            
            var fpsItem = prefabItem as FpsInventoryItemBase;
            if (fpsItem != null)
            {
                Transform t;

                // Create instance
                if (wieldableRootNsgo != null)
                {
                    fpsItem = wieldableRootNsgo.InstantiatePrefab(fpsItem);
                    t = fpsItem.transform;
                }
                else
                {
                    fpsItem = Instantiate(fpsItem);
                    t = fpsItem.transform;
                    t.SetParent(m_WieldableRoot);
                }
                
                // Localise transform
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;

                // Callbacks
                fpsItem.OnAddToInventory(this, InventoryAddResult.Full);
                if (onItemAdded != null)
                    onItemAdded(fpsItem);

                // Deactivate the object
                fpsItem.gameObject.SetActive(false);

                // Set item
                AddItemReference(fpsItem);

                return InventoryAddResult.Full;
            }
            else
            {
                Debug.LogError("FpsInventoryQuickSwitch can only accept FpsInventoryItem components or components inherited from it.");
                return InventoryAddResult.Rejected;
            }

        }

        protected InventoryAddResult AppendItem(IInventoryItem existing, IInventoryItem item)
        {
            // At limit, so reject item
            if (existing.quantity >= existing.maxQuantity)
                return InventoryAddResult.Rejected;
            else
            {
                // Can take more
                int total = existing.quantity + item.quantity;
                if (total > existing.maxQuantity)
                {
                    // Limit reached. Reduce item quantity to excess and leave
                    existing.quantity = existing.maxQuantity;
                    int excess = total - existing.maxQuantity;
                    item.quantity = excess;
                    item.OnAddToInventory(this, InventoryAddResult.Partial);
                    return InventoryAddResult.Partial;
                }
                else
                {
                    // Limit not reached.
                    item.quantity = 0;
                    existing.quantity = total;
                    item.OnAddToInventory(this, InventoryAddResult.AppendedFull);
                    return InventoryAddResult.AppendedFull; // Caller responsible for destruction, etc
                }
            }
        }

        public void RemoveItem(IInventoryItem item)
		{
			if (item == null || item.itemIdentifier == 0)
				return;

			var result = (FpsInventoryItemBase)GetItem(item.itemIdentifier);
			if (result == null || result != (FpsInventoryItemBase)item)
				return;

			// Check if selected
			bool wasSelected = false;
			var qsi = result as IQuickSlotItem;
			if (qsi != null)
			{
				wasSelected = (selected == qsi);
				if (wasSelected)
					qsi.OnDeselectInstant ();
            }
                
			// Callbacks
			result.OnRemoveFromInventory ();
			OnItemRemoved (result);

			// Remove
			RemoveItemReference(result);

			// Switch selection if required
			if (wasSelected)
				SwitchSelection ();
		}
		
		public void RemoveItem(int itemIdentifier, UnityAction<IInventoryItem> onClearAction)
		{
			if (itemIdentifier == 0)
				return;
            
			var result = (FpsInventoryItemBase)GetItem (itemIdentifier);
			if (result == null)
				return;
                        
			// Check if selected
			bool wasSelected = false;
			var qsi = result as IQuickSlotItem;
			if (qsi != null)
			{
				wasSelected = (selected == qsi);
				if (wasSelected)
					qsi.OnDeselect ();
			}
            
			// Callbacks
			result.OnRemoveFromInventory ();
			OnItemRemoved (result);
			if (onClearAction != null)
				onClearAction (result);

			// Remove
			RemoveItemReference(result);

			// Switch selection if required
			if (wasSelected)
				SwitchSelection ();
		}


        public void GetItems(List<IInventoryItem> output)
        {
            GetItemsSorted(output, null, null);
        }

        public void GetItems(List<IInventoryItem> output, InventoryCallbacks.FilterItem filter)
        {
            GetItemsSorted(output, filter, null);
        }

        public abstract void GetItemsSorted(List<IInventoryItem> output, InventoryCallbacks.FilterItem filter, Comparison<IInventoryItem>  compare);

        public IInventoryItem[] GetItems()
        {
            var results = new List<IInventoryItem>();
            GetItemsSorted(results, null, null);
            return results.ToArray();
        }

        public IInventoryItem[] GetItems(InventoryCallbacks.FilterItem filter)
        {
            var results = new List<IInventoryItem>();
            GetItemsSorted(results, filter, null);
            return results.ToArray();
        }

        public IInventoryItem[] GetItemsSorted(InventoryCallbacks.FilterItem filter, Comparison<IInventoryItem> compare)
        {
            var results = new List<IInventoryItem>();
            GetItemsSorted(results, filter, compare);
            return results.ToArray();
        }

        #endregion

        #region IQuickSlots implementation

        [SerializeField, Range (2, 10), Tooltip("The number of item quick slots.")]
		private int m_SlotCount = 10;

        [SerializeField, Tooltip("The selection method for the starting slot.")]
        private StartingSlot m_StartingSlotChoice = StartingSlot.Ascending;

		[SerializeField, Tooltip("This array specifies the selection order on start. The highest on the list that exists will be the starting selection.")]
		private int[] m_StartingOrder = {0,1,2,3,4,5,6,7,8,9};
        
		public event UnityAction<IQuickSlotItem> onSelectionChanged;
		public event UnityAction<int, IQuickSlotItem> onSlotItemChanged;
		public event UnityAction<IQuickSlotItem> onItemDropped;

        private Waitable m_DeselectWaitable = null;
        private Coroutine m_SelectionCoroutine = null;
        private int[] m_InternalStartingOrder = null;
        private int m_Holstered = -1;

        protected IQuickSlotItem backupSlot { get; private set; }

		private IQuickSlotItem m_Selected = null;
        public IQuickSlotItem selected
		{
			get { return m_Selected; }
		}

        IEnumerator SlowSwitch(bool silent)
        {
            while (m_DeselectWaitable != null && !m_DeselectWaitable.isComplete)
                yield return null;

            if (m_Selected != null)
                m_Selected.OnSelect();

            if (!silent)
                OnSelectionChanged();

            m_SelectionCoroutine = null;
        }

		public abstract void SetSlotItem(int slot, IQuickSlotItem item);
		public abstract IQuickSlotItem GetSlotItem(int slot);
		public abstract void ClearSlots();
		public abstract bool IsSlotSelectable(int index);
		protected abstract bool SelectSlotInternal(int slot);

        protected void SetSelected(IQuickSlotItem item, bool instant, bool silent)
        {
            if (m_Selected != item)
            {
                if (instant)
                {
                    if (m_Selected != null)
                        m_Selected.OnDeselectInstant();

                    m_Selected = item;

                    if (m_Selected != null)
                        m_Selected.OnSelect();

                    if (!silent)
                        OnSelectionChanged();
                }
                else
                {
                    Waitable waitable = null;
                    if (m_Selected != null)
                        waitable = m_Selected.OnDeselect();

                    m_Selected = item;

                    if (waitable != null)
                    {
                        m_DeselectWaitable = waitable;
                        if (m_SelectionCoroutine == null)
                            m_SelectionCoroutine = StartCoroutine(SlowSwitch(silent));
                    }
                    else
                    {
                        if (m_Selected != null)
                            m_Selected.OnSelect();

                        if (!silent)
                            OnSelectionChanged();
                    }
                }
            }
        }

        public bool isBackupItemSelected
        {
            get { return m_Selected != null && m_Selected.quickSlot == -1; }
        }

        protected void OnSelectionChanged()
		{
			if (onSelectionChanged != null)
				onSelectionChanged(selected);
		}

		protected void OnSlotItemChanged(int slot, IQuickSlotItem item)
		{
			if (onSlotItemChanged != null)
				onSlotItemChanged(slot, item);
		}

		public virtual bool SelectSlot (int slot)
        {
            if (m_LockObject != null)
                return false;
            if (slot == -1 || m_LockObject)
                return SelectBackupItem(true, false);
            return SelectSlotInternal(slot);
		}

        public virtual bool SelectBackupItem(bool toggle, bool silent)
        {
            if (m_LockObject != null || backupSlot == null)
                return false;

            if (selected == backupSlot)
            {
                if (toggle)
                {
                    // Select previous
                    if (m_Holstered == -1)
                        return SelectStartingSlot();
                    else
                        SelectSlotInternal(m_Holstered);
                    // If changing SelectSlotInternal to SelectSlot, will need to update stacked inventory
                    // Since SelectSlot takes 0-9 and calls internal 0-99 (which is what holstering records)
                }
                else
                    return false;
            }
            else
            {
                if (selected != null)
                    m_Holstered = selected.quickSlot;
                SetSelected(backupSlot, false, silent);
            }

            return true;
        }

		public bool SelectStartingSlot ()
        {
            if (m_LockObject != null)
                return false;

            // Select the starting slot (note, if none are valid then none will be selected)
            switch (m_StartingSlotChoice)
			{
				case StartingSlot.Ascending:
					for (int i = 0; i < m_SlotCount; ++i)
					{
						if (SelectSlotInternal(i))
							return true;
					}
					break;
				case StartingSlot.Descending:
					for (int i = m_SlotCount - 1; i >= 0; --i)
					{
						if (SelectSlotInternal(i))
							return true;
					}
					break;
				case StartingSlot.CustomOrder:
					for (int i = 0; i < m_SlotCount; ++i)
					{
						if (SelectSlotInternal(m_StartingOrder[i]))
							return true;
					}
					break;
			}

            // Check for the backup item
            if (backupSlot != null)
            {
                SetSelected(backupSlot, false, false);
                return true;
			}

            return false;
		}

        public virtual bool SelectNextSlot ()
        {
            if (m_LockObject != null)
                return false;

            if (selected != null)
            {
                // Get the next slot
                int index = selected.quickSlot;

				// Keep cycling until a valid slot is found (limited to number of slots)
				for (int i = 0; i < m_SlotCount; ++i)
				{
					index = WrapSlotIndex (index + 1);
					// Select the slot if possible
					if (SelectSlotInternal (index))
						return true;
				}

				// No valid slots found
				// Check for the backup item
				if (backupSlot != null)
                {
                    SetSelected(backupSlot, false, false);
                    return true;
				}
				return false;
			}
			else
				return SelectStartingSlot ();
		}

		public virtual bool SelectPreviousSlot ()
        {
            if (m_LockObject != null)
                return false;

            if (selected != null)
			{
				// Get the previous slot
				int index = selected.quickSlot;

				// Keep cycling until a valid slot is found (limited to number of slots)
				for (int i = 0; i < m_SlotCount; ++i)
				{
					index = WrapSlotIndex (index - 1);
					// Select the slot if possible
					if (SelectSlotInternal (index))
						return true;
				}

				// No valid slots found
				// Check for the backup item
				if (backupSlot != null)
                {
                    SetSelected(backupSlot, false, false);
                    return true;
				}
				return false;
			}
			else
				return SelectStartingSlot ();
		}


		public int numSlots
		{
			get { return m_SlotCount; }
		}

		public virtual void AutoSwitchSlot (int slot)
        {
            if (m_LockObject != null)
                return;

            // Check if this is the backup item
            if (slot == -1 && backupSlot != null)
            {
                SetSelected(backupSlot, false, false);
                return;
			}

            if (initialisedContents)
                StartCoroutine(AutoSwitchSlotDelayed(slot));
		}

		protected virtual IEnumerator AutoSwitchSlotDelayed (int slot)
		{
			yield return null;

			// Properly wrap the number (should be 1-SlotCount)
			slot = WrapSlotIndex (slot);

			if (selected == null)
				SelectSlotInternal(slot);
			else
			{
				// Check if valid + better than current & set
				if (IsSlotSelectable (slot))
				{
					if (FpsSettings.gameplay.autoSwitchWeapons)
					{
						int currentSlotIndex = selected.quickSlot;
						for (int i = 0; i < m_StartingOrder.Length; ++i)
						{
							if (m_StartingOrder[i] == currentSlotIndex)
								break;
							if (m_StartingOrder[i] == slot)
							{
								SelectSlotInternal(slot);
                                break;
							}
						}
					}
				}
			}
		}

		public virtual void SwitchSelection ()
		{
			SelectNextSlot ();
		}

		public void DropSelected ()
        {
            if (m_LockObject == null)
                DropItem(selected);
		}

		protected void DropItem (IQuickSlotItem qsi)
		{
			if (qsi != null && qsi.isDroppable)
			{
				// Fire onDropped event
				if (onItemDropped != null)
					onItemDropped(qsi);

                // Remove from inventory
                var item = qsi as FpsInventoryItemBase;
                RemoveItem(item);

                // Get the item to handle its own drop
                if (m_DropTransform != null)
                	qsi.DropItem(m_DropTransform.position, m_DropTransform.forward, m_WieldableRoot.rotation * m_DropVelocity);
				else
                	qsi.DropItem(m_WieldableRoot.position, m_WieldableRoot.forward, m_WieldableRoot.rotation * m_DropVelocity);

				// Destroy the inventory item
				Destroy (item.gameObject);
			}
		}

		private int WrapSlotIndex (int slot)
		{
			while (slot < 0)
				slot += m_SlotCount;
			while (slot >= m_SlotCount)
				slot -= m_SlotCount;
			return slot;
		}

		protected int[] GetStartingSlotOrder ()
		{
			if (m_InternalStartingOrder == null)
			{
				m_InternalStartingOrder = new int[m_SlotCount + 1];
				switch (m_StartingSlotChoice)
				{
					case StartingSlot.Ascending:
						for (int i = 0; i < m_SlotCount; ++i)
							m_InternalStartingOrder [i] = i;
						break;
					case StartingSlot.Descending:
						for (int i = 0; i < m_SlotCount; ++i)
							m_InternalStartingOrder [i] = m_SlotCount - 1 - i;
						break;
					case StartingSlot.CustomOrder:
						{
							// Create list of available indices
							List<int> available = new List<int> (m_SlotCount);
							for (int i = 0; i < m_SlotCount; ++i)
								available.Add (i);
							// Set indices from list
							int done = 0;
							for (int i = 0; i < m_StartingOrder.Length; ++i)
							{
								// Check if not already used
								if (available.Contains (m_StartingOrder[i]))
								{
									m_InternalStartingOrder [i] = m_StartingOrder [i];
									available.Remove (m_StartingOrder [i]);
									++done;
								}
							}
							// Fill remaining slots with available
							for (int i = 0; i < m_SlotCount - done; ++i)
								m_InternalStartingOrder [i + done] = available[i];
						}
						break;
				}
                m_InternalStartingOrder[m_SlotCount] = -1;
            }
			return m_InternalStartingOrder;
		}

        private Coroutine m_LockCoroutine = null;
        private UnityEngine.Object m_LockObject = null;
        private int m_PreLockedItem = -1;
        
        public bool LockSelectionToSlot(int index, UnityEngine.Object o)
        {
            // Check if lock is valid
            if (o == null || m_LockObject != null)
                return false;

            // Cancel lock/unlock event from this frame
            if (m_LockCoroutine != null)
            {
                StopCoroutine(m_LockCoroutine);
                m_LockCoroutine = null;
            }

            // Check if selectable
            var item = GetSlotItem(index);
            if (item == null || !item.isSelectable)
                return false;

            // Delayed lock (allows cancellation by another lock event on the same frame)
            m_LockCoroutine = StartCoroutine(DelayedLockSelectionToSlot(index, o));

            return true;
        }

        IEnumerator DelayedLockSelectionToSlot(int index, UnityEngine.Object o)
        {
            yield return null;

            // Select the new object and record the old one
            m_PreLockedItem = m_Selected.quickSlot;
            SelectSlot(index);
            m_LockObject = o;

            m_LockCoroutine = null;
        }

        public bool LockSelectionToBackupItem(UnityEngine.Object o, bool silent)
        {
            // Check if lock is valid
            if (o == null || m_LockObject != null)
                return false;

            // Cancel lock/unlock event from this frame
            if (m_LockCoroutine != null)
            {
                StopCoroutine(m_LockCoroutine);
                m_LockCoroutine = null;
            }
            
            // Delayed lock (allows cancellation by another lock event on the same frame)
            m_LockCoroutine = StartCoroutine(DelayedLockSelectionToBackupItem(silent, o));

            return true;
        }

        IEnumerator DelayedLockSelectionToBackupItem(bool silent, UnityEngine.Object o)
        {
            yield return null;

            // Select the new object and record the old one
            m_PreLockedItem = m_Selected.quickSlot;
            SelectBackupItem(false, silent);
            m_LockObject = o;

            m_LockCoroutine = null;
        }

        public bool LockSelectionToNothing(UnityEngine.Object o, bool silent)
        {
            // Check if lock is valid
            if (o == null || m_LockObject != null)
                return false;

            // Cancel lock/unlock event from this frame
            if (m_LockCoroutine != null)
                StopCoroutine(m_LockCoroutine);

            // Delayed lock (allows cancellation by another lock event on the same frame)
            m_LockCoroutine = StartCoroutine(DelayedLockSelectionToNothing(silent, o));

            return true;
        }

        IEnumerator DelayedLockSelectionToNothing(bool silent, UnityEngine.Object o)
        {
            yield return null;

            // Select the new object and record the old one
            if (m_Selected != null)
                m_PreLockedItem = m_Selected.quickSlot;
            SetSelected(null, false, silent);
            m_LockObject = o;

            m_LockCoroutine = null;
        }

        public void UnlockSelection(UnityEngine.Object o)
        {
            if (m_LockObject == o)
            {
                // Cancel lock/unlock event from this frame
                if (m_LockCoroutine != null)
                    StopCoroutine(m_LockCoroutine);

                // Delayed lock (allows cancellation by another lock event on the same frame)
                if (enabled && gameObject.activeInHierarchy)
                    m_LockCoroutine = StartCoroutine(DelayedUnlockSelection());
                else
                {
                    SelectSlot(m_PreLockedItem);
                    m_LockCoroutine = null;
                }
                m_LockObject = null;
            }
        }

        IEnumerator DelayedUnlockSelection()
        {
            // Wait a few frames
            // Logic is that this will mostly be used with the motion graph behaviours,
            // and a few frames leeway can make a big difference if transitioning wall run to fall to mantle, etc
            // with a middle state of 1-2 frames that doesn't require the inventory locked
            yield return null;
            yield return null;
            yield return null;

            SelectSlot(m_PreLockedItem);

            m_LockCoroutine = null;
        }

        #endregion

        #region STARTING STATE

        [SerializeField, NeoPrefabField, Tooltip("An item to use if no wieldables are in the inventory. This could be empty hands or an infinite weapon such as a knife.")]
		private FpsInventoryWieldable m_BackupItem = null;
        [SerializeField, Tooltip("A selection of inventory items to be added to the inventory on start.")]
        private FpsInventoryItemBase[] m_StartingItems = new FpsInventoryItemBase[0];

        protected bool initialisedContents
        {
            get;
            private set;
        }

        public void ApplyLoadout(FpsInventoryLoadout loadout)
        {
            m_StartingItems = loadout.items;
        }

		protected virtual void Start ()
		{
            if (!initialisedContents)
            {
                if (m_BackupItem != null && backupSlot == null)
                {
                    // Instantiate the prefab and add to inventory
                    if (wieldableRootNsgo != null)
                        backupSlot = wieldableRootNsgo.InstantiatePrefab(m_BackupItem);
                    else
                        backupSlot = Instantiate(m_BackupItem);

                    var result = AddItem(backupSlot as IInventoryItem);
                    if (result == InventoryAddResult.Rejected)
                        Debug.Log("Player inventory rejected backup item: " + m_BackupItem.name);
                }

                // Add each item to the inventory
                for (int i = 0; i < m_StartingItems.Length; ++i)
                {
                    // Check if null
                    if (m_StartingItems[i] == null)
                        continue;

                    // Instantiate the prefab and add to inventory
                    FpsInventoryItemBase item = null;
                    if (wieldableRootNsgo != null)
                    {
                        if (m_StartingItems[i].GetComponent<NeoSerializedGameObject>() == null)
                        {
                            Debug.LogWarning("Added wieldable to character inventory which does not have a NeoSerializedGameObject component. This means it will not work with the NeoFPS save system. Wieldable: " + m_StartingItems[i].name);
                            item = Instantiate(m_StartingItems[i]);
                        }
                        else
                            item = wieldableRootNsgo.InstantiatePrefab(m_StartingItems[i]);
                    }
                    else
                        item = Instantiate(m_StartingItems[i]);

                    var result = AddItem(item);
                    if (result == InventoryAddResult.Rejected)
                        Debug.Log("Player inventory rejected starting item: " + item.name);
                }

                // Select the starting item
                SelectStartingSlot();

                initialisedContents = true;
            }
		}

        #endregion

        #region INeoSerializableComponent implementation

        private static readonly NeoSerializationKey k_BackupKey = new NeoSerializationKey("backup");
        private static readonly NeoSerializationKey k_SelectedKey = new NeoSerializationKey("selected");
        private static readonly NeoSerializationKey k_HolsteredKey = new NeoSerializationKey("holstered");
        private static readonly NeoSerializationKey k_PreLockedKey = new NeoSerializationKey("preLocked");

        protected NeoSerializedGameObject wieldableRootNsgo { get; private set; }

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteComponentReference(k_BackupKey, backupSlot, nsgo);
            writer.WriteValue(k_HolsteredKey, m_Holstered);
            if (selected != null)
                writer.WriteValue(k_SelectedKey, selected.quickSlot);
            if (m_LockObject != null)
                writer.WriteValue(k_PreLockedKey, m_PreLockedItem);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            if (initialisedContents != true)
            {
                // Add inventory items (backup item will be rejected due to quickslot)
                var found = m_WieldableRoot.GetComponentsInChildren<FpsInventoryItemBase>(true);
                for (int i = 0; i < found.Length; ++i)
                    AddItem(found[i]);
                
                // Add backup item
                IQuickSlotItem serializedItem;
                if (reader.TryReadComponentReference(k_BackupKey, out serializedItem, nsgo) && serializedItem != null)
                    backupSlot = serializedItem;

                // Select the correct slot
                int selectedSlot;
                if (reader.TryReadValue(k_SelectedKey, out selectedSlot, -1))
                {
                    if (selectedSlot == -1)
                        SetSelected(backupSlot, false, false);
                    else
                        SelectSlotInternal(selectedSlot);
                }

                // Read holstered item
                reader.TryReadValue(k_HolsteredKey, out m_Holstered, -1);

                // Read pre-locked item
                reader.TryReadValue(k_PreLockedKey, out m_PreLockedItem, m_PreLockedItem);

                initialisedContents = true;
            }
            else
                Debug.LogError("Attempting to load inventory contents after it's been initialised: " + name);
        }

        #endregion
    }
}