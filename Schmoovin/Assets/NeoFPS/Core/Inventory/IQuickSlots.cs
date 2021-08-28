using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
	public interface IQuickSlots
	{
		event UnityAction<IQuickSlotItem> onSelectionChanged;
		event UnityAction<int, IQuickSlotItem> onSlotItemChanged;
		event UnityAction<IQuickSlotItem> onItemDropped;

		void SetSlotItem (int slot, IQuickSlotItem item);
		IQuickSlotItem GetSlotItem (int slot);
		void ClearSlots ();

		int numSlots { get; }

		IQuickSlotItem selected { get; }
		bool SelectStartingSlot ();
		bool SelectSlot (int slot);
		bool SelectNextSlot ();
		bool SelectPreviousSlot ();

        bool LockSelectionToSlot(int index, Object o);
        bool LockSelectionToBackupItem(Object o, bool silent);
        bool LockSelectionToNothing(Object o, bool silent);
        void UnlockSelection(Object o);

        bool IsSlotSelectable (int index);

        bool SelectBackupItem(bool toggle, bool silent);
        bool isBackupItemSelected { get; }

        void AutoSwitchSlot (int slot);
		void SwitchSelection ();
		void DropSelected ();
	}
}