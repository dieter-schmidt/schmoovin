using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
    public interface ITargetLock
    {
        event UnityAction<Collider, bool> onTargetLock;
        event UnityAction<Collider> onTargetLockBroken;
        event UnityAction<Collider, float> onTargetLockStrengthChanged;
    }
}
