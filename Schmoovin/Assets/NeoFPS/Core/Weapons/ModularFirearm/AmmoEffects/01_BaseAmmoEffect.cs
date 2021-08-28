using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NeoFPS.ModularFirearms
{
	public abstract class BaseAmmoEffect : MonoBehaviour, IAmmoEffect, IFirearmModuleValidity
    {
        [SerializeField, Tooltip("The type of damage the weapon should do with this ammo.")]
        private DamageType m_DamageType = DamageType.Default;

        public DamageType damageType
        {
            get { return m_DamageType; }
        }

        public abstract void Hit (RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource);

        public virtual bool isModuleValid
        {
            get { return true; }
        }
	}
}