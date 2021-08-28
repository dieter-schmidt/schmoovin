using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
    public interface IProjectile
    {
        void Fire(Vector3 position, Vector3 velocity, float gravity, IAmmoEffect effect, Transform ignoreRoot, LayerMask layers, IDamageSource damageSource = null, bool wait1 = false);
        void Teleport(Vector3 position, Quaternion rotation, bool relativeRotation = true);

        event UnityAction onTeleported;
        event UnityAction onHit;

        GameObject gameObject { get; }
    }
}