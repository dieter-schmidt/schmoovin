using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.CharacterMotion.Parameters
{
    [MotionGraphElement("Event", "My Event")]
    public class EventParameter : MotionGraphParameter
    {
		private event UnityAction onInvoked;

		public void Invoke ()
		{
			if (onInvoked != null)
				onInvoked ();
		}

		public void AddListener (UnityAction listener)
		{
			onInvoked += listener;
		}

		public void RemoveListener (UnityAction listener)
		{
			onInvoked -= listener;
		}
    }
}