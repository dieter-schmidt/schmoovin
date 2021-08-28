using UnityEngine;
using System;
using UnityEngine.Events;


namespace NeoFPS
{
	public class ToggleOrHoldEvent : ToggleOrHold
	{
		public ToggleOrHoldEvent() : base(null)
		{}

		public ToggleOrHoldEvent(Func<bool> isBlockedCallback) : base(isBlockedCallback)
		{}

		public ToggleOrHoldEvent(UnityAction onAct, UnityAction onDeact) : base(null)
		{
			onActivate += onAct;
			onDeactivate += onDeact;
		}

		public ToggleOrHoldEvent(UnityAction onAct, UnityAction onDeact, Func<bool> isBlockedCallback) : base(isBlockedCallback)
		{
			onActivate += onAct;
			onDeactivate += onDeact;
		}

		public event UnityAction onActivate;
		public event UnityAction onDeactivate;

		protected override void OnActivate ()
		{
			if (onActivate != null)
				onActivate ();
		}

		protected override void OnDeactivate ()
		{
			if (onDeactivate != null)
				onDeactivate ();
		}
	}
}