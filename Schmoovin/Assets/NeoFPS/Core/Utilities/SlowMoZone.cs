using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-slowmozone.html")]
    public class SlowMoZone : CharacterTriggerZone
    {
        [SerializeField, Tooltip("The time scale inside this zone")]
        private float m_TimeScale = 0.25f;

        protected override void OnCharacterEntered(ICharacter c)
        {
            base.OnCharacterEntered(c);
            var slowmo = c.GetComponent<ISlowMoSystem>();
            if (slowmo != null)
                slowmo.SetTimeScale(m_TimeScale, 0f);
        }

        protected override void OnCharacterExited(ICharacter c)
        {
            base.OnCharacterExited(c);
            var slowmo = c.GetComponent<ISlowMoSystem>();
            if (slowmo != null)
                slowmo.ResetTimescale();
        }
    }
}
