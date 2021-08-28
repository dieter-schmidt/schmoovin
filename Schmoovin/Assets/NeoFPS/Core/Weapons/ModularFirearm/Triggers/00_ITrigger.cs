using System;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
	public interface ITrigger
	{
		IModularFirearm firearm { get; }

		void Enable ();
		void Disable ();

		bool blocked { get; set; } // Use IModularFirearm.AddTriggerBlocker/RemoveTriggerBlocker for ref counted system instead of setting from here (much safer)
		bool pressed { get; }
        bool cancelOnReload { get; }

        void Press ();
		void Release ();
        void Cancel();

		event UnityAction onShoot;
		event UnityAction<bool> onShootContinuousChanged;
		event UnityAction<bool> onStateChanged;
    }
}