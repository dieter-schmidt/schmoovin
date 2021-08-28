using System;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
	public interface IReloader
	{
		IModularFirearm firearm { get; }

		void Enable ();
		void Disable ();

		bool empty { get; }
		bool full { get; }
		bool canReload { get; }
		int magazineSize { get; }
		int currentMagazine { get; set; }
        int startingMagazine { get; set; }

		void DecrementMag (int amount);

		event UnityAction<IModularFirearm, int> onCurrentMagazineChange;
		event UnityAction<IModularFirearm> onReloadStart;
		event UnityAction<IModularFirearm> onReloadComplete;

		bool isReloading { get; }
		Waitable Reload ();

        bool interruptable { get; }
        void Interrupt ();

        FirearmDelayType reloadDelayType { get; }
		void ManualReloadPartial ();
		void ManualReloadComplete ();
    }
}