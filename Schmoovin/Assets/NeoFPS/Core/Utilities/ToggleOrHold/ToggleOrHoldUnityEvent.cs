using UnityEngine;
using UnityEngine.Events;
using System;


namespace NeoFPS
{
	[Serializable]
	public class ToggleOrHoldUnityEvent : ToggleOrHold
	{
        [SerializeField] private UnityEvent m_OnActivate = new UnityEvent();
        [SerializeField] private UnityEvent m_OnDeactivate = new UnityEvent();

        public UnityEvent onActivate { get { return m_OnActivate; } }
        public UnityEvent onDeactivate { get { return m_OnDeactivate; } }

		public ToggleOrHoldUnityEvent() : base(null)
		{ }

		public ToggleOrHoldUnityEvent(Func<bool> isBlockedCallback) : base(isBlockedCallback)
		{ }

		protected override void OnActivate ()
		{
			m_OnActivate.Invoke ();
		}

		protected override void OnDeactivate ()
		{
			m_OnDeactivate.Invoke ();
		}
	}
}