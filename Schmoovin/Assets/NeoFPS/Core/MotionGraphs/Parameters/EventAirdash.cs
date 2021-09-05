using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.CharacterMotion.Parameters
{
    [MotionGraphElement("Event", "My Event")]
    public class EventAirdash : EventParameter
    {
		private event UnityAction onInvoked;

        private CameraFXController cameraFXController;

        public new void Invoke()
		{
            //DS
            onInvoked += startCameraFX;
            //DS

			if (onInvoked != null)
				onInvoked ();
		}

		//public void AddListener (UnityAction listener)
		//{
		//	onInvoked += listener;
		//}

		//public void RemoveListener (UnityAction listener)
		//{
		//	onInvoked -= listener;
		//}

        private void startCameraFX()
        {
            
        }
    }
}