using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-timedslowmozone.html")]
    public class TimedSlowMoZone : CharacterTriggerZone
    {
        [SerializeField, Tooltip("The time scale inside this zone")]
        private float m_TimeScale = 0.25f;

        [SerializeField, Tooltip("The duration the slow-mo effect should last")]
        private float m_Duration = 5f;

        private void OnValidate()
        {
            m_Duration = Mathf.Clamp(m_Duration, 1f, 100f);
        }

        protected override void OnCharacterEntered(ICharacter c)
        {
            base.OnCharacterEntered(c);
            var slowmo = c.GetComponent<ISlowMoSystem>();
            if (slowmo != null)
            {
                // Reset charge to 1
                slowmo.charge = 1f;
                // Slow with a rate based on duration
                slowmo.SetTimeScale(m_TimeScale, 1f / m_Duration);
            }
        }
    }
}
