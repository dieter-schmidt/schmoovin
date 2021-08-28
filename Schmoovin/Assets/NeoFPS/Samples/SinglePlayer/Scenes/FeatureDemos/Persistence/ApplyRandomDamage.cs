using NeoFPS.SinglePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.Samples
{
    public class ApplyRandomDamage : MonoBehaviour
    {
        [SerializeField]
        private int m_MinDamage = 1;
        [SerializeField]
        private int m_MaxDamage = 10;

        public void DamagePlayer()
        {
            var health = FpsSoloCharacter.localPlayerCharacter.GetComponent<IHealthManager>();
            health.AddDamage(Random.Range(m_MinDamage, m_MaxDamage + 1));
        }
    }
}