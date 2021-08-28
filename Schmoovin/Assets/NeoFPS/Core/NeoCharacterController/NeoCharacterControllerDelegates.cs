using System;
using UnityEngine;

namespace NeoCC
{
    public static class NeoCharacterControllerDelegates
    {
        /// <summary>
        /// An event fired when the character controller changes height
        /// </summary>
        /// <param name="newHeight">The new height of the character capsule.</param>
        /// <param name="rootOffset">The distance the base of the capsule moved vertically.</param>
        public delegate void OnHeightChange(float newHeight, float rootOffset);

        /// <summary>
        /// A callback for requesting input from registered inputs. Can be used to retrieve either a move vector or velocity vector.
        /// </summary>
        /// <param name="move">The output move vector for this tick.</param>
        /// <param name="applyGravity">Should the character controller add gravity to the move.</param>
        /// <param name="stickToGround">Should the character snap to the  ground.</param>
        public delegate void GetMoveVector(out Vector3 move, out bool applyGravity, out bool stickToGround);

        /// <summary>
        /// An event fired after the character controller has completed a move update
        /// </summary>
        public delegate void OnMoved();

        /// <summary>
        /// An event fired when the character controller is teleported
        /// </summary>
        public delegate void OnTeleported();

        /// <summary>
        /// An event called when the character controller hits another object during a move.
        /// </summary>
        /// <param name="hit">A struct containing relevant data about the collision.</param>
        public delegate void OnCharacterControllerHit(NeoCharacterControllerHit hit);

        /// <summary>
        /// An event called when the character controller hits another character controller during a move.
        /// </summary>
        /// <param name="other">The character controller that was hit.</param>
        /// <param name="normal">The normal of the impact.</param>
        /// <param name="flags">The collision flags for this specific impact.</param>
        public delegate void OnHitCharacter(INeoCharacterController other, Vector3 normal, NeoCharacterCollisionFlags flags);

        /// <summary>
        /// An event called when the character controller hits a non-kinematic rigidbody during a move.
        /// </summary>
        /// <param name="rigidbody">The rigidbody that was hit.</param>
        /// <param name="point">The hit point of the impact.</param>
        /// <param name="normal">The normal of the impact.</param>
        /// <param name="flags">The collision flags for this specific impact.</param>
        public delegate void OnHitRigidbody(Rigidbody rigidbody, Vector3 point, Vector3 normal, NeoCharacterCollisionFlags flags);
    }
}
