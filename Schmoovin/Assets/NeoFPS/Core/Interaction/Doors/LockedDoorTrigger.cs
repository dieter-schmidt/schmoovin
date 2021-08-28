using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-lockeddoortrigger.html")]
    public class LockedDoorTrigger : LockedTriggerZone
    {
        [SerializeField, Tooltip("The door to open.")]
        private DoorBase m_Door = null;

        protected override void OnCharacterEntered(BaseCharacter c)
        {
            base.OnCharacterEntered(c);
            if (m_Door != null)
                m_Door.Open();
        }

        protected override void OnCharacterExited(BaseCharacter c)
        {
            base.OnCharacterExited(c);
            if (m_Door != null)
                m_Door.Close();
        }
    }
}