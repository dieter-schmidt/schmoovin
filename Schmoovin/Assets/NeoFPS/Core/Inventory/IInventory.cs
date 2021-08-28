using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace NeoFPS
{
	/// <summary>
	/// The inventory interface.
	/// </summary>
	/// <remarks>
	/// If you want to replace the NeoFPS inventory systems with your own, or another third party implementation,
	/// then implement this interface and other NeoFPS systems will still be able to interact with it.
	/// </remarks>
	public interface IInventory
	{
		/// <summary>
		/// Add an item to the inventory.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>The result of the addition (success, partial, failure).</returns>
		/// <seealso cref="InventoryAddResult"/>
		InventoryAddResult AddItem (IInventoryItem item);
        InventoryAddResult AddItemFromPrefab(GameObject prefab);

        void RemoveItem (IInventoryItem item);
		void RemoveItem (int itemIdentifier, UnityAction<IInventoryItem> onClearAction);
		void ClearAllItems (UnityAction<IInventoryItem> onClearAction);

		void ApplyLoadout(FpsInventoryLoadout loadout);

		void GetItems(List<IInventoryItem> output);
        void GetItems(List<IInventoryItem> output, InventoryCallbacks.FilterItem filter);
        void GetItemsSorted(List<IInventoryItem> output, InventoryCallbacks.FilterItem filter, Comparison<IInventoryItem> compare);
        IInventoryItem[] GetItems();
        IInventoryItem[] GetItems(InventoryCallbacks.FilterItem filter);
        IInventoryItem[] GetItemsSorted(InventoryCallbacks.FilterItem filter, Comparison<IInventoryItem> compare);

        IInventoryItem GetItem (int identifier);

		event UnityAction<IInventoryItem> onItemAdded;
		event UnityAction<IInventoryItem> onItemRemoved;
	}
}