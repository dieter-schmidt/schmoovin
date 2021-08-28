using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public interface IGuidedProjectileTargetTracker
    {
        bool GetTargetPosition(out Vector3 targetPosition);
    }
}