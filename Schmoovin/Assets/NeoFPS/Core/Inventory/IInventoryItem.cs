using System;
using UnityEngine;
using UnityEngine.Events;
using NeoFPS.Constants;

namespace NeoFPS
{
	public interface IInventoryItem
	{
		IInventory inventory { get; }
		ICharacter owner { get; }

		int itemIdentifier { get; }

		int quantity { get; set; }
		int maxQuantity { get; }

		event UnityAction onAddToInventory;
		event UnityAction onQuantityChange;
		event UnityAction onRemoveFromInventory;

		void OnAddToInventory (IInventory i, InventoryAddResult addResult);
		void OnRemoveFromInventory ();

        // Add monobehaviour methods to remove need for casting if required
        GameObject gameObject { get; }
        Transform transform { get; }
        T GetComponent<T>();
        T GetComponentInChildren<T>();
        T GetComponentInParent<T>();
        T[] GetComponents<T>();
        T[] GetComponentsInChildren<T>(bool includeInactive = false);
        T[] GetComponentsInParent<T>(bool includeInactive = false);
        Component GetComponent(Type t);
        Component GetComponentInChildren(Type t);
        Component GetComponentInParent(Type t);
        Component[] GetComponents(Type t);
        Component[] GetComponentsInChildren(Type t, bool includeInactive = false);
        Component[] GetComponentsInParent(Type t, bool includeInactive = false);
    }
    
	public enum InventoryAddResult
	{
		Full,
		Partial,
        AppendedFull,
		Rejected
	}
}