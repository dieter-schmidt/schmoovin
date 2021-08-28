using NeoCC;
using System;
using UnityEngine;

namespace NeoFPSEditor.CharacterMotion.Debugger
{
    [Serializable]
    public struct MotionControllerDebugSnapshot
    {
        public int frame;
        public string state;
        public string stateType;
        public Vector3 targetMove;
        public Vector3 previousMove;
        public bool isGrounded;
        public Vector2 inputDirection;
        public float inputScale;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 rawVelocity;
        public Vector3 targetVelocity;
        public Vector3 groundNormal;
        public Vector3 groundSurfaceNormal;
        public Vector3 upTarget;
        public float ledgeFriction;
        public float slopeFriction;
        public float radius;
        public float height;
        public bool snapToGround;
        public float groundSnapHeight;
        public bool applyGravity;
        public Vector3 gravity;
        public bool ignoreExternalForces;
        public Vector3 externalForceMove;
        public int depenetrations;
        public int moveIterations;
        public string platform;
        public bool ignorePlatforms;
        public NeoCharacterCollisionFlags collisionFlags;
    }
}