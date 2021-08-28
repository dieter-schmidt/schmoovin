using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-mb-fpsinventorywieldabledrop.html")]
    public class FpsInventoryWieldableDrop : MonoBehaviour
    {
		[SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The object rigidbody for the drop. This will be thrown away from the character that drops it.")]
        private Rigidbody m_RigidBody = null;

        [SerializeField, Tooltip("The pickup for the item. This allows characters to pick it back up. This will be initialised with the correct quantity based on the dropper's inventory.")]
        private InteractivePickup m_Pickup = null;

        public InteractivePickup pickup
        {
            get { return m_Pickup; }
        }

        #if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (m_RigidBody == null)
                m_RigidBody = GetComponentInChildren<Rigidbody>();
            if (m_Pickup == null)
                m_Pickup = GetComponentInChildren<InteractivePickup>();
        }
        #endif
       
        public virtual void Drop(IInventoryItem item, Vector3 position, Vector3 forward, Vector3 velocity)
        {
            m_RigidBody.position = position;
            m_RigidBody.rotation = Quaternion.LookRotation(forward, Vector3.up);
            m_RigidBody.velocity = velocity;

            if (m_Pickup != null && m_Pickup.item != null)
                m_Pickup.item.quantity = item.quantity;
        }
    }
}