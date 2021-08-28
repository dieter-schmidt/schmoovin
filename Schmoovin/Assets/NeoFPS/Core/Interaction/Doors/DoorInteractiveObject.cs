using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-doorinteractiveobject.html")]
    public class DoorInteractiveObject : InteractiveObject
    {
		[SerializeField, Tooltip("The door to open (will accept any door that inherits from `DoorBase`).")]
        private DoorBase m_Door = null;

        public override void Interact(ICharacter character)
        {
            base.Interact(character);

            if (m_Door == null)
                return;

            if (m_Door.state == DoorState.Closed || m_Door.state == DoorState.Closing)
                m_Door.Open(m_Door.reversible && !m_Door.IsTransformInFrontOfDoor(character.motionController.localTransform));
            else
                m_Door.Close();
        }
    }
}