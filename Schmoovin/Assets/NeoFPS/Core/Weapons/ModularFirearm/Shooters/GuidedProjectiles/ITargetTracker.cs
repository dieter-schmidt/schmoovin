using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
    public interface ITargetTracker
    {
        event UnityAction<ITargetTracker> onDestroyed;

        bool hasTarget { get; }

        void SetTargetPosition(Vector3 target);
        void SetTargetCollider(Collider target);
        void SetTargetTransform(Transform target);
        void SetTargetTransform(Transform target, Vector3 offset, bool worldOffset);
        void ClearTarget();
    }
}
