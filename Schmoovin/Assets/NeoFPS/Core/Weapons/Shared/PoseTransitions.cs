using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public static class PoseTransitions
    {
        public static Vector3 PositionLerp(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, lerp);
        }

        public static Vector3 PositionEaseInQuadratic(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, EasingFunctions.EaseInQuadraticUnclamped(lerp));
        }
        public static Vector3 PositionEaseOutQuadratic(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, EasingFunctions.EaseOutQuadraticUnclamped(lerp));
        }
        public static Vector3 PositionEaseInOutQuadratic(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, EasingFunctions.EaseInOutQuadraticUnclamped(lerp));
        }

        public static Vector3 PositionEaseInCubic(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, EasingFunctions.EaseInCubicUnclamped(lerp));
        }
        public static Vector3 PositionEaseOutCubic(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, EasingFunctions.EaseOutCubicUnclamped(lerp));
        }
        public static Vector3 PositionEaseInOutCubic(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, EasingFunctions.EaseInOutCubicUnclamped(lerp));
        }

        public static Vector3 PositionSwingUp(Vector3 source, Vector3 target, float lerp)
        {
            return new Vector3(
                Mathf.Lerp(source.x, target.x, EasingFunctions.EaseOutQuadraticUnclamped(lerp)),
                Mathf.Lerp(source.y, target.y, EasingFunctions.EaseInQuadraticUnclamped(lerp)),
                Mathf.Lerp(source.z, target.z, lerp)
                );
        }

        public static Vector3 PositionSwingAcross(Vector3 source, Vector3 target, float lerp)
        {
            return new Vector3(
                Mathf.Lerp(source.x, target.x, EasingFunctions.EaseInQuadraticUnclamped(lerp)),
                Mathf.Lerp(source.y, target.y, EasingFunctions.EaseOutQuadraticUnclamped(lerp)),
                Mathf.Lerp(source.z, target.z, lerp)
                );
        }

        public static Vector3 PositionOvershootIn(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, EasingFunctions.EaseInOvershootUnclamped(lerp));
        }
        public static Vector3 PositionOvershootOut(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, EasingFunctions.EaseOutOvershootUnclamped(lerp));
        }

        public static Vector3 PositionSpringIn(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, EasingFunctions.EaseInSpringUnclamped(lerp));
        }
        public static Vector3 PositionSpringOut(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, EasingFunctions.EaseOutSpringUnclamped(lerp));
        }

        public static Vector3 PositionBounceIn(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, EasingFunctions.EaseInBounceUnclamped(lerp));
        }
        public static Vector3 PositionBounceOut(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, EasingFunctions.EaseOutBounceUnclamped(lerp));
        }

        public static Quaternion RotationLerp(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, lerp);
        }

        public static Quaternion RotationSlerp(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Slerp(source, target, lerp);
        }

        public static Quaternion RotationEaseInQuadratic(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, EasingFunctions.EaseInQuadraticUnclamped(lerp));
        }
        public static Quaternion RotationEaseOutQuadratic(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, EasingFunctions.EaseOutQuadraticUnclamped(lerp));
        }
        public static Quaternion RotationEaseInOutQuadratic(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, EasingFunctions.EaseInOutQuadraticUnclamped(lerp));
        }

        public static Quaternion RotationEaseInCubic(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, EasingFunctions.EaseInCubicUnclamped(lerp));
        }
        public static Quaternion RotationEaseOutCubic(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, EasingFunctions.EaseOutCubicUnclamped(lerp));
        }
        public static Quaternion RotationEaseInOutCubic(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, EasingFunctions.EaseInOutCubicUnclamped(lerp));
        }

        public static Quaternion RotationOvershootIn(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, EasingFunctions.EaseInOvershootUnclamped(lerp));
        }
        public static Quaternion RotationOvershootOut(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, EasingFunctions.EaseOutOvershootUnclamped(lerp));
        }

        public static Quaternion RotationSpringIn(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, EasingFunctions.EaseInSpringUnclamped(lerp));
        }
        public static Quaternion RotationSpringOut(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, EasingFunctions.EaseOutSpringUnclamped(lerp));
        }

        public static Quaternion RotationBounceIn(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, EasingFunctions.EaseInBounceUnclamped(lerp));
        }
        public static Quaternion RotationBounceOut(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, EasingFunctions.EaseOutBounceUnclamped(lerp));
        }
    }
}