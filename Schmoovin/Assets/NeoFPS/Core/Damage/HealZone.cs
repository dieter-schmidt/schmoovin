using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-healzone.html")]
    public class HealZone : CharacterTriggerZonePersistant
    {
        [SerializeField, Tooltip("The amount of health to apply to the player character per second")]
        private float m_HealthPerSecond = 10f;

        protected override void OnCharacterStay(ICharacter c)
        {
            var hm = c.GetComponent<IHealthManager>();
            if (hm != null)
                hm.AddHealth(m_HealthPerSecond * Time.deltaTime);

            base.OnCharacterStay(c);
        }
    }
}