using UnityEngine;

namespace NeoCC
{
    public struct NeoCharacterControllerHit
    {
        /// <summary>
        /// The character controller that collided.
        /// </summary>
        public INeoCharacterController controller;

        /// <summary>
        /// The type of the collision.
        /// </summary>
        public NeoCharacterCollisionFlags collisionFlags;

        /// <summary>
        /// The collider that the character collided with.
        /// </summary>
        public Collider collider;

        /// <summary>
        /// The rigidbody attached to the object the character collided with (can be null).
        /// </summary>
        public Rigidbody rigidbody;

        /// <summary>
        /// The transform of the object the character collided with.
        /// </summary>
        public Transform transform;

        /// <summary>
        /// The hit point of the current collision.
        /// </summary>
        public Vector3 point;

        /// <summary>
        /// The hit normal of the current collision.
        /// </summary>
        public Vector3 normal;

        /// <summary>
        /// The direction the character was moving when it collided.
        /// </summary>
        public Vector3 moveDirection;

        public NeoCharacterControllerHit(
            INeoCharacterController controller,
            NeoCharacterCollisionFlags collisionFlags,
            Collider collider,
            Rigidbody rigidbody,
            Transform transform,
            Vector3 point,
            Vector3 normal,
            Vector3 moveDirection
        )
        {
            this.controller = controller;
            this.collisionFlags = collisionFlags;
            this.collider = collider;
            this.rigidbody = rigidbody;
            this.transform = transform;
            this.point = point;
            this.normal = normal;
            this.moveDirection = moveDirection;
        }
    }
}
