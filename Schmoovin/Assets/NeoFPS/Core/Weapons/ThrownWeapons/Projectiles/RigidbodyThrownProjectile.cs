using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ThrownProjectiles
{
    [RequireComponent(typeof(Rigidbody))]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-rigidbodythrownprojectile.html")]
    public class RigidbodyThrownProjectile : ThrownWeaponProjectile
    {
        [SerializeField, Tooltip("The angular velocity when thrown.")]
        private Vector3 m_AngularVelocity = Vector3.zero;

        private Rigidbody m_Rigidbody = null;

        protected override void Awake()
        {
            base.Awake();
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        public override void Throw(Vector3 velocity, IDamageSource source)
        {
            base.Throw(velocity, source);
            m_Rigidbody.velocity = velocity;
            m_Rigidbody.angularVelocity = m_AngularVelocity;
        }
    }
}
