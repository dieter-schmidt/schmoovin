using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-mb-interactivepickuptrigger.html")]
    public class InteractivePickupTrigger : InteractiveObject
    {
        IPickup m_Pickup = null;

        protected override void Start()
        {
            base.Start();

            m_Pickup = GetComponent<IPickup>();
            if (m_Pickup == null)
                Debug.LogError("InteractivePickupTrigger requires IPickup inherited behaviour attached to game object");
        }

        public override void Interact(ICharacter character)
        {
            if (character != null)
                m_Pickup.Trigger(character);

            base.Interact(character);
        }
    }
}
