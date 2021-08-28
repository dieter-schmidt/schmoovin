using System;

namespace NeoFPSEditor.CharacterMotion.Debugger
{
    public enum GraphContents
    {
        Speed,
        RawSpeed,
        HorizontalSpeed,
        RawHorizontalSpeed,
        UpVelocity,
        RawUpVelocity,
        WorldHeight,
        IsGrounded,
        //State,
        GroundSlope,
        GroundSurfaceSlope,
        InputScale,
        ExternalForceMagnitude,
        Depenetrations,
        MoveIterations
    }

    [Flags]
    public enum MotionControllerDebugFilter
    {
        None = 0,
        PreviousMove = 1 << 0,
        IsGrounded = 1 << 1,
        Position = 1 << 2,
        Rotation = 1 << 3,
        Input = 1 << 4,
        Velocity = 1 << 5,
        Speed = 1 << 6,
        HorizontalSpeed = 1 << 7,
        UpVelocity = 1 << 8,
        CollisionFlags = 1 << 9,
        GroundNormals = 1 << 10,
        SlopeAngles = 1 << 11,
        Friction = 1 << 12,
        Dimensions = 1 << 13,
        Gravity = 1 << 14,
        UpVector = 1 << 15,
        GroundSnapping = 1 << 16,
        ExternalForces = 1 << 17,
        Depenetrations = 1 << 18,
        MoveIterations = 1 << 19,
        Platforms = 1 << 20
    }
}