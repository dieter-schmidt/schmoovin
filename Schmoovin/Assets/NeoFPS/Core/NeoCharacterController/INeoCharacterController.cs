using System;
using UnityEngine;

namespace NeoCC
{
    /// <summary>
    /// INeoCharacterController is a character controller, which handles kinematic character movement and collision resolution within a scene.
    /// </summary>
    public interface INeoCharacterController
    {
        /// <summary>
        /// The velocity of the character during the last move phase with some corrections such as ignoring the vertical movement of steps.
        /// </summary>
        Vector3 velocity { get; }

        /// <summary>
        /// The actual velocity of the character during the last move phase with no modifications applied.
        /// </summary>
        Vector3 rawVelocity { get; }

        /// <summary>
        /// The velocity from the character input during the last move phase, before collision response was calculated.
        /// </summary>
        Vector3 targetVelocity { get; }

        /// <summary>
        /// The collision layers the controller will collide with when moving.
        /// </summary>
        LayerMask collisionMask { get; set; }

        /// <summary>
        /// The collision layers the controller will depenetrate from. Use this to prevent small rigidbodies from causing the character to move out of the way
        /// </summary>
        LayerMask depenetrationMask { get; set; }

        /// <summary>
        /// Is the character in contact with a ground surface.
        /// </summary>
        bool isGrounded { get; }

        /// <summary>
        /// The amount of time the controller has been airborne without a ground contact.
        /// </summary>
        float airTime { get; }

        /// <summary>
        /// The normal of the last ground contact.
        /// </summary>
        Vector3 groundNormal { get; }

        /// <summary>
        /// The normal of the ground surface for the last contact. If the controller is on a ledge, this is the top surface of the ledge.
        /// </summary>
        Vector3 groundSurfaceNormal { get; }

        /// <summary>
        /// The height below which the character controller should snap to the ground.
        /// </summary>
        float groundSnapHeight { get; set; }

        /// <summary>
        /// The collision flags for detecting scene collisions while moving.
        /// </summary>
        NeoCharacterCollisionFlags collisionFlags { get; }

        /// <summary>
        /// Should the character move with platforms it's in contact with. Defaults to false.
        /// </summary>
        bool ignorePlatforms { get; set; }

        /// <summary>
        /// The platform the character is currently affected by.
        /// </summary>
        IMovingPlatform platform { get; }

        /// <summary>
        /// Should the character inherit yaw rotation from moving platforms? Defaults to true.
        /// </summary>
        bool inheritPlatformYaw { get; set; }

        /// <summary>
        /// Should the character inherit velocity along its up vector from moving platforms? Defaults to false.
        /// </summary>
        NeoCharacterVelocityInheritance inheritPlatformVelocity { get; set; }

        /// <summary>
        /// The maximum upwards slope the character can climb. Any horizontal movement into the slope will not become vertical movement above this angle.
        /// </summary>
        float slopeLimit { get; set; }

        /// <summary>
        /// The friction of ground contacts when standiong on a slope. At 1, all downward velocity will be cancelled out. At 0, the character will slide down the slope.
        /// </summary>
        float slopeFriction { get; set; }

        /// <summary>
        /// The friction of ground contacts when the controller is overhanging a ledge. At 1, the character will not slide off the ledge.
        /// </summary>
        float ledgeFriction { get; set; }

        /// <summary>
        /// The move vector from the last fixed update move.
        /// </summary>
        Vector3 lastFrameMove { get; }

        /// <summary>
        /// The character will step up onto ledges with this height without losing horizontal speed.
        /// The upward movement will not be factored into the character velocity at the end of the move.
        /// </summary>
        float stepHeight { get; set; }

        /// <summary>
        /// The up axis for the controller in world space.
        /// </summary>
        Vector3 up { get; }

        /// <summary>
        /// The forward axis of the controller in world space.
        /// </summary>
        Vector3 forward { get; }

        /// <summary>
        /// The right axis of the controller in world space.
        /// </summary>
        Vector3 right { get; }

        /// <summary>
        /// The gravity vector applied to this character.
        /// </summary>
        Vector3 gravity { get; }

        /// <summary>
        /// The variable gravity setup for this character if applicable.
        /// </summary>
        INeoCharacterVariableGravity characterGravity { get; }

        /// <summary>
        /// The height of the character capsule.
        /// </summary>
        float height { get; set; }

        /// <summary>
        /// The radius of the character capsule.
        /// </summary>
        float radius { get; set; }

        /// <summary>
        /// When performing the move loop, the capsule is shrunk by this amount. When testing for contacts it is grown by this amount.
        /// </summary>
        float skinWidth { get; set; }

        /// <summary>
        /// The mass of the character.
        /// </summary>
        float mass { get; set; }

        /// <summary>
        /// Should the character collide with scene colliders (static or otherwise). Defaults to true.
        /// </summary>
        bool collisionsEnabled { get; set; }

        /// <summary>
        /// Should the character push dynamic rigidbodies. Defaults to true.
        /// </summary>
        bool pushRigidbodies { get; set; }

        /// <summary>
        /// Should the character push other character controllers. Defaults to true.
        /// </summary>
        bool pushCharacters { get; set; } 

        /// <summary>
        /// Can the character be pushed by other character controllers. Defaults to true.
        /// </summary>
        bool pushedByCharacters { get; set; }

        /// <summary>
        /// If this is true, any forces applied with AddForce() will be ignored. Defaults to false.
        /// </summary>
        bool ignoreExternalForces { get; set; }

        /// <summary>
        /// A manual initialisation function to prevent issues with script execution order
        /// </summary>
        void Initialise();

        /// <summary>
        /// Attach input callbacks to the character controller. The controller movement needs to occur in a specific sequence, and so will
        /// request input when required.
        /// </summary>
        /// <param name="moveCallback">A callback for the character move, called each fixed update.</param>
        void SetMoveCallback(NeoCharacterControllerDelegates.GetMoveVector moveCallback, NeoCharacterControllerDelegates.OnMoved onMovedCallback);

        /// <summary>
        /// Check if the character has space to change its height to the specified value without overlapping the environment.
        /// </summary>
        /// <param name="h">The target height. The height can not be smaller than double the radius.</param>
        /// <returns>Can the character expand or not.</returns>
        bool IsHeightRestricted(float h);

        /// <summary>
        /// Try to set the character capsule height to a specific value. The height can not be smaller than double the radius.
        /// If the character height is restricted by the environment, then the character height will not be changed.
        /// </summary>
        /// <param name="h">The target height.</param>
        /// <param name="fromNormalisedHeight">The point to resize from. 0 is the bottom of the current capsule, and 1 is the top.</param>
        /// <returns>Did the character height change (true), or was it restricted (false).</returns>
        bool TrySetHeight(float h, float fromNormalisedHeight = 0f);

        /// <summary>
        /// Set the height of the character to a specific value. The height can not be smaller than double the radius.
        /// If the resulting capsule would overlap the environment then the controller will keep trying until there is space for the capsule to be resized.
        /// </summary>
        /// <param name="h">The target height.</param>
        /// <param name="fromNormalisedHeight">The point to resize from. 0 is the bottom of the current capsule, and 1 is the top.</param>
        void SetHeight(float h, float fromNormalisedHeight = 0f);

        /// <summary>
        /// Try to reset the character capsule height to its value upon start.
        /// If the character height is restricted by the environment, then the character height will not be changed.
        /// </summary>
        /// <param name="fromNormalisedHeight">The point to resize from. 0 is the bottom of the current capsule, and 1 is the top.</param>
        /// <returns>Did the character height change (true), or was it restricted (false).</returns>
        bool TryResetHeight(float fromNormalisedHeight = 0f);

        /// <summary>
        /// Reset the character capsule height to its value upon start.
        /// If the character height is restricted, it will keep trying until it has space to resize, or the height change is cancelled.
        /// </summary>
        /// <param name="fromNormalisedHeight">The point to resize from. 0 is the bottom of the current capsule, and 1 is the top.</param>
        void ResetHeight(float fromNormalisedHeight = 0f);

        /// <summary>
        /// Cancel any delayed height change and keep the current height.
        /// </summary>
        void CancelHeightChange();

        /// <summary>
        /// Try to reset the character capsule radius to its value upon start.
        /// If the resulting capsule would overlap the environment then the character capsule will not be changed.
        /// </summary>
        /// <param name="r">The target radius.</param>
        /// <returns>Did the character radius change (true), or was it restricted (false).</returns>
        bool TrySetRadius(float r);

        /// <summary>
        /// Set the radius of the character to a specific value.
        /// If the resulting capsule would overlap the environment then the controller will keep trying until there is space for the capsule to be resized.
        /// </summary>
        /// <param name="r">The target radius.</param>
        void SetRadius(float r);

        /// <summary>
        /// Try to reset the character capsule radius to its value upon start.
        /// If the resulting capsule would overlap the environment then the character capsule will not be changed.
        /// </summary>
        /// <returns>Did the character radius change (true), or was it restricted (false).</returns>
        bool TryResetRadius();

        /// <summary>
        /// Reset the character capsule radius to its value upon start.
        /// If the resulting capsule would overlap the environment then the controller will keep trying until there is space for the capsule to be resized.
        /// </summary>
        void ResetRadius();

        /// <summary>
        /// An event that is fired when the character changes height.
        /// If the character height is restricted, then a height change will be delayed until the character has space to resize.
        /// </summary>
        event NeoCharacterControllerDelegates.OnHeightChange onHeightChanged;

        /// <summary>
        /// A callback that is used by the character to resolve collisions with other INeoCharacterController characters.
        /// The character should implement and expose a default handler.
        /// </summary>
        NeoCharacterControllerDelegates.OnHitCharacter characterCollisionHandler { get; set; }

        /// <summary>
        /// A callback that is used by the character to resolve collisions with dynamic rigidbodies.
        /// The character should implement and expose a default handler.
        /// </summary>
        NeoCharacterControllerDelegates.OnHitRigidbody rigidbodyCollisionHandler { get; set; }

        /// <summary>
        /// An event fired when the character collides with an obstacle while calculating its move.
        /// Not all collisions fire an event. For example, colliding with a step and stepping up is handled silently.
        /// </summary>
        event NeoCharacterControllerDelegates.OnCharacterControllerHit onControllerHit;

        /// <summary>
        /// An event fired when the character is teleported to a new position.
        /// </summary>
        event NeoCharacterControllerDelegates.OnTeleported onTeleported;

        /// <summary>
        /// Move to the new position and rotation. Updates velocity and properties to match new frame of reference
        /// </summary>
        /// <param name="position">The position the character should move to.</param>
        /// <param name="rotation">The new rotation for the character.</param>
        /// <param name="relativeRotation">Is the rotation parameter relative to the current rotation or absolute?</param>
        void Teleport(Vector3 position, Quaternion rotation, bool relativeRotation = true);

        /// <summary>
        /// Add a force to the character outside of the usual movement logic (eg. explosions, etc)
        /// </summary>
        /// <param name="force">Force vector in world coordinates.</param>
        /// <param name="mode">Type of force to apply. See the <see href="https://docs.unity3d.com/ScriptReference/ForceMode.html">Unity Scripting Reference.</see></param>
        void AddForce (Vector3 force, ForceMode mode = ForceMode.Force, bool disableGroundSnapping = false);

        /// <summary>
        /// Set all velocity variables to a specific vector
        /// </summary>
        void SetVelocity(Vector3 v);

        /// <summary>
        /// Reset all velocity variables to zero
        /// </summary>
        void ResetVelocity();

        /// <summary>
        /// Reset the vertical component of all velocity variables to zero
        /// </summary>
        void ResetVerticalVelocity();

        /// <summary>
        /// Reset the horizontal component of all velocity variables to zero
        /// </summary>
        void ResetHorizontalVelocity();

        bool RayCast(float normalisedHeight, Vector3 castVector, Space space, int layerMask = -5);


        bool RayCast(float normalisedHeight, Vector3 castVector, Space space, out RaycastHit hit, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);


        bool SphereCast(float normalisedHeight, Vector3 castVector, Space space, int layerMask = -5);


        bool SphereCast(float normalisedHeight, Vector3 castVector, Space space, out RaycastHit hit, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);


        bool CapsuleCast(Vector3 castVector, Space space, int layerMask = -5);


        bool CapsuleCast(Vector3 castVector, Space space, out RaycastHit hit, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);

        T GetComponent<T>();
        Transform transform { get; }

#if UNITY_EDITOR

        Vector3 debugExternalForceMove { get; }
        bool debugSnapToGround { get; }
        int debugMoveIterations { get; }
        int debugDepenetrationCount { get; }

#endif
    }
}
