using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
	public interface IAmmoEffect
    {
        void Hit (RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource);
	}
}