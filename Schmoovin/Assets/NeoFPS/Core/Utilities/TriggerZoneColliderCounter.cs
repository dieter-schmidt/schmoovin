using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-triggerzonecollidercounter.html")]
    public class TriggerZoneColliderCounter : MonoBehaviour
    {
        [SerializeField, Tooltip("The valid layers for objects to track. Colliders on another layer will be ignored.")]
        private LayerMask m_ValidLayers = 0;

        List<Collider> m_Collisions = new List<Collider>();

        public int numCollisions
        {
            get { return m_Collisions.Count; }
        }

        void OnTriggerEnter (Collider other)
        {
            if ((m_ValidLayers.value & other.gameObject.layer) != 0)
                m_Collisions.Add(other);
        }

        void OnTriggerExit (Collider other)
        {
            int index = m_Collisions.IndexOf(other);
            if (index != -1)
                m_Collisions.RemoveAt(index);
        }
    }
}

