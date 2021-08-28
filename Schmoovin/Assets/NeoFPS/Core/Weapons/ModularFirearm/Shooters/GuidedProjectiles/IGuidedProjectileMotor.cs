using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public interface IGuidedProjectileMotor
    {
        void SetStartingVelocity(Vector3 v);
        Vector3 GetVelocity(Vector3 currentPosition);
        Vector3 GetVelocity(Vector3 currentPosition, Vector3 targetPosition);
        void OnTeleport(Vector3 position, Quaternion rotation, bool relativeRotation);
    }
}