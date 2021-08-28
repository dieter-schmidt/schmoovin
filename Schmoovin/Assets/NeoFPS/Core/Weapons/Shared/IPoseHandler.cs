using UnityEngine;

namespace NeoFPS
{
    public interface IPoseHandler
    {
        void SetPose(Vector3 position, Quaternion rotation, float duration);
        void SetPose(Vector3 position, CustomPositionInterpolation posInterp, Quaternion rotation, CustomRotationInterpolation rotInterp, float duration);
        void ResetPose(float duration);
        void ResetPose(CustomPositionInterpolation posInterp, CustomRotationInterpolation rotInterp, float duration);
    }

    public delegate Vector3 CustomPositionInterpolation(Vector3 from, Vector3 to, float lerp);
    public delegate Quaternion CustomRotationInterpolation(Quaternion from, Quaternion to, float lerp);
}
