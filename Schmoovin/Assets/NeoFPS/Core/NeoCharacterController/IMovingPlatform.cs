using UnityEngine;

namespace NeoCC
{
    public interface IMovingPlatform
    {
        /// <summary>
        /// The current fixed update position of the platform in world space (used for interpolation).
        /// </summary>
        Vector3 fixedPosition { get; }

        /// <summary>
        /// The position of the platform in world space on the last fixed update frame (used for interpolation).
        /// </summary>
        Vector3 previousPosition { get; }

        /// <summary>
        /// The current fixed update rotation of the platform in world space (used for interpolation).
        /// </summary>
        Quaternion fixedRotation { get; }

        /// <summary>
        /// The rotation of the platform in world space on the last fixed update frame (used for interpolation).
        /// </summary>
        Quaternion previousRotation { get; }
    }
}
