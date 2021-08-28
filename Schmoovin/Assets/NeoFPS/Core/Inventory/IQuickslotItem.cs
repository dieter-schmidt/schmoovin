using System;
using System.Collections;
using UnityEngine;

namespace NeoFPS
{
	public interface IQuickSlotItem
	{
		IQuickSlots slots { get; }
        IWieldable wieldable { get; }

		Sprite displayImage { get; }

		int quickSlot { get; }
		bool isSelected { get; }
		bool isSelectable { get; }
		bool isDroppable { get; }

        void OnSelect ();
        Waitable OnDeselect ();
        void OnDeselectInstant ();
        bool DropItem(Vector3 position, Vector3 forward, Vector3 velocity);
        
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
}