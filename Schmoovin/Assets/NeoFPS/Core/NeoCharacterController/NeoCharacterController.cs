// #define VISUALISE_MOVEMENT
// Uncomment the above and the comment at the bottom of the file to draw debug rays

using UnityEngine;
using NeoFPS;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using System.Collections.Generic;

namespace NeoCC
{
    /// <summary>
    /// NeoCharacterController is a character controller, which handles kinematic character movement and collision resolution within a scene.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mb-neocharactercontroller.html")]
    public class NeoCharacterController : MonoBehaviour, INeoCharacterController, INeoCharacterVariableGravity, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The physics layers that the character will depenetrate from. This cannot include anything outside of the collision matrix for the gameobject layer. Use this to filter out small dynamic props that should not influence the player.")]
        private LayerMask m_DepenetrationMask = -5;

        [SerializeField, Tooltip("When performing the move loop, the capsule is shrunk by this amount. When testing for contacts it is grown by this amount.")]
        private float m_SkinWidth = 0.005f;

        [SerializeField, Tooltip("The maximum distance above the ground to apply a \"sticky\" downforce on the frame after leaving the ground.")]
        private float m_SlopeLimit = 45f;
        
        [SerializeField, Range(0f, 1f), Tooltip("The friction of ground contacts when standiong on a slope. At 1, all downward velocity will be cancelled out. At 0, the character will slide down the slope.")]
        private float m_SlopeFriction = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("The friction of ground contacts when overhanging a ledge. At 1, the character will not slide off the ledge.")]
        private float m_LedgeFriction = 0f;

        [SerializeField, Tooltip("The angle (in degrees) from the vertical for a surface to be considered a wall.")]
        private float m_WallAngle = 5f;

        [SerializeField, Tooltip("The character will traverse any ledge up to their radius in height. If the step is equal to or below the step height then the character will not lose any horizontal speed when stepping up, and any vertical movement does not count to the character's velocity calculations.")]
        private float m_StepHeight = 0.3f;

        [SerializeField, Tooltip("The maximum slope angle of the top surface of a ledge for it to be considered a step and stepped onto")]
        private float m_StepMaxAngle = 30f;

        [SerializeField, Tooltip("The maximum distance above the ground to apply a \"sticky\" downforce on the frame after leaving the ground in certain conditions. This prevents leaving the ground when stepping onto down-slopes or off low steps.")]
        private float m_GroundSnapHeight = 0.3f;

        [SerializeField, Tooltip("Should the character stick to the ground when walking down steep slopes or over the top of ramps.")]
        private bool m_StickToGround = true;

        [SerializeField, Tooltip("The distance to check ahead of a contact (based on contact normal) to see if it was a slope or a step. Set this higher if you are using physics with bevelled corners instead of primitives (naughty).")]
        private float m_GroundHitLookahead = 0.005f;

        [SerializeField, Tooltip("Do not apply forces to non-kinematic rigidbodies if false.")]
        private bool m_PushRigidbodies = true;

        [SerializeField, Tooltip("Any rigidbodies this mass or below will be pushed with the full push multiplier. Above this and it drops off to zero at max mass.")]
        private float m_LowRigidbodyPushMass = 10f;

        [SerializeField, Tooltip("Any rigidbodies above this mass will have zero force applied to them.")]
        private float m_MaxRigidbodyPushMass = 200f;

        [SerializeField, Tooltip("A multiplier for the push force at or below the minimum push mass. At normal gravity with no physics materials applied, a 1m box will be on the threshold of moving when this is set to 10. Higher will push the box up to the character's velocity with greater acceleration.")]
        private float m_RigidbodyPush = 20f;

        [SerializeField, Tooltip("Can this character be pushed by other INeoCharacterControllers.")]
        private bool m_PushedByCharacters = true;

        [SerializeField, Tooltip("Can this character push other INeoCharacterControllers.")]
        private bool m_PushCharacters = true;

        [SerializeField, Tooltip("A multiplier for the push force when pushing characters at or below this characters mass. Drops to 0 when approaching mass push mass.")]
        private float m_CharacterPush = 2.5f;

        [SerializeField, Tooltip("Does the character inherit yaw changes from moving platforms.")]
        private bool m_InheritPlatformYaw = true;

        [SerializeField, Tooltip("What component of the platform velocity should be included in the character velocity.")]
        private NeoCharacterVelocityInheritance m_InheritPlatformVelocity = NeoCharacterVelocityInheritance.None;

        [SerializeField, Tooltip("The gravity vector (direction and acceleration) for the character.")]
        private Vector3 m_Gravity = new Vector3(0f, -9.8f, 0f);

        [SerializeField, Tooltip("If this is true, then adjusting the gravity direction will reorient the character so that down is in the direction of gravity, and up is opposed.")]
        private bool m_OrientUpWithGravity = true;

        [SerializeField, Range(0f, 3f), Tooltip("The duration (in seconds) it takes to rotate the character up vector a whole 180 degrees.")]
        private float m_UpSmoothing = 2f;

        //DS
        [SerializeField, Tooltip("The duration (in seconds) it takes to rotate the character up vector a whole 180 degrees.")]
        private bool m_MatchDirOnGravChange = true;

        private InputGravity inputGravity;
        //DS

        const float k_ClampMaxSlopeLow = 30f;
        const float k_ClampSkinWidthLow = 0.001f;
        const float k_ClampSkinWidthHigh = 0.1f;
        const float k_ClampWallAngleLow = 0.5f;
        const float k_ClampWallAngleHigh = 5f;
        const float k_ClampStepMaxAngleLow = 5f;
        const float k_ClampMaxGroundingDistanceHigh = 0.5f;
        const float k_ClampMinRbPushMassHigh = 50f;
        const float k_ClampMaxRbPushMassHigh = 1000f;
        const float k_ClampMinRbPushHigh = 50f;
        const float k_ClampMaxCharPushLow = 0.1f;
        const float k_ClampMaxCharPushHigh = 20f;
        const float k_MinRigidbodyKnockMass = 5f;
        const float k_GroundingCheckDistance = 0.05f;
        const float k_MaxDepenetrationDistance = 0.1f;
        const int k_MoveIterationLimit = 10;
        const int k_MoveBufferLength = 8;
        const int k_MaxOverlapColliders = 10;
        const int k_MaxRigidbodyHits = 8;
        const int k_MaxTinyMoves = 3;
        const int k_LowDepenetrationLimit = 4;
        const int k_HighDepenetrationLimit = 6;
        const int k_HeightCheckFrequency = 10;
        const float k_MinCapsuleRadius = 0.1f;
        const float k_MinMoveDistance = 0.0001f;
        const float k_TinyValue = 0.00001f;
        const float k_Cos5 = 0.99619469809174553229501040247389f;

#if UNITY_EDITOR
        void OnValidate()
        {
            m_SkinWidth = Mathf.Clamp(m_SkinWidth, k_ClampSkinWidthLow, k_ClampSkinWidthHigh);
            m_WallAngle = Mathf.Clamp(m_WallAngle, k_ClampWallAngleLow, k_ClampWallAngleHigh);
            m_GroundSnapHeight = Mathf.Clamp(m_GroundSnapHeight, 0f, k_ClampMaxGroundingDistanceHigh);
            m_LowRigidbodyPushMass = Mathf.Clamp(m_LowRigidbodyPushMass, 0f, k_ClampMinRbPushMassHigh);
            m_MaxRigidbodyPushMass = Mathf.Clamp(m_MaxRigidbodyPushMass, m_LowRigidbodyPushMass, k_ClampMaxRbPushMassHigh);
            m_RigidbodyPush = Mathf.Clamp(m_RigidbodyPush, 1f, k_ClampMinRbPushHigh);
            m_CharacterPush = Mathf.Clamp(m_CharacterPush, k_ClampMaxCharPushLow, k_ClampMaxCharPushHigh);

            // Filter depenetration mask
            m_DepenetrationMask &= PhysicsFilter.GetMatrixMaskFromLayerIndex(gameObject.layer);
            
            // Clamp properties that rely on others
            float clampMaxSlopeHigh = 90f - m_WallAngle;
            m_SlopeLimit = Mathf.Clamp(m_SlopeLimit, k_ClampMaxSlopeLow, clampMaxSlopeHigh);
            float r = GetComponent<CapsuleCollider>().radius;
            float clampStepOffsetHigh = r - (r - m_SkinWidth) * Mathf.Cos(Mathf.Deg2Rad * clampMaxSlopeHigh);
            m_StepHeight = Mathf.Clamp(m_StepHeight, 0f, clampStepOffsetHigh);
            m_StepMaxAngle = Mathf.Clamp(m_StepMaxAngle, k_ClampStepMaxAngleLow, m_SlopeLimit);
        }
#endif

        private static List<INeoCharacterControllerHitHandler> s_TargetHitHandlers = new List<INeoCharacterControllerHitHandler>(4);

        private bool m_Initialised = false;
        private Transform m_LocalTransform = null;
        private INeoCharacterControllerHitHandler[] m_EventHandlers = null;
        private IAimController m_Aimer = null;
        private bool m_CollisionsEnabled = true;
        private float m_StartingHeight = 0f;
        private float m_TargetHeight = -1f;
        private float m_TargetHeightFromNormalised = 0f;
        private int m_TargetHeightCounter = -1;
        private float m_StartingRadius = 0f;
        private float m_TargetRadius = 0f;
        private float m_UpLerp = 0f;
        private float m_UpIncrement = 0f;
        private float m_GroundHitCutoff = 0f;
        private float m_MaxSlopeCutoff = 0f;
        private float m_SinWallAngle = 0f;
        private float m_MoveOvershoot = 0f;
        private bool m_BlockGroundSnapping = true;
        private bool m_PlatformWasNull = true;
        private Rigidbody m_Rigidbody = null;
        private Vector3 m_ExternalForceMove = Vector3.zero;
        private Vector3 m_StartPosition = Vector3.zero;
        private Vector3 m_TargetPosition = Vector3.zero;
        private Quaternion m_StartRotation = Quaternion.identity;
        private Quaternion m_TargetRotation = Quaternion.identity;
        private Quaternion m_InverseRotation = Quaternion.identity;
        private Vector3 m_TargetUp = Vector3.zero;
        private Vector3 m_CurrentUp = Vector3.zero;
        private CapsuleCollider m_Capsule = null;
        private RaycastHit m_Hit = new RaycastHit();
        private Vector3 m_PositionCorrection = Vector3.zero;
        private Vector3 m_PlatformVelocity = Vector3.zero;
        private Collider[] m_OverlapColliders = new Collider[k_MaxOverlapColliders];
        private Rigidbody[] m_HitRigidbodies = new Rigidbody[k_MaxRigidbodyHits];
        private int m_NumRigidbodyHits = 0;
        private MoveSegment[] m_MoveSegments = new MoveSegment[k_MoveBufferLength];
        private int m_MoveIndex = 0;
        private int m_MoveCount = 0;
        private int m_MoveIterations = 0;
        private int m_Depenetrations = 0;

        private NeoCharacterControllerDelegates.GetMoveVector m_MoveCallback;
        private NeoCharacterControllerDelegates.OnMoved m_OnMoved;

        /// <summary>
        /// An event that is fired when the character changes height.
        /// If the character height is restricted, then a height change will be delayed until the character has space to resize.
        /// </summary>
        public event NeoCharacterControllerDelegates.OnHeightChange onHeightChanged;

        /// <summary>
        /// An event fired when the character is teleported to a new position.
        /// </summary>
        public event NeoCharacterControllerDelegates.OnTeleported onTeleported;

        /// <summary>
        /// A callback that is used by the character to resolve collisions with other INeoCharacterController characters.
        /// The character should implement and expose a default handler.
        /// </summary>
        public NeoCharacterControllerDelegates.OnHitCharacter characterCollisionHandler { get; set; }

        /// <summary>
        /// A callback that is used by the character to resolve collisions with dynamic rigidbodies.
        /// The character should implement and expose a default handler.
        /// </summary>
        public NeoCharacterControllerDelegates.OnHitRigidbody rigidbodyCollisionHandler { get; set; }

        /// <summary>
        /// An event fired when the character collides with an obstacle while calculating its move.
        /// Not all collisions fire an event. For example, colliding with a step and stepping up is handled silently.
        /// </summary>
        public event NeoCharacterControllerDelegates.OnCharacterControllerHit onControllerHit;

        /// <summary>
        /// The forward axis of the controller in world space.
        /// </summary>
        public Vector3 forward { get { return m_LocalTransform.forward; } }

        /// <summary>
        /// The right axis of the controller in world space.
        /// </summary>
        public Vector3 right { get { return m_LocalTransform.right; } }

        /// <summary>
        /// Used to prevent the up vector changing in certain conditions, such as climbing ladders. Defaults to false.
        /// </summary>
        public bool lockUpVector { get; set; }

        /// <summary>
        /// The velocity of the character during the last move phase with some corrections such as ignoring the vertical movement of steps.
        /// </summary>
        public Vector3 velocity { get; private set; }

        /// <summary>
        /// The actual velocity of the character during the last move phase with no modifications applied.
        /// </summary>
        public Vector3 rawVelocity { get; private set; }

        /// <summary>
        /// The velocity of the character input before collisions.
        /// </summary>
        public Vector3 targetVelocity { get; private set; }

        /// <summary>
        /// The collision layers the controller will collide with when moving.
        /// </summary>
        public LayerMask collisionMask { get; set; }

        /// <summary>
        /// The collision layers the controller will check against for depenetration.
        /// </summary>
        public LayerMask depenetrationMask
        {
            get { return m_DepenetrationMask & collisionMask; }
            set { m_DepenetrationMask = value & collisionMask; }
        }

        /// <summary>
        /// The move vector from the last fixed update move.
        /// </summary>
        public Vector3 lastFrameMove
        {
            get;
            private set;
        }

        /// <summary>
        /// Is the character in contact with a ground surface.
        /// </summary>
        public bool isGrounded { get; private set; }

        /// <summary>
        /// The amount of time the controller has been airborne without a ground contact.
        /// </summary>
        public float airTime { get; private set; }

        /// <summary>
        /// The normal of the last ground contact.
        /// </summary>
        public Vector3 groundNormal { get; private set; }

        /// <summary>
        /// The normal of the ground surface for the last contact. If the controller is on a ledge, this is the top surface of the ledge.
        /// </summary>
        public Vector3 groundSurfaceNormal { get; private set; }

        /// <summary>
        /// The height below which the character controller should snap to the ground.
        /// </summary>
        public float groundSnapHeight
        {
            get { return m_GroundSnapHeight; }
            set { m_GroundSnapHeight = Mathf.Clamp(value, 0f, 20f); }
        }

        /// <summary>
        /// The collision flags for detecting scene collisions while moving.
        /// </summary>
        public NeoCharacterCollisionFlags collisionFlags { get; private set; }
        
        /// <summary>
        /// Should the character move with platforms it's in contact with. Defaults to false.
        /// </summary>
        public bool ignorePlatforms
        {
            get;
            set;
        }

        /// <summary>
        /// The platform the character is currently affected by.
        /// </summary>
        public IMovingPlatform platform
        {
            get;
            private set;
        }

        // The component of a move to be discounted from final velocity calculations
        enum VelocityCorrection : byte
        {
            None,
            Full,
            Vertical,
            Horizontal
        }
        
        // MoveSegments are consumed by the move loop. They are stored in a circular buffer
        // that has new segments added based on input, collision response and features such
        // as steps or snapping.
        struct MoveSegment
        {
            // The direction and distance of the move
            public Vector3 moveDirection;
            public float moveDistance;

            // Should the character slide on collision during this move
            public bool slide;

            // Should this move or a component of it be discounted from the final velocity calculations
            public VelocityCorrection correction;
            
            public MoveSegment(Vector3 mdir, float mdist, bool s, VelocityCorrection vc)
            {
                moveDirection = mdir;
                moveDistance = mdist;
                slide = s;
                correction = vc;
            }
        }

        /// <summary>
        /// The up axis for the controller in world space.
        /// </summary>
        public Vector3 up
        {
            get { return m_TargetUp; }
            set
            {
                if (!orientUpWithGravity || m_Gravity.sqrMagnitude <= Mathf.Epsilon)
                    SetUpVector(value);
#if UNITY_EDITOR
                else
                    Debug.LogError("Cannot set up vector on NeoCharacterController while orientUpWithGravity is true.", this);
#endif
            }
        }

        /// <summary>
        /// The gravity vector applied to this character.
        /// </summary>
        public Vector3 gravity
        {
            get { return m_Gravity; }
            set
            {
                m_Gravity = value;
                if (orientUpWithGravity && m_Gravity.sqrMagnitude > Mathf.Epsilon)
                    SetUpVector(-m_Gravity.normalized);
            }
        }

        public bool matchDirOnGravChange
        {
            get { return m_MatchDirOnGravChange; }
            set { m_MatchDirOnGravChange = value; }
        }

        /// <summary>
        /// The variable gravity setup for this character if applicable.
        /// </summary>
        public INeoCharacterVariableGravity characterGravity
        {
            get { return this; }
        }

        /// <summary>
        /// The amount of smoothing over time to apply to changes in the controller up vector. 0 is instantaneous, 1 is 1 second for a full 180 degree rotation.
        /// </summary>
        public float upSmoothing
        {
            get { return m_UpSmoothing; }
            set { m_UpSmoothing = Mathf.Clamp01(value); }
        }

        /// <summary>
        /// Should the controller automatically change the up vector to match gravity (treating gravity as a down vector).
        /// </summary>
        public bool orientUpWithGravity
        {
            get { return m_OrientUpWithGravity; }
            set
            {
                if (!m_OrientUpWithGravity && value && m_Gravity.sqrMagnitude > Mathf.Epsilon)
                    SetUpVector(-m_Gravity.normalized);
                m_OrientUpWithGravity = value;
            }
        }

        /// <summary>
        /// The friction of ground contacts when the controller is overhanging a ledge. At 1, the character will not slide off the ledge.
        /// </summary>
        public float slopeFriction
        {
            get { return m_SlopeFriction; }
            set { m_SlopeFriction = Mathf.Clamp01(value); }
        }

        /// <summary>
        /// The friction of ground contacts when the controller is overhanging a ledge. At 1, the character will not slide off the ledge.
        /// </summary>
        public float ledgeFriction
        {
            get { return m_LedgeFriction; }
            set { m_LedgeFriction = Mathf.Clamp01(value); }
        }
        
        /// <summary>
        /// Should the character inherit yaw rotation from moving platforms? Defaults to true.
        /// </summary>
        public bool inheritPlatformYaw
        {
            get { return m_InheritPlatformYaw; }
            set { m_InheritPlatformYaw = value; }
        }

        /// <summary>
        /// What component of the platform velocity should be included in the character velocity after a move tick.
        /// For characters that track momentum this should be set to *None* as it will lead to a feedback loop and exponential velocity increase while on platforms.
        /// </summary>
        public NeoCharacterVelocityInheritance inheritPlatformVelocity
        {
            get { return m_InheritPlatformVelocity; }
            set { m_InheritPlatformVelocity = value; }
        }

        /// <summary>
        /// Should the character collide with scene colliders (static or otherwise). Defaults to true.
        /// </summary>
        public bool collisionsEnabled
        {
            get { return m_CollisionsEnabled; }
            set
            {
                bool was = m_CollisionsEnabled;
                m_CollisionsEnabled = value;
                if (!was && m_CollisionsEnabled)
                    Depenetrate();
                
            }
        }

        /// <summary>
        /// The height of the character capsule.
        /// </summary>
        public float height
        {
            get { return m_Capsule.height; }
            set { SetHeight(value, 0f); }
        }

        /// <summary>
        /// The radius of the character capsule.
        /// </summary>
        public float radius
        {
            get { return m_Capsule.radius; }
            set { SetRadius(value); }
        }

        /// <summary>
        /// When performing the move loop, the capsule is shrunk by this amount. When testing for contacts it is grown by this amount.
        /// </summary>
        public float skinWidth
        {
            get { return m_SkinWidth; }
            set { m_SkinWidth = value; }
        }

        /// <summary>
        /// The character will step up onto ledges with this height without losing horizontal speed.
        /// The upward movement will not be factored into the character velocity at the end of the move.
        /// </summary>
        public float stepHeight
        {
            get { return m_StepHeight; }
            set
            {
                float clampStepOffsetHigh = radius - (radius - m_SkinWidth) * Mathf.Cos(Mathf.Deg2Rad * (90f - m_WallAngle));
                m_StepHeight = Mathf.Clamp(value, 0f, clampStepOffsetHigh);
            }
        }

        /// <summary>
        /// The maximum upwards slope the character can climb. Any horizontal movement into the slope will not become vertical movement above this angle.
        /// </summary>
        public float slopeLimit
        {
            get { return m_SlopeLimit; }
            set
            {
                m_SlopeLimit = Mathf.Clamp(value, k_ClampMaxSlopeLow, 90f - m_WallAngle);
                m_MaxSlopeCutoff = radius - (radius - m_SkinWidth) * Mathf.Cos(Mathf.Deg2Rad * m_SlopeLimit);
            }
        }

        /// <summary>
        /// The mass of the character.
        /// </summary>
        public float mass
        {
            get { return m_Rigidbody.mass; }
            set { m_Rigidbody.mass = value; }
        }

        /// <summary>
        /// Should the character push dynamic rigidbodies. Defaults to true.
        /// </summary>
        public bool pushRigidbodies
        {
            get { return m_PushRigidbodies; }
            set { m_PushRigidbodies = value; }
        }

        /// <summary>
        /// Should the character push other character controllers. Defaults to true.
        /// </summary>
        public bool pushCharacters
        {
            get { return m_PushCharacters; }
            set { m_PushCharacters = value; }
        }

        /// <summary>
        /// Can the character be pushed by other character controllers. Defaults to true.
        /// </summary>
        public bool pushedByCharacters
        {
            get { return m_PushedByCharacters; }
            set { m_PushedByCharacters = value; }
        }

        /// <summary>
        /// If this is true, any forces applied with AddForce() will be ignored. Defaults to false.
        /// </summary>
        public bool ignoreExternalForces
        {
            get;
            set;
        }

        void Awake()
        {
            Initialise();
        }

        void Start()
        {
            // Initialise positions, etc (should reinit up?)
            m_InverseRotation = Quaternion.Inverse(m_StartRotation);
            m_StartPosition = m_TargetPosition = m_LocalTransform.position;
            m_StartRotation = m_TargetRotation = m_LocalTransform.rotation;

            //DS
            inputGravity = GetComponent<InputGravity>();
            //DS

            // Initialise the capsule
            OnRadiusChanged();
            if (m_TargetHeight == -1f)
                SetCapsuleHeightInternal(m_StartingHeight, 0f);
        }

        public void Initialise ()
        {
            if (m_Initialised)
                return;

            // Get components
            m_LocalTransform = transform;
            m_Capsule = GetComponent<CapsuleCollider>();
            m_Aimer = GetComponentInChildren<IAimController>();
            m_EventHandlers = GetComponentsInChildren<INeoCharacterControllerHitHandler>(true);

            // Get layer mask
            collisionMask = PhysicsFilter.GetMatrixMaskFromLayerIndex(gameObject.layer);
            depenetrationMask &= collisionMask;

            // Set up rigidbody
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.useGravity = false;
            m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            // Set defaults
            ignorePlatforms = false;
            ignoreExternalForces = false;

            // Initialise up in case it's used by components on the same object
            m_CurrentUp = m_TargetUp = m_LocalTransform.up;
            m_UpLerp = 1f;

            // Initialise the capsule
            m_StartingRadius = m_Capsule.radius;
            m_StartingHeight = m_Capsule.height;

            m_Initialised = true;
        }

        /// <summary>
        /// Attach input callbacks to the character controller. The controller movement needs to occur in a specific sequence, and so will
        /// request input when required.
        /// </summary>
        /// <param name="moveCallback">A callback for the character move, called each fixed update.</param>
        public void SetMoveCallback(NeoCharacterControllerDelegates.GetMoveVector moveCallback, NeoCharacterControllerDelegates.OnMoved onMovedCallback)
        {
            m_MoveCallback = moveCallback;
            m_OnMoved = onMovedCallback;
        }

        Vector3 GetCapsuleBottom ()
        {
            return m_TargetPosition + m_CurrentUp * radius;
        }

        Vector3 GetCapsuleCenter()
        {
            return m_TargetPosition + m_CurrentUp * (m_Capsule.height * 0.5f);
        }

        Vector3 GetCapsuleTop ()
        {
            return m_TargetPosition + m_CurrentUp * (m_Capsule.height - radius);
        }

        #region RESIZE

        void OnRadiusChanged ()
        {
            float fullRadius = radius;
            float smallRadius = fullRadius - m_SkinWidth;
            m_SinWallAngle = Mathf.Sin(Mathf.Deg2Rad * m_WallAngle);
            m_GroundHitCutoff = fullRadius - smallRadius * m_SinWallAngle;
            m_MaxSlopeCutoff = fullRadius - smallRadius * Mathf.Cos(Mathf.Deg2Rad * m_SlopeLimit);
            m_GroundHitCutoff -= k_TinyValue;
            m_MaxSlopeCutoff -= k_TinyValue;
            m_MoveOvershoot = 0.5f * Mathf.Sqrt(8 * m_SkinWidth * (fullRadius - 0.5f * m_SkinWidth));
        }

        void OnHeightChanged(float newHeight, float rootOffset)
        {
            if (onHeightChanged != null)
                onHeightChanged(newHeight, rootOffset);
        }

        float SetCapsuleHeightInternal(float h, float offset)
        {
            // Record the old height
            float oldHeight = m_Capsule.height;

            // Set the new height & center
            float newHeight = Mathf.Clamp(h, 2 * radius, float.MaxValue);
            m_Capsule.height = newHeight;
            m_Capsule.center = new Vector3(0f, newHeight * 0.5f);

            // Fire height change event
            OnHeightChanged (newHeight, offset);

            // Reset target height
            m_TargetHeight = 0f;

            // Update the position (no need for corrections as both start and end change)
            Vector3 move = m_CurrentUp * offset;
            m_StartPosition += move;
            m_TargetPosition += move;
            m_Rigidbody.position += move;

            // Return the offset
            return newHeight - oldHeight;
        }

        /// <summary>
        /// Check if the character has space to change its height to the specified value without overlapping the environment.
        /// </summary>
        /// <param name="h">The target height. The height can not be smaller than double the radius.</param>
        /// <returns>Can the character expand or not.</returns>
        public bool IsHeightRestricted(float height)
        {
            float offset;
            return IsHeightRestricted(height, 0f, out offset);
        }

        bool IsHeightRestricted(float h, float fromNormalisedHeight, out float offset)
        {
            // Can always shrink
            if (h < m_Capsule.height)
            {
                offset = Mathf.Lerp(0f, m_Capsule.height - h, fromNormalisedHeight);
                return false;
            }
            
            // Clamp height
            float r = radius;
            h = Mathf.Clamp(h, r * 2f, float.MaxValue);

            // Initialise loop variables
            int hitCount = 0;
            bool castDown = fromNormalisedHeight > 0f;
            float totalDistance = h - m_Capsule.height;
            float distance = (castDown) ? totalDistance * fromNormalisedHeight : totalDistance;

            // Clear root offset
            offset = 0f;

            // Check above / below (3 in case first is a short check and doesn't hit, but opposite does)
            for (int i = 0; i < 3; ++i)
            {
                // Get the ray cast
                Ray ray = (castDown) ? new Ray(GetCapsuleBottom(), -m_CurrentUp) : new Ray(GetCapsuleTop(), m_CurrentUp);
                if (PhysicsExtensions.SphereCastNonAllocSingle(
                    ray,
                    r - m_SkinWidth,
                    out m_Hit,
                    distance + m_SkinWidth,
                    collisionMask,
                    m_LocalTransform,
                    QueryTriggerInteraction.Ignore
                ))
                {
                    // Check if hit above and below
                    if (++hitCount == 2)
                    {
                        offset = 0f;
                        return true;
                    }

                    // Add root offset if required
                    if (castDown)
                        offset -= m_Hit.distance - m_SkinWidth;

                    // Remove hit distance from total
                    totalDistance -= m_Hit.distance - m_SkinWidth;
                }
                else
                {
                    // Add root offset if required
                    if (castDown)
                        offset -= distance;

                    // Remove checked distance from total
                    totalDistance -= distance;
                }

                // Get the remaining distance
                distance = totalDistance;
                if (distance <= Mathf.Epsilon)
                    return false;

                // Flip directions
                castDown = !castDown;
            }

            offset = 0f;
            return true;
        }

        /// <summary>
        /// Try to set the character capsule height to a specific value. The height can not be smaller than double the radius.
        /// If the character height is restricted by the environment, then the character height will not be changed.
        /// </summary>
        /// <param name="h">The target height.</param>
        /// <param name="fromNormalisedHeight">The point to resize from. 0 is the bottom of the current capsule, and 1 is the top.</param>
        /// <returns>Did the character height change (true), or was it restricted (false).</returns>
        public bool TrySetHeight(float h, float fromNormalisedHeight = 0f)
        {
            h = Mathf.Clamp(h, radius * 2f, float.MaxValue);
            float offset;

            // Check height restriction
            if (IsHeightRestricted(h, Mathf.Clamp01(fromNormalisedHeight), out offset))
                return false;

            // Set the height
            SetCapsuleHeightInternal(h, offset);

            m_TargetHeightCounter = -1;
            return true;
        }

        /// <summary>
        /// Set the height of the character to a specific value. The height can not be smaller than double the radius.
        /// If the resulting capsule would overlap the environment then the controller will keep trying until there is space for the capsule to be resized.
        /// </summary>
        /// <param name="h">The target height.</param>
        /// <param name="fromNormalisedHeight">The point to resize from. 0 is the bottom of the current capsule, and 1 is the top.</param>
        public void SetHeight(float h, float fromNormalisedHeight = 0f)
        {
            m_TargetHeightFromNormalised = Mathf.Clamp01(fromNormalisedHeight);
            m_TargetHeight = Mathf.Clamp(h, radius * 2f, float.MaxValue);
            m_TargetHeightCounter = 0;
        }

        /// <summary>
        /// Try to reset the character capsule height to its value upon start.
        /// If the character height is restricted by the environment, then the character height will not be changed.
        /// </summary>
        /// <param name="fromNormalisedHeight">The point to resize from. 0 is the bottom of the current capsule, and 1 is the top.</param>
        /// <returns>Did the character height change (true), or was it restricted (false).</returns>
        public bool TryResetHeight(float fromNormalisedHeight = 0f)
        {
            if (Mathf.Approximately(height, m_StartingHeight))
            {
                m_TargetHeightCounter = -1;
                return true;
            }
            else
                return TrySetHeight(m_StartingHeight, fromNormalisedHeight);
        }

        /// <summary>
        /// Reset the character capsule height to its value upon start.
        /// If the character height is restricted, it will keep trying until it has space to resize, or the height change is cancelled.
        /// </summary>
        /// <param name="fromNormalisedHeight">The point to resize from. 0 is the bottom of the current capsule, and 1 is the top.</param>
        public void ResetHeight(float fromNormalisedHeight = 0f)
        {
            if (!Mathf.Approximately(height, m_StartingHeight))
                SetHeight(m_StartingHeight, fromNormalisedHeight);
        }

        /// <summary>
        /// Cancel any delayed height change and keep the current height.
        /// </summary>
        public void CancelHeightChange()
        {
            m_TargetHeight = 0f;
            m_TargetHeightFromNormalised = 0f;
        }

        /// <summary>
        /// Try to reset the character capsule radius to its value upon start.
        /// If the resulting capsule would overlap the environment then the character capsule will not be changed.
        /// </summary>
        /// <param name="r">The target radius.</param>
        /// <returns>Did the character radius change (true), or was it restricted (false).</returns>
        public bool TrySetRadius (float r)
        {
            // Clamp target radius
            r = Mathf.Clamp(r, k_MinCapsuleRadius, float.MaxValue * 0.5f);

            // If new minimum height is larger than current height then resize
            if (height < (r * 2f) && !TrySetHeight(r * 2f, 0f))
                return false;

            // Set the new radius and depenetrate
            m_Capsule.radius = r;
            OnRadiusChanged();
            Depenetrate();

            // Clear any delayed radius change
            if (m_TargetRadius != 0f)
            {
                onHeightChanged -= ChangeRadiusOnHeightChange;
                m_TargetRadius = 0f;
            }

            return true;
        }

        /// <summary>
        /// Set the radius of the character to a specific value.
        /// If the resulting capsule would overlap the environment then the controller will keep trying until there is space for the capsule to be resized.
        /// </summary>
        /// <param name="r">The target radius.</param>
        public void SetRadius (float r)
        {
            // Clamp target radius
            r = Mathf.Clamp(r, k_MinCapsuleRadius, float.MaxValue * 0.5f);

            // If new minimum height is larger than current height then resize
            if (height < (r * 2f))
            {
                // Setting radius is delayed until resize is complete
                if (m_TargetRadius == 0f)
                    onHeightChanged += ChangeRadiusOnHeightChange;
                m_TargetRadius = r;
            }
            else
            {
                // Set the new radius and depenetrate
                m_Capsule.radius = r;
                OnRadiusChanged();
                Depenetrate();

                // Clear any delayed radius change
                if (m_TargetRadius != 0f)
                {
                    onHeightChanged -= ChangeRadiusOnHeightChange;
                    m_TargetRadius = 0f;
                }
            }
        }

        void ChangeRadiusOnHeightChange(float newHeight, float rootOffset)
        {
            // Set the new radius and depenetrate
            m_Capsule.radius = m_TargetRadius;
            OnRadiusChanged();
            Depenetrate();

            // Remove event handler
            onHeightChanged -= ChangeRadiusOnHeightChange;
            m_TargetRadius = 0f;
        }

        /// <summary>
        /// Try to reset the character capsule radius to its value upon start.
        /// If the resulting capsule would overlap the environment then the character capsule will not be changed.
        /// </summary>
        /// <returns>Did the character radius change (true), or was it restricted (false).</returns>
        public bool TryResetRadius ()
        {
            if (Mathf.Approximately(radius, m_StartingRadius))
                return true;
            else
                return TrySetRadius(m_StartingRadius);
        }

        /// <summary>
        /// Reset the character capsule radius to its value upon start.
        /// If the resulting capsule would overlap the environment then the controller will keep trying until there is space for the capsule to be resized.
        /// </summary>
        public void ResetRadius()
        {
            if (!Mathf.Approximately(radius, m_StartingRadius))
                SetRadius(m_StartingRadius);
        }

        #endregion

        /// <summary>
        /// Set all velocity variables to a specific vector
        /// </summary>
        public void SetVelocity(Vector3 v)
        {
            velocity = v;
            rawVelocity = v;
            targetVelocity = v;
        }

        /// <summary>
        /// Reset all velocity variables to zero
        /// </summary>
        public void ResetVelocity()
        {
            velocity = Vector3.zero;
            rawVelocity = Vector3.zero;
            targetVelocity = Vector3.zero;
        }

        /// <summary>
        /// Reset the vertical component of all velocity variables to zero
        /// </summary>
        public void ResetVerticalVelocity()
        {
            velocity = Vector3.ProjectOnPlane(velocity, up);
            rawVelocity = Vector3.ProjectOnPlane(rawVelocity, up);
            targetVelocity = Vector3.ProjectOnPlane(targetVelocity, up);
        }

        /// <summary>
        /// Reset the horizontal component of all velocity variables to zero
        /// </summary>
        public void ResetHorizontalVelocity()
        {
            velocity = Vector3.Project(velocity, up);
            rawVelocity = Vector3.Project(rawVelocity, up);
            targetVelocity = Vector3.Project(targetVelocity, up);
        }

        void SetUpVector(Vector3 to)
        {
            if (lockUpVector || upSmoothing > 0f)
            {
                m_TargetUp = to;
                m_UpLerp = 0f;
                float angle = Vector3.Angle(m_CurrentUp, m_TargetUp);
                float inverseDuration = 180f / (angle * upSmoothing);
                m_UpIncrement = Mathf.Clamp01(Time.fixedDeltaTime * inverseDuration);
            }
            else
            {
                m_TargetUp = to;
                m_UpLerp = 0f;
                m_UpIncrement = 1f;
            }
        }

        void FixedUpdate ()
        {
            ClearMoveBuffer();

            // Reset the position / rotation
            //m_Rigidbody.position = m_TargetPosition;
            if (m_Aimer != null)
            {
                //DS
                //Debug.Log(matchDirOnGravChange);
                if (matchDirOnGravChange)
                {
                    //Debug.Log(inputGravity.gravityChanged);
                    //m_TargetRotation *= m_Aimer.yawLocalRotation;
                    if (inputGravity.gravityChanged)
                    {
                        Debug.Log("CHANGE");
                        m_TargetRotation *= m_Aimer.yawLocalRotation;// * new Quaternion(0f, 180f, 0f, 1f).normalized;
                        m_Aimer.ResetYawLocal();
                    }
                    else
                    {
                        Debug.Log("NO CHANGE");
                        m_TargetRotation *= m_Aimer.yawLocalRotation;
                        m_Aimer.ResetYawLocal();
                    }
                    inputGravity.gravityChanged = false;
                }
                else
                {
                    m_TargetRotation *= m_Aimer.yawLocalRotation;
                    m_Aimer.ResetYawLocal();
                }
                    
                //DS
                //m_TargetRotation *= m_Aimer.yawLocalRotation;
                //m_Aimer.ResetYawLocal();
            }
            m_Rigidbody.rotation = m_TargetRotation;

            // Record starting details
            lastFrameMove = m_TargetPosition - m_StartPosition;
            m_StartPosition = m_TargetPosition;
            m_StartRotation = m_TargetRotation;

            // Depenetrate in case any objects have overlapped, etc
            Depenetrate();

            // Add platform rotation
            Quaternion platformRotation = Quaternion.identity;
            if (!ignorePlatforms && platform != null && inheritPlatformYaw)
            {
                platformRotation = platform.fixedRotation * Quaternion.Inverse(platform.previousRotation);
                Quaternion rotationDiff = GetYawFromRotation(platformRotation);
                m_TargetRotation *= rotationDiff;
            }

            // Interpolate up direction
            if (!lockUpVector && m_UpLerp < 1f)
            {
                // Get capsule center (world orientation offset from position)
                Vector3 center = m_TargetRotation * m_Capsule.center;

                // Rotate towards up vector
                Quaternion fromTo = Quaternion.FromToRotation(m_TargetRotation * Vector3.up, m_TargetUp);
                m_UpLerp += m_UpIncrement;
                if (m_UpLerp < 1f)
                    fromTo = Quaternion.Lerp(Quaternion.identity, fromTo, m_UpLerp);
                m_TargetRotation = fromTo * m_TargetRotation;

                // Add move offset to rotate from capsule center
                Vector3 offset = center - (fromTo * center);
                m_TargetPosition += offset;
                m_PositionCorrection += offset;

                // Get the current up vector
                m_CurrentUp = m_TargetRotation * Vector3.up;
            }

            // Get the inverse of the character rotation
            m_InverseRotation = Quaternion.Inverse(m_TargetRotation);

            // Resize capsule
            if (m_TargetHeightCounter >= 0)
            {
                if (m_TargetHeightCounter == 0)
                {
                    float offset;
                    if (!IsHeightRestricted(m_TargetHeight, m_TargetHeightFromNormalised, out offset))
                    {
                        SetCapsuleHeightInternal(m_TargetHeight, offset);
                        m_TargetHeightCounter = -1;
                    }
                    else
                        m_TargetHeightCounter = k_HeightCheckFrequency;
                }
                else
                    --m_TargetHeightCounter;
            }

            // Add platform translation
            if (!ignorePlatforms && platform != null)
            {
                Vector3 capsuleBottom = GetCapsuleBottom();
                Vector3 relativePos = Quaternion.Inverse(platform.fixedRotation) * (capsuleBottom - platform.fixedPosition);
                Vector3 prevBottom = platform.previousPosition + (platform.previousRotation * relativePos);
                Vector3 platformMove = platformRotation * (capsuleBottom - prevBottom);

                // Record platform velocity for frame after leaving
                m_PlatformVelocity = platformMove / Time.deltaTime;

                switch (inheritPlatformVelocity)
                {
                    case NeoCharacterVelocityInheritance.None:
                        AddMoveLast(platformMove, true, VelocityCorrection.Full);
                        break;
                    case NeoCharacterVelocityInheritance.Full:
                        AddMoveLast(platformMove, true, VelocityCorrection.None);
                        break;
                    case NeoCharacterVelocityInheritance.HorizontalOnly:
                        AddMoveLast(platformMove, true, VelocityCorrection.Vertical);
                        break;
                    case NeoCharacterVelocityInheritance.VerticalOnly:
                        AddMoveLast(platformMove, true, VelocityCorrection.Horizontal);
                        break;
                }
            }

            // Add force translation
            if (!ignoreExternalForces && m_ExternalForceMove != Vector3.zero)
                AddMoveLast(m_ExternalForceMove * Time.deltaTime, true, VelocityCorrection.None);
#if UNITY_EDITOR
            debugExternalForceMove = m_ExternalForceMove; // Record external forces for debugger (editor only)
#endif
            m_ExternalForceMove = Vector3.zero;

            // Get frame movement vector from input
            bool applyGravity = true;
            bool applyGroundForce = false;
            Vector3 move = Vector3.zero;
            if (m_MoveCallback != null)
                m_MoveCallback(out move, out applyGravity, out applyGroundForce);

            // Record target velocity
            targetVelocity = move / Time.deltaTime;

            // Apply modifiers
            if (applyGravity) // Add as separate move?
                move += m_Gravity * Time.deltaTime * Time.deltaTime;
#if UNITY_EDITOR
            debugSnapToGround = !m_BlockGroundSnapping; // Record ground snapping setting for debugger (editor only)
#endif
            if (m_BlockGroundSnapping)
            {
                applyGroundForce = false;
                m_BlockGroundSnapping = false;
            }
            
            // Add move to queue if large enough
            float moveMagnitude = move.magnitude;
            if (moveMagnitude > k_TinyValue)
                AddMoveLast(move / moveMagnitude, moveMagnitude, true, VelocityCorrection.None);

            // Update position
            MoveLoop();

            // Ground stickiness
            if (m_StickToGround && applyGroundForce)
                ApplyGroundForce();

            // Depenetrate in case any objects have overlapped, etc
            //Depenetrate();

            // Get grounding state
            CheckGrounding();

            Vector3 oldVelocity = velocity;

            // Get velocity with corrections (remove effect of steps and ground snapping) and without
            velocity = (m_TargetPosition - m_PositionCorrection - m_StartPosition) / Time.deltaTime;
            rawVelocity = (m_TargetPosition - m_StartPosition) / Time.deltaTime;

            // Add platform velocity
            if (!ignorePlatforms)
            {
                if (platform == null && !m_PlatformWasNull)
                    velocity += m_PlatformVelocity;
                if (platform != null && m_PlatformWasNull)
                    velocity -= (platform.fixedPosition - platform.previousPosition) / Time.fixedDeltaTime;
            }
            m_PlatformWasNull = (platform == null);

            // Reset frame correction
            m_PositionCorrection = Vector3.zero;

            // Set the position to the target so the character appears in the right place for other fixed updates this frame
            m_Rigidbody.MovePosition(m_TargetPosition);
            m_Rigidbody.MoveRotation(m_TargetRotation);

            // Signal the move is complete
            if (m_OnMoved != null)
                m_OnMoved();
        }
        
        void MoveLoop ()
        {
            // Reset the collision flags
            NeoCharacterCollisionFlags previousCollisionFlags = collisionFlags;
            collisionFlags = NeoCharacterCollisionFlags.None;
            ResetRigidbodyHits();

            float capHeight = height;
            float capRadius = radius;

            // Record tiny moves and abort if over a certain number
            int numTinyMoves = 0;
            
            MoveSegment move;
            m_MoveIterations = 0;
            while (GetMoveVector(out move))
            {
                Vector3 targetUp = m_TargetRotation * Vector3.up;
                Vector3 bottomPosition = m_TargetPosition + targetUp * capRadius;
                Vector3 topPosition = m_TargetPosition + targetUp * (capHeight - capRadius);

                // Capsule cast from start to start + move
                if (collisionsEnabled && PhysicsExtensions.CapsuleCastNonAllocSingle(
                    bottomPosition, topPosition,
                    capRadius - m_SkinWidth,
                    move.moveDirection.normalized,
                    out m_Hit,
                    move.moveDistance + m_MoveOvershoot,
                    collisionMask,
                    m_LocalTransform,
                    QueryTriggerInteraction.Ignore
                    ))
                {
                    bool fireCollisionEvent = true;
                    //Vector3 segmentStartPosition = m_TargetPosition;

                    // Move character and subtract from future moves
                    float cosAngle = Vector3.Dot(m_Hit.normal, -move.moveDirection);
                    float distanceMoved = m_Hit.distance - m_SkinWidth / cosAngle;
                    if (distanceMoved < 0f)
                    {
                        //Debug.Log("Failed move. Hit: " + m_Hit.point.ToString("F5"));
                        distanceMoved = 0f;
                    }

                    // Check if character is stuck and abort remaining moves
                    if (distanceMoved < k_MinMoveDistance)
                    {
                        ++numTinyMoves;
                        if (numTinyMoves == k_MaxTinyMoves)
                        {
                            //Debug.Log("Max tiny moves reached");
                            break;
                        }
                    }
                    else
                        numTinyMoves = 0;

                    if (distanceMoved > move.moveDistance)
                    {
                        // Update position
                        Vector3 delta = move.moveDirection * move.moveDistance;
#if VISUALISE_MOVEMENT
                        Color debugColor = (move.correction == VelocityCorrection.None) ? Color.black : Color.blue;
                        Debug.DrawLine(m_TargetPosition, m_TargetPosition + delta, debugColor, 1f, false);
#endif
                        m_TargetPosition += delta;

                        // Record correction for velocity calculations
                        if (move.correction != VelocityCorrection.None)
                            AddCorrection(delta, move.correction);

                        // Next move segment as the collision was past the real end point
                        ++m_MoveIterations;
                        continue;
                    }
                    else
                    {
                        Vector3 delta = move.moveDirection * distanceMoved;
#if VISUALISE_MOVEMENT
                        Color debugColor = (move.correction == VelocityCorrection.None) ? Color.black : Color.blue;
                        Debug.DrawLine(m_TargetPosition,  m_TargetPosition + delta, debugColor, 1f, false);
#endif
                        m_TargetPosition += delta;

                        // Record correction for velocity calculations
                        if (move.correction != VelocityCorrection.None)
                            AddCorrection(delta, move.correction);
                    }

                    // If it doesn't slide, this move is completed
                    if (!move.slide)
                    {
                        ++m_MoveIterations;
                        continue;
                    }

                    // Get the hit normal in local space
                    Vector3 localHitNormal = m_InverseRotation * m_Hit.normal;

                    // Get the impact type
                    NeoCharacterCollisionFlags collisionType;
                    if (localHitNormal.y > m_SinWallAngle)
                    {
                        // Ground collision
                        collisionType = NeoCharacterCollisionFlags.Below;
                    }
                    else
                    {
                        // Check if head or wall collision
                        if (localHitNormal.y < -m_SinWallAngle)
                        {
                            collisionType = NeoCharacterCollisionFlags.Above;
                        }
                        else
                        {
                            // Check if left / right vs front / back
                            float absX = Mathf.Abs(localHitNormal.x);
                            float absZ = Mathf.Abs(localHitNormal.z);
                            if (absX > absZ)
                            {
                                if (localHitNormal.x < 0f)
                                    collisionType = NeoCharacterCollisionFlags.Right;
                                else
                                    collisionType = NeoCharacterCollisionFlags.Left;
                            }
                            else
                            {
                                if (localHitNormal.z < 0f)
                                    collisionType = NeoCharacterCollisionFlags.Front;
                                else
                                    collisionType = NeoCharacterCollisionFlags.Back;
                            }
                        }
                    }
                    
                    float movedAmount = distanceMoved / move.moveDistance;
                    float newMoveDistance = move.moveDistance * (1f - movedAmount);

                    // Only perform collision response if the new move justifies it
                    if (newMoveDistance > k_TinyValue)
                    {
                        Vector3 newMove = move.moveDirection * newMoveDistance;

                        // Get the horizontal & vertical components of the move vector
                        float verticalAmount = Vector3.Dot(newMove, m_CurrentUp);
                        Vector3 vertical = m_CurrentUp * verticalAmount;
                        Vector3 horizontal = newMove - vertical;
                        float horizontalMagnitude = horizontal.magnitude;

#if VISUALISE_MOVEMENT
                        Debug.DrawRay(m_Hit.point, m_Hit.normal * 0.1f, Color.cyan, 1f);
#endif

                        // Get new move vector based on hit normal
                        switch (collisionType)
                        {
                            case NeoCharacterCollisionFlags.Above:
                                {
#if VISUALISE_MOVEMENT
                                    Debug.DrawRay(m_Hit.point, newMove.normalized * 0.5f, Color.magenta, 1f);
#endif
                                    // Get the horizontal constrained hit normal and check if ground is flat  
                                    bool flat;
                                    Vector3 horizontalHit = GetHorizontalHitNormal(m_Hit.normal, out flat);
                                    Debug.DrawRay(m_Hit.point, horizontalHit * 0.5f, Color.yellow, 1f);

                                    if (flat)
                                    {
                                        newMove = Vector3.ProjectOnPlane(newMove, m_Hit.normal);
                                    }
                                    else
                                    {
                                        if (Vector3.Dot(newMove, horizontalHit) > 0f)
                                        {
                                            newMove = Vector3.ProjectOnPlane(newMove, m_CurrentUp);
                                        }
                                        else
                                        {
                                            float mag = newMove.magnitude;
                                            newMove = Vector3.ProjectOnPlane(newMove, horizontalHit);
                                            newMove += horizontalHit * 0.01f;
                                        }
                                    }
#if VISUALISE_MOVEMENT
                                    Debug.DrawRay(m_Hit.point, newMove.normalized * 0.5f, Color.cyan, 1f);
#endif

                                    AddMoveFirst(newMove, true, VelocityCorrection.None);
                                }
                                break;
                            case NeoCharacterCollisionFlags.Below:
                                {
                                    // Check if ground is "flat" and set surface normal to match
                                    Vector3 surfaceNormal;
                                    bool isEdge = GetSurfaceNormalFromHit(m_Hit.point, m_Hit.normal, out surfaceNormal);

                                    if (!isEdge)
                                        RepairHitNormal();

                                    // Get the horizontal constrained hit normal and check if ground is flat  
                                    bool flat;
                                    Vector3 horizontalHit = GetHorizontalHitNormal(m_Hit.normal, out flat);

                                    //  Get the movement heading into hit
                                    float moveIntoHit = (flat) ? 0f : -Vector3.Dot(horizontal.normalized, horizontalHit);

                                    // Handle stepping up onto a ledge differently to the others
                                    if (isEdge && m_StepHeight > k_TinyValue && !flat && (1 - localHitNormal.y) * radius < (m_StepHeight + m_SkinWidth) && moveIntoHit > k_TinyValue && Vector3.Angle(surfaceNormal, m_CurrentUp) < m_StepMaxAngle &&
                                        PhysicsExtensions.RaycastFiltered(new Ray(m_TargetPosition + m_CurrentUp * m_SkinWidth, -m_CurrentUp), m_SkinWidth + m_StepHeight /*- radius * (1f - localHitNormal.y)*/, collisionMask, m_LocalTransform))
                                    {
                                        // Step up, finish move, step down. Added to start in reverse order
                                        float stepOverHeight = radius * (1f - localHitNormal.y) + m_SkinWidth;
                                        AddMoveFirst(-m_CurrentUp, stepOverHeight, false, VelocityCorrection.Full);
                                        AddMoveFirst(newMove, true, VelocityCorrection.None);
                                        AddMoveFirst(m_CurrentUp, stepOverHeight, true, VelocityCorrection.Full);

                                        // Don't trigger collision events for steps
                                        fireCollisionEvent = false;
                                    }
                                    else
                                    {
                                        Vector3 projected = Vector3.ProjectOnPlane(newMove, m_Hit.normal);

                                        // Get horizontal deflection (based on slope limit)
                                        float hitAngle = Mathf.Acos(localHitNormal.y) * Mathf.Rad2Deg;
                                        if (hitAngle >= m_SlopeLimit)
                                        {
                                            newMove = Vector3.ProjectOnPlane(horizontal, horizontalHit);
                                        }
                                        else
                                        {
                                            // Get the vector length projected onto the plane
                                            float projectedLength = projected.magnitude;

                                            // Create an impact plane and use to get height deflection
                                            Plane impactPlane = new Plane(-m_Hit.normal, 0f);

                                            float vOffset = 0f;
                                            impactPlane.Raycast(new Ray(horizontal, m_CurrentUp), out vOffset);
                                            if (vOffset < 0f)
                                            {
                                                // Horizontal is above plane
                                                newMove = horizontal + m_CurrentUp * Mathf.Max(verticalAmount, vOffset);
                                            }
                                            else
                                            {
                                                // Horizontal is below plane
                                                newMove = horizontal + m_CurrentUp * vOffset;
                                            }

                                            // Clamp speed to projected
                                            newMove = Vector3.ClampMagnitude(newMove, projectedLength);
                                        }

                                        float friction = slopeFriction;
                                        if (isEdge)
                                        {
                                            // Check step distance below dropoff for contact and don't slide if positive
                                            // Prevents sliding down stairs, etc
                                            if (m_StepHeight != 0f && PhysicsExtensions.RaycastFiltered(
                                                new Ray(m_TargetPosition + m_CurrentUp * m_SkinWidth, -m_CurrentUp),
                                                m_SkinWidth + m_StepHeight, collisionMask, m_LocalTransform))
                                            {
                                                friction = slopeFriction;
                                            }
                                            else
                                            {
                                                friction = Mathf.Min(slopeFriction, ledgeFriction);
                                            }
                                        }

                                        if (friction < 1f - k_TinyValue)
                                        {
                                            // Slerp
                                            newMove = Vector3.Lerp(projected, newMove, friction);
                                        }

                                        AddMoveFirst(newMove, true, VelocityCorrection.None);
                                    }
                                }
                                break;
                            default:
                                {
                                    // Horizontal wall deflection
                                    if (horizontalMagnitude > k_MinMoveDistance)
                                    {
                                        // Get the normal of the slope limit "invisible wall"
                                        Vector3 limitNormal = Vector3.ProjectOnPlane(m_Hit.normal, m_CurrentUp).normalized;

                                        // Get the up slope amount
                                        float upSlopeHeading = -Vector3.Dot(horizontal.normalized, limitNormal);

                                        // If trying to move up slope, deflect
                                        if (upSlopeHeading > 0f)
                                            newMove = Vector3.ProjectOnPlane(horizontal, limitNormal);
                                        else
                                            newMove = horizontal;
                                    }
                                    else
                                        newMove = Vector3.zero;

                                    // Vertical wall deflection
                                    if (Mathf.Abs(verticalAmount) > k_MinMoveDistance)
                                        newMove += DeflectVertical(verticalAmount, 0f, 0f);

                                    AddMoveFirst(newMove, true, VelocityCorrection.None);
                                }
                                break;
                        }
                    }
                    // Else might need to perform check if this was a step still to prevent firing collision events in edge-cases (no pun intended)
                    // Sort this if it causes problems, but the maths aint cheap so don't bother unless it's needed

                    if (fireCollisionEvent)
                    {
                        // Get pushable objects
                        INeoCharacterController hitCharacter = (pushCharacters) ? m_Hit.collider.GetComponent<INeoCharacterController>() : null;
                        Rigidbody hitRigidbody = (pushRigidbodies && m_Hit.rigidbody != null && hitCharacter == null && !m_Hit.rigidbody.isKinematic) ? m_Hit.rigidbody : null;

                        // Push characters
                        if (hitCharacter != null)
                        {
                            if (characterCollisionHandler != null)
                                characterCollisionHandler(hitCharacter, m_Hit.normal, collisionType);
                            else
                                DefaultCharacterCollisionHandler(hitCharacter, m_Hit.normal, collisionType);
                        }
                        else
                        {
                            // Push rigidbodies (not if it was attached to a character)
                            if (hitRigidbody != null && AddRigidbodyHit(hitRigidbody))
                            {
                                if (rigidbodyCollisionHandler != null)
                                    rigidbodyCollisionHandler(hitRigidbody, m_Hit.point, m_Hit.normal, collisionType);
                                else
                                    DefaultRigidbodyCollisionHandler(hitRigidbody, m_Hit.point, m_Hit.normal, collisionType);
                            }
                        }

                        // Get hit event handlers on target
                        m_Hit.collider.GetComponents(s_TargetHitHandlers);

                        // Fire hit events
                        if (onControllerHit != null || m_EventHandlers.Length > 0 || s_TargetHitHandlers.Count > 0)
                        {
                            // Build the controller hit data
                            var hit = new NeoCharacterControllerHit(
                                    this,
                                    collisionType,
                                    m_Hit.collider,
                                    m_Hit.rigidbody,
                                    m_Hit.transform,
                                    m_Hit.point,
                                    m_Hit.normal,
                                    move.moveDirection
                                );

                            // Send to event handlers on character (includes disabled)
                            for (int i = 0; i < m_EventHandlers.Length; ++i)
                            {
                                if (m_EventHandlers[i].enabled)
                                    m_EventHandlers[i].OnNeoCharacterControllerHit(hit);
                            }

                            // Send to event handlers on target
                            for (int i = 0; i < s_TargetHitHandlers.Count; ++i)
                            {
                                if (s_TargetHitHandlers[i].enabled)
                                    s_TargetHitHandlers[i].OnNeoCharacterControllerHit(hit);
                            }

                            // Fire generic collision event
                            if (onControllerHit != null)
                                onControllerHit(hit);
                        }

                        // Clear gathered event handlers
                        s_TargetHitHandlers.Clear();
                    }

                    // Record collision type for tick
                    collisionFlags |= collisionType;
                }
                else
                {
                    // No collision. Move the target position and sort corrections
                    Vector3 deltaPos = move.moveDirection * move.moveDistance;
#if VISUALISE_MOVEMENT
                    Color debugColor = (move.correction == VelocityCorrection.None) ? Color.black : Color.blue;
                    Debug.DrawLine(m_TargetPosition, m_TargetPosition + deltaPos, debugColor, 1f, false);
#endif
                    m_TargetPosition += deltaPos;
                    if (move.correction != VelocityCorrection.None)
                        AddCorrection(deltaPos, move.correction);
                }
                
                // Check if move iterations for this segment are too high
                if (++m_MoveIterations >= k_MoveIterationLimit)
                {
                    break;
                }
            }
        }

        Vector3 GetHorizontalHitNormal (Vector3 normal, out bool flat)
        {
            // Project
            Vector3 result = Vector3.ProjectOnPlane(normal, m_CurrentUp);

            // Check if flat (zero if true, normalise if not)
            float horizontalHitMagnitude = result.magnitude;
            flat = horizontalHitMagnitude < k_TinyValue;
            if (flat)
                result = Vector3.zero;
            else
                result /= horizontalHitMagnitude;

            return result;
        }
        
        Vector3 DeflectVertical (float v, float ignoreV, float friction)
        {
            Vector3 result = Vector3.zero;

            float totalV = v - ignoreV;
            if (Mathf.Abs(totalV) > k_TinyValue)
            {
                Vector3 vertical = m_CurrentUp * totalV;
                if (Vector3.Dot(m_CurrentUp, m_Hit.normal) < 0f)
                {
                    if (friction < 1f)
                        result += Vector3.ProjectOnPlane(vertical, m_Hit.normal) * (1f - friction);
                }
                else
                {
                    // If the horizontal has been deflected up/down, only apply v if it's greater than that amount
                    if (v > 0f)
                    {
                        if (v > ignoreV)
                            result += vertical;
                    }
                    else
                    {
                        if (v < ignoreV)
                            result += vertical;
                    }
                }
            }
            return result;
        }

        public void RepairHitNormal()
        {
            // Is this required in Unity 2019+?

            if (m_Hit.collider is MeshCollider && m_Hit.triangleIndex >= 0)
            {
                var collider = m_Hit.collider as MeshCollider;
                var mesh = collider.sharedMesh;
                if (mesh.isReadable)
                {
                    var tris = mesh.triangles;
                    var verts = mesh.vertices;

                    var v0 = verts[tris[m_Hit.triangleIndex * 3]];
                    var v1 = verts[tris[m_Hit.triangleIndex * 3 + 1]];
                    var v2 = verts[tris[m_Hit.triangleIndex * 3 + 2]];

                    var n = Vector3.Cross(v1 - v0, v2 - v1).normalized;

                    m_Hit.normal = m_Hit.transform.TransformDirection(n);
                }
            }
        }

        void ResetRigidbodyHits ()
        {
            m_NumRigidbodyHits = 0;
        }

        bool AddRigidbodyHit (Rigidbody rb)
        {
            for (int i = 0; i < m_NumRigidbodyHits; ++i)
            {
                if (m_HitRigidbodies[i] == rb)
                    return false;
            }

            if (m_NumRigidbodyHits == k_MaxRigidbodyHits - 1)
                return false;

            m_HitRigidbodies[m_NumRigidbodyHits] = rb;
            ++m_NumRigidbodyHits;

            return true;
        }

        public void DefaultCharacterCollisionHandler (INeoCharacterController other, Vector3 normal, NeoCharacterCollisionFlags flags)
        {
            if (other.mass <= mass && other.pushedByCharacters)
            {
                float pushPower = m_CharacterPush * other.mass;
                other.AddForce(normal * -pushPower);
            }
        }

        public void DefaultRigidbodyCollisionHandler (Rigidbody rb, Vector3 hitPoint, Vector3 hitNormal, NeoCharacterCollisionFlags hitType)
        {
            if (rb.mass <= m_MaxRigidbodyPushMass)
            {
                float pushPower = Mathf.Lerp(m_RigidbodyPush, 0f, (rb.mass - m_LowRigidbodyPushMass) / (m_MaxRigidbodyPushMass - m_LowRigidbodyPushMass));
                rb.AddForceAtPosition(hitNormal * pushPower * -rb.mass, hitPoint);
            }
        }
        
        void OnCollisionEnter(Collision collision)
        {
            // Check tracked rigidbodies
            if (collision.rigidbody != null && AddRigidbodyHit(collision.rigidbody) && collision.rigidbody.mass >= k_MinRigidbodyKnockMass)
            {
                // Add force to character for next frame
                AddForce(collision.GetContact(0).normal * collision.rigidbody.mass * collision.relativeVelocity.magnitude);
            }
        }

        void CheckGrounding ()
        {
            platform = null;
            bool wasGrounded = isGrounded;

            //#if UNITY_2018_4_OR_NEWER
            //            bool hitGround = collisionsEnabled && Physics.SphereCast(GetCapsuleBottom(), radius - m_SkinWidth, -m_CurrentUp, out m_Hit, k_GroundingCheckDistance + m_SkinWidth, collisionMask, QueryTriggerInteraction.Ignore);
            //#else
            // Need to double check sphere casts as Unity has a bug that spherecasts on an edge (eg the invisible edge dividing a quad) are off by a large enough distance to not register here, and return a crazy normal
            // Should have been fixed in 2018.3
            bool hitGround = false;
            if (collisionsEnabled)
            {
                hitGround = Physics.SphereCast(GetCapsuleBottom(), radius - m_SkinWidth, -m_CurrentUp, out m_Hit, k_GroundingCheckDistance + m_SkinWidth, collisionMask, QueryTriggerInteraction.Ignore);
                if (!hitGround)
                    hitGround = Physics.Raycast(GetCapsuleBottom(), -m_CurrentUp, out m_Hit, radius + k_GroundingCheckDistance, collisionMask, QueryTriggerInteraction.Ignore);
            }
//#endif

            if (hitGround)
            {
                // Check if hit was technically a wall hit
                Vector3 localHitNormal = m_InverseRotation * m_Hit.normal;
                if (localHitNormal.y < m_SinWallAngle)
                    hitGround = false;
            }

            if (hitGround)
            {
                isGrounded = true;
                airTime = 0f;

                // Check if the ground contact is a moving platform
                if (m_Hit.rigidbody != null)
                    platform = m_Hit.transform.GetComponent<IMovingPlatform>();

                // Get ground normals
                groundNormal = m_Hit.normal;
                Vector3 surfaceNormal;
                GetSurfaceNormalFromHit(m_Hit.point, m_Hit.normal, out surfaceNormal);
                groundSurfaceNormal = surfaceNormal;
            }
            else
            {
                // Clear ground variables
                isGrounded = false;
                groundNormal = Vector3.zero;
                groundSurfaceNormal = Vector3.zero;
                // Track time in the air
                airTime += Time.deltaTime;
            }
        }

        bool IsPartOfHeirarchy (Transform t)
        {
            while (t != null)
            {
                if (t == m_LocalTransform)
                {
                    t = null;
                    return true;
                }
                t = t.parent;
            }
            return false;
        }

        bool GetPenetration (Collider c, Transform t, out Vector3 depentrationVector)
        {
            Vector3 direction;
            float distance;

            if (t != null && Physics.ComputePenetration(m_Capsule, m_TargetPosition, m_TargetRotation, c, t.position, t.rotation, out direction, out distance))
            {
                // Normalize direction if required
                if (direction.sqrMagnitude > 1.0001f)
                    direction.Normalize();

                // Get a clamped depenetration vector for this overlap
                depentrationVector = direction * Mathf.Min(distance, k_MaxDepenetrationDistance);
                return true;
            }

            depentrationVector = Vector3.zero;
            return false;
        }

        void Depenetrate()
        {
            if (!collisionsEnabled)
                return;

            Vector3 totalDepenetration = Vector3.zero;

            m_Depenetrations = 0;
            while (true)
            {
                // Get overlapping colliders
                int overlaps = Physics.OverlapCapsuleNonAlloc(GetCapsuleBottom(), GetCapsuleTop(), radius - m_SkinWidth, m_OverlapColliders, depenetrationMask, QueryTriggerInteraction.Ignore);
                if (overlaps == 0)
                    return;

                bool depenetrated = false;

                // Check static colliders
                for (int i = 0; i < overlaps; ++i)
                {
                    // Skip self
                    if (m_OverlapColliders[i] == m_Capsule || m_OverlapColliders[i].attachedRigidbody != null)
                        continue;

                    // Discard if part of character heirarchy
                    Transform t = m_OverlapColliders[i].transform;
                    if (IsPartOfHeirarchy(t))
                        continue;

                    // Get penetration
                    Vector3 depenetrationVector;
                    if (GetPenetration(m_OverlapColliders[i], t, out depenetrationVector))
                    {
                        totalDepenetration += depenetrationVector;
                        depenetrated = true;
                    }
                }

                if (depenetrated)
                {
#if VISUALISE_MOVEMENT
                    Debug.DrawLine(m_TargetPosition, m_TargetPosition + totalDepenetration, Color.red, 1f, false);
#endif
                    m_TargetPosition += totalDepenetration;
                    m_PositionCorrection += totalDepenetration;
                    ++m_Depenetrations;
                    if (m_Depenetrations >= k_LowDepenetrationLimit)
                        return;
                    continue;
                }

                // Check kinematic colliders
                for (int i = 0; i < overlaps; ++i)
                {
                    // Skip self
                    if (m_OverlapColliders[i] == m_Capsule || m_OverlapColliders[i].attachedRigidbody == null)
                        continue;

                    // Discard if part of character heirarchy
                    Transform t = m_OverlapColliders[i].transform;
                    if (IsPartOfHeirarchy(t))
                        continue;

                    // Get penetration
                    Vector3 depenetrationVector;
                    if (GetPenetration(m_OverlapColliders[i], t, out depenetrationVector))
                    {
                        totalDepenetration += depenetrationVector;
                        depenetrated = true;
                    }
                }

                if (depenetrated)
                {
                    m_TargetPosition += totalDepenetration;
                    m_PositionCorrection += totalDepenetration;
                    ++m_Depenetrations;
                    if (m_Depenetrations >= k_HighDepenetrationLimit)
                        return;
                    continue;
                }
                else
                    return;
            }
        }

        bool GetSurfaceNormalFromHit(Vector3 hitPoint, Vector3 hitNormal, out Vector3 outNormal)
        {
            // Pre-2018.3 there's issues with hits near meshcollider edges. Could get normal from triangle indexes instead
            // but it would be much more expensive. Only worthwhile if this starts behaving super erratically

            Vector3 planeUp = Vector3.ProjectOnPlane(m_CurrentUp, hitNormal);
            float mag = planeUp.magnitude;
            planeUp *= m_GroundHitLookahead / mag;

            RaycastHit hit;
            if (PhysicsExtensions.RaycastNonAllocSingle(
                new Ray(hitPoint + planeUp + hitNormal * 0.25f, -hitNormal),
                out hit,
                0.75f,
                collisionMask,
                m_LocalTransform,
                QueryTriggerInteraction.Ignore
                ))
            {
                // Check if shallower surface than original hit
                if (Vector3.Dot(hit.normal, m_CurrentUp) > Vector3.Dot(hitNormal, m_CurrentUp) + k_TinyValue)
                {
                    // It was an edge
                    outNormal = hit.normal;
                    return true;
                }
            }

            // Return original normal
            outNormal = hitNormal;
            return false;
        }

        void ApplyGroundForce ()
        {
            if (!collisionsEnabled || !isGrounded)
                return;
            
            Vector3 bottom = GetCapsuleBottom();

            if (PhysicsExtensions.SphereCastNonAllocSingle(
                new Ray(bottom, -m_CurrentUp),
                radius - m_SkinWidth,
                out m_Hit,
                m_GroundSnapHeight + m_SkinWidth,
                collisionMask,
                m_LocalTransform,
                QueryTriggerInteraction.Ignore
                ))
            {
                float distance = m_Hit.distance;
                bool snapDown = false;

                // Check if moving into hit normal
                Vector3 moved = m_TargetPosition - m_StartPosition;
                if (Vector3.Dot(moved.normalized, Vector3.ProjectOnPlane(m_Hit.normal, m_CurrentUp).normalized) < 0.25f)
                    snapDown = true;
                else
                {
                    // Check the surface normal. If it's a slope, snap down
                    Vector3 surface;
                    //bool isEdge = GetSurfaceNormalFromHit(m_Hit.point, m_Hit.normal, out surface);
                    //if (CompareVector3s(m_Hit.normal, surface, k_TinyValue))
                    if (GetSurfaceNormalFromHit(m_Hit.point, m_Hit.normal, out surface))
                        snapDown = true;
                    else
                    {
                        // Check if the step height is smaller than the step down
                        if (PhysicsExtensions.RaycastNonAllocSingle(
                            new Ray(bottom, -m_CurrentUp),
                            out m_Hit,
                            m_GroundSnapHeight + radius,
                            collisionMask,
                            m_LocalTransform,
                            QueryTriggerInteraction.Ignore
                            ))
                        {
                            snapDown = true;
                        }
                    }
                }

                if (snapDown)
                {
                    Vector3 groundOffset = m_CurrentUp * -(distance - m_SkinWidth);
#if VISUALISE_MOVEMENT
                    Debug.DrawLine(m_TargetPosition, m_TargetPosition + groundOffset, Color.magenta, 1f, false);
#endif
                    m_TargetPosition += groundOffset;
                    m_PositionCorrection += groundOffset;

                    collisionFlags |= NeoCharacterCollisionFlags.Below;
                }
            }
        }

        bool CheckValidVector (Vector3 v)
        {
            bool result = !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z);
#if UNITY_EDITOR
            if (!result)
                Debug.LogError("Trying to add invalid move vector to buffer");
#endif
            return result;
        }

        void ClearMoveBuffer()
        {
            m_MoveCount = 0;
        }

        void AddCorrection (Vector3 deltaPos, VelocityCorrection correction)
        {
            switch (correction)
            {
                case VelocityCorrection.Full:
                    m_PositionCorrection += deltaPos;
                    break;
                case VelocityCorrection.Vertical:
                    m_PositionCorrection += Vector3.Project(deltaPos, m_CurrentUp);
                    break;
                case VelocityCorrection.Horizontal:
                    m_PositionCorrection += Vector3.ProjectOnPlane(deltaPos, m_CurrentUp);
                    break;
            }
        }

        bool GetMoveVector(out MoveSegment move)
        {
            if (m_MoveCount < 1)
            {
                move = new MoveSegment(Vector3.zero, 0f, false, VelocityCorrection.None);
                return false;
            }

            move = m_MoveSegments[m_MoveIndex];

            ++m_MoveIndex;
            if (m_MoveIndex == k_MoveBufferLength)
                m_MoveIndex = 0;

            --m_MoveCount;

            return true;
        }

        void AddMoveLast (Vector3 deltaPosition, bool slide, VelocityCorrection correction)
        {
            float moveDistance = deltaPosition.magnitude;
            if (moveDistance < k_TinyValue)
                return;

            Vector3 moveDirection = deltaPosition / moveDistance;
            if (!CheckValidVector(moveDirection))
                return;

            if (m_MoveCount == k_MoveBufferLength)
                return;

            ++m_MoveCount;

            int index = m_MoveIndex + m_MoveCount - 1;
            if (index >= k_MoveBufferLength)
                index -= k_MoveBufferLength;

            m_MoveSegments[index] = new MoveSegment(moveDirection, moveDistance, slide, correction);
        }

        void AddMoveLast(Vector3 moveDirection, float moveDistance, bool slide, VelocityCorrection correction)
        {
            if (moveDistance < k_TinyValue)
                return;

            if (!CheckValidVector(moveDirection))
                return;

            if (m_MoveCount == k_MoveBufferLength)
                return;

            ++m_MoveCount;

            int index = m_MoveIndex + m_MoveCount - 1;
            if (index >= k_MoveBufferLength)
                index -= k_MoveBufferLength;

            m_MoveSegments[index] = new MoveSegment(moveDirection, moveDistance, slide, correction);
        }

        void AddMoveFirst(Vector3 deltaPosition, bool slide, VelocityCorrection correction)
        {
            float moveDistance = deltaPosition.magnitude;
            if (moveDistance < k_TinyValue)
                return;

            Vector3 moveDirection = deltaPosition / moveDistance;
            if (!CheckValidVector(moveDirection))
                return;

            --m_MoveIndex;
            if (m_MoveIndex < 0)
                m_MoveIndex = k_MoveBufferLength - 1;

            m_MoveSegments[m_MoveIndex] = new MoveSegment(moveDirection, moveDistance, slide, correction);

            if (m_MoveCount != k_MoveBufferLength)
                ++m_MoveCount;
        }

        void AddMoveFirst(Vector3 moveDirection, float moveDistance, bool slide, VelocityCorrection correction)
        {
            if (moveDistance < k_TinyValue)
                return;

            if (!CheckValidVector(moveDirection))
                return;

            --m_MoveIndex;
            if (m_MoveIndex < 0)
                m_MoveIndex = k_MoveBufferLength - 1;

            m_MoveSegments[m_MoveIndex] = new MoveSegment(moveDirection, moveDistance, slide, correction);

            if (m_MoveCount != k_MoveBufferLength)
                ++m_MoveCount;
        }

        /// <summary>
        /// Move to the new position and rotation. Updates velocity and properties to match new frame of reference
        /// </summary>
        /// <param name="position">The position the character should move to.</param>
        /// <param name="rotation">The new rotation for the character.</param>
        /// <param name="relativeRotation">Is the rotation parameter relative to the current rotation or absolute?</param>
        public void Teleport(Vector3 position, Quaternion rotation, bool relativeRotation = true)
        {
            // Set the new position
            m_Rigidbody.interpolation = RigidbodyInterpolation.None;
            m_LocalTransform.position = m_StartPosition = m_TargetPosition = position;
            m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            // Set the new rotation
            if (relativeRotation)
            {
                m_StartRotation *= rotation;
                m_TargetRotation *= rotation;

                // Reset the velocity, etc
                velocity = rotation * velocity;
                rawVelocity = rotation * rawVelocity;
            }
            else
            {
                var relative = rotation * Quaternion.Inverse(m_StartRotation);

                m_StartRotation = rotation;
                m_TargetRotation = rotation;
                velocity = relative * velocity;
                rawVelocity = relative * rawVelocity;
            }

            // Depenetrate from any obstacles
            Depenetrate();

            // Fire event if required
            if (onTeleported != null)
                onTeleported.Invoke();

            // Block ground snapping for this frame
            m_BlockGroundSnapping = true;
        }

        /// <summary>
        /// Add a force to the character outside of the usual movement logic (eg. explosions, etc)
        /// </summary>
        /// <param name="force">Force vector in world coordinates.</param>
        /// <param name="mode">Type of force to apply. See the <see href="https://docs.unity3d.com/ScriptReference/ForceMode.html">Unity Scripting Reference.</see></param>
        /// <param name="disableGroundSnapping">Block the controller from snapping to the ground on the frame this is applied</param>
        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force, bool disableGroundSnapping = false)
        {
            // Factor in time and mass as required
            float multiplier = 1f;
            switch (mode)
            {
                case ForceMode.Force:
                    multiplier = Time.fixedDeltaTime / mass;
                    break;
                case ForceMode.Acceleration:
                    multiplier = Time.fixedDeltaTime;
                    break;
                case ForceMode.Impulse:
                    multiplier = 1f / mass;
                    break;
            }
            force *= multiplier;
            m_ExternalForceMove += force;
            m_BlockGroundSnapping = disableGroundSnapping;
        }
        
        Quaternion GetYawFromRotation(Quaternion rotation)
        {
            Vector3 rotatedForward = rotation * forward;
            rotatedForward = Vector3.ProjectOnPlane(rotatedForward, m_CurrentUp).normalized;
            return Quaternion.FromToRotation(forward, rotatedForward);

            // Alternate (needs testing)

            //Vector3 ra = new Vector3(rotation.x, rotation.y, rotation.z);
            //Vector3 p = Vector3.Project(ra, m_CurrentUp);
            //Vector4 twist = new Vector4(p.x, p.y, p.z, rotation.w);
            //twist.Normalize();

            //return new Quaternion(twist.x, twist.y, twist.z, twist.w);
        }

        static bool CompareVector3s(Vector3 v1, Vector3 v2, float tolerance)
        {
            float difference = Mathf.Abs(v1.x - v2.x);
            difference += Mathf.Abs(v1.y - v2.y);
            difference += Mathf.Abs(v1.z - v2.z);
            return difference <= tolerance;
        }

        public bool RayCast(float normalisedHeight, Vector3 castVector, Space space, int layerMask = -5)
        {
            return PhysicsExtensions.RaycastFiltered(
                new Ray(GetRayCastSource(normalisedHeight),
                    space == Space.Self ? m_LocalTransform.rotation * castVector : castVector),
                castVector.magnitude,
                layerMask,
                m_LocalTransform
            );
        }

        public bool RayCast(float normalisedHeight, Vector3 castVector, Space space, out RaycastHit hit, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return PhysicsExtensions.RaycastNonAllocSingle(
                new Ray(GetRayCastSource(normalisedHeight),
                    space == Space.Self ? m_LocalTransform.rotation * castVector : castVector),
                out hit,
                castVector.magnitude,
                layerMask,
                m_LocalTransform,
                queryTriggerInteraction
            );
        }

        public bool SphereCast(float normalisedHeight, Vector3 castVector, Space space, int layerMask = -5)
        {
            return PhysicsExtensions.SphereCastFiltered(
                new Ray(GetSphereCastSource(normalisedHeight),
                    space == Space.Self ? m_LocalTransform.rotation * castVector : castVector),
                radius - m_SkinWidth,
                castVector.magnitude,
                layerMask,
                m_LocalTransform
            );
        }

        public bool SphereCast(float normalisedHeight, Vector3 castVector, Space space, out RaycastHit hit, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            bool didHit = PhysicsExtensions.SphereCastNonAllocSingle(
                new Ray(GetSphereCastSource(normalisedHeight),
                    space == Space.Self ? m_LocalTransform.rotation * castVector : castVector),
                radius - m_SkinWidth,
                out hit,
                castVector.magnitude + m_SkinWidth,
                layerMask,
                m_LocalTransform,
                queryTriggerInteraction
            );

            if (didHit)
                hit.distance -= m_SkinWidth;

            return didHit;
        }

        public bool CapsuleCast(Vector3 castVector, Space space, int layerMask = -5)
        {
            float magnitude = castVector.magnitude;
            if (magnitude < k_TinyValue)
                return false;

            Vector3 root = m_LocalTransform.position;
            Vector3 direction = (space == Space.Self) ? m_LocalTransform.rotation * castVector : castVector;
            direction /= magnitude;

            return PhysicsExtensions.CapsuleCastFiltered(
                root + m_CurrentUp * radius,
                root + m_CurrentUp * (height - radius),
                radius - m_SkinWidth,
                direction,
                castVector.magnitude + m_SkinWidth,
                layerMask,
                m_LocalTransform
            );
        }

        public bool CapsuleCast(Vector3 castVector, Space space, out RaycastHit hit, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            float magnitude = castVector.magnitude;
            if (magnitude < k_TinyValue)
            {
                hit = new RaycastHit();
                return false;
            }

            Vector3 root = m_LocalTransform.position;
            Vector3 direction = (space == Space.Self) ? m_LocalTransform.rotation * castVector : castVector;
            direction /= magnitude;

            bool didHit = PhysicsExtensions.CapsuleCastNonAllocSingle(
                root + m_CurrentUp * radius,
                root + m_CurrentUp * (height - radius),
                radius - m_SkinWidth,
                direction,
                out hit,
                castVector.magnitude + m_SkinWidth,
                layerMask,
                m_LocalTransform,
                queryTriggerInteraction
            );

            if (didHit)
                hit.distance -= m_SkinWidth;

            return didHit;
        }

        Vector3 GetRayCastSource (float normalisedHeight)
        {
            return m_TargetPosition + m_CurrentUp * (normalisedHeight * height);
        }

        Vector3 GetSphereCastSource(float normalisedHeight)
        { 
            return m_TargetPosition + m_CurrentUp * (radius + (height - radius * 2f) * normalisedHeight);
        }

#region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_PositionKey = new NeoSerializationKey("position");
        private static readonly NeoSerializationKey k_RotationKey = new NeoSerializationKey("rotation");
        private static readonly NeoSerializationKey k_CollisionMaskKey = new NeoSerializationKey("collisionMask");
        private static readonly NeoSerializationKey k_DepenetrationMaskKey = new NeoSerializationKey("depenetrationMask");
        private static readonly NeoSerializationKey k_IgnorePlatformsKey = new NeoSerializationKey("ignorePlatforms");
        private static readonly NeoSerializationKey k_OrientUpWithGravityKey = new NeoSerializationKey("orientUpWithGravity");
        private static readonly NeoSerializationKey k_UpSmoothingKey = new NeoSerializationKey("upSmoothing");
        private static readonly NeoSerializationKey k_UpKey = new NeoSerializationKey("up");
        private static readonly NeoSerializationKey k_GravityKey = new NeoSerializationKey("gravity");
        private static readonly NeoSerializationKey k_LockUpVectorKey = new NeoSerializationKey("lockUpVector");
        private static readonly NeoSerializationKey k_SlopeFrictionKey = new NeoSerializationKey("slopeFriction");
        private static readonly NeoSerializationKey k_LedgeFrictionKey = new NeoSerializationKey("ledgeFriction");
        private static readonly NeoSerializationKey k_CollisionsEnabledKey = new NeoSerializationKey("collisionsEnabled");
        private static readonly NeoSerializationKey k_HeightKey = new NeoSerializationKey("height");
        private static readonly NeoSerializationKey k_RadiusKey = new NeoSerializationKey("radius");
        private static readonly NeoSerializationKey k_StepHeightKey = new NeoSerializationKey("stepHeight");
        private static readonly NeoSerializationKey k_SlopeLimitKey = new NeoSerializationKey("slopeLimit");
        private static readonly NeoSerializationKey k_MassKey = new NeoSerializationKey("mass");
        private static readonly NeoSerializationKey k_PushRigidbodiesKey = new NeoSerializationKey("pushRigidbodies");
        private static readonly NeoSerializationKey k_PushCharactersKey = new NeoSerializationKey("pushCharacters");
        private static readonly NeoSerializationKey k_PushedByCharactersKey = new NeoSerializationKey("pushedByCharacters");
        private static readonly NeoSerializationKey k_IgnoreExternalForcesKey = new NeoSerializationKey("ignoreExternalForces");

        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");
        private static readonly NeoSerializationKey k_RawVelocityKey = new NeoSerializationKey("rawVelocity");
        private static readonly NeoSerializationKey k_AirTimeKey = new NeoSerializationKey("airTime");
        private static readonly NeoSerializationKey k_IsGroundedKey = new NeoSerializationKey("isGrounded");
        private static readonly NeoSerializationKey k_GroundNormalKey = new NeoSerializationKey("groundNormal");
        private static readonly NeoSerializationKey k_GroundSurfaceNormalKey = new NeoSerializationKey("groundSurfaceNormal");
        private static readonly NeoSerializationKey k_GroundSnapHeightKey = new NeoSerializationKey("snapHeight");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (saveMode == SaveMode.Default)
            {
                writer.WriteValue(k_PositionKey, m_TargetPosition);
                writer.WriteValue(k_RotationKey, m_TargetRotation);
                writer.WriteValue(k_CollisionMaskKey, collisionMask);
                writer.WriteValue(k_DepenetrationMaskKey, depenetrationMask);
                writer.WriteValue(k_IgnorePlatformsKey, ignorePlatforms);
                writer.WriteValue(k_UpSmoothingKey, upSmoothing);
                writer.WriteValue(k_OrientUpWithGravityKey, orientUpWithGravity);
                if (!orientUpWithGravity)
                    writer.WriteValue(k_UpKey, up);
                writer.WriteValue(k_GravityKey, gravity);
                writer.WriteValue(k_LockUpVectorKey, lockUpVector);
                writer.WriteValue(k_SlopeFrictionKey, slopeFriction);
                writer.WriteValue(k_LedgeFrictionKey, ledgeFriction);
                writer.WriteValue(k_CollisionsEnabledKey, collisionsEnabled);
                writer.WriteValue(k_HeightKey, height);
                writer.WriteValue(k_RadiusKey, radius);
                writer.WriteValue(k_StepHeightKey, stepHeight);
                writer.WriteValue(k_GroundSnapHeightKey, groundSnapHeight);
                writer.WriteValue(k_SlopeLimitKey, slopeLimit);
                writer.WriteValue(k_MassKey, mass);
                writer.WriteValue(k_PushRigidbodiesKey, pushRigidbodies);
                writer.WriteValue(k_PushCharactersKey, pushCharacters);
                writer.WriteValue(k_PushedByCharactersKey, pushedByCharacters);
                writer.WriteValue(k_IgnoreExternalForcesKey, ignoreExternalForces);

                writer.WriteValue(k_VelocityKey, velocity);
                writer.WriteValue(k_RawVelocityKey, rawVelocity);
                writer.WriteValue(k_AirTimeKey, airTime);
                writer.WriteValue(k_IsGroundedKey, isGrounded);
                writer.WriteValue(k_GroundNormalKey, groundNormal);
                writer.WriteValue(k_GroundSurfaceNormalKey, groundSurfaceNormal);
            }
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            Initialise();

            bool boolResult = false;
            float floatResult = 0f;
            int intResult = 0;
            Vector3 vector3Result = Vector3.zero;

            reader.TryReadValue(k_PositionKey, out m_TargetPosition, m_TargetPosition);
            reader.TryReadValue(k_RotationKey, out m_TargetRotation, m_TargetRotation);
            m_StartPosition = m_TargetPosition;
            m_StartRotation = m_TargetRotation;

            if (reader.TryReadValue(k_CollisionMaskKey, out intResult, collisionMask))
                collisionMask = intResult;
            if (reader.TryReadValue(k_DepenetrationMaskKey, out intResult, depenetrationMask))
                depenetrationMask = intResult;
            if (reader.TryReadValue(k_IgnorePlatformsKey, out boolResult, ignorePlatforms))
                ignorePlatforms = boolResult;
            if (reader.TryReadValue(k_UpSmoothingKey, out floatResult, upSmoothing))
                upSmoothing = floatResult;
            if (reader.TryReadValue(k_OrientUpWithGravityKey, out boolResult, orientUpWithGravity))
                orientUpWithGravity = boolResult;
            if (!orientUpWithGravity && reader.TryReadValue(k_UpKey, out vector3Result, up))
                up = vector3Result;
            if (reader.TryReadValue(k_GravityKey, out vector3Result, gravity))
                gravity = vector3Result;
            if (reader.TryReadValue(k_LockUpVectorKey, out boolResult, lockUpVector))
                lockUpVector = boolResult;
            if (reader.TryReadValue(k_SlopeFrictionKey, out floatResult, slopeFriction))
                slopeFriction = floatResult;
            if (reader.TryReadValue(k_LedgeFrictionKey, out floatResult, ledgeFriction))
                ledgeFriction = floatResult;
            if (reader.TryReadValue(k_CollisionsEnabledKey, out boolResult, collisionsEnabled))
                collisionsEnabled = boolResult;
            if (reader.TryReadValue(k_HeightKey, out floatResult, height))
            {

                m_Capsule.height = floatResult;
                m_Capsule.center = new Vector3(0f, floatResult * 0.5f);
                m_TargetHeight = 0f;
                m_TargetHeightCounter = -1;
            }
            if (reader.TryReadValue(k_RadiusKey, out floatResult, radius))
                radius = floatResult;
            if (reader.TryReadValue(k_StepHeightKey, out floatResult, stepHeight))
                stepHeight = floatResult;
            if (reader.TryReadValue(k_GroundSnapHeightKey, out floatResult, groundSnapHeight))
                groundSnapHeight = floatResult;
            if (reader.TryReadValue(k_SlopeLimitKey, out floatResult, slopeLimit))
                slopeLimit = floatResult;
            if (reader.TryReadValue(k_MassKey, out floatResult, mass))
                mass = floatResult;
            if (reader.TryReadValue(k_PushRigidbodiesKey, out boolResult, pushRigidbodies))
                pushRigidbodies = boolResult;
            if (reader.TryReadValue(k_PushCharactersKey, out boolResult, pushCharacters))
                pushCharacters = boolResult;
            if (reader.TryReadValue(k_PushedByCharactersKey, out boolResult, pushedByCharacters))
                pushedByCharacters = boolResult;
            if (reader.TryReadValue(k_IgnoreExternalForcesKey, out boolResult, ignoreExternalForces))
                ignoreExternalForces = boolResult;

            if (reader.TryReadValue(k_VelocityKey, out vector3Result, velocity))
                velocity = vector3Result;
            if (reader.TryReadValue(k_RawVelocityKey, out vector3Result, rawVelocity))
                rawVelocity = vector3Result;
            if (reader.TryReadValue(k_AirTimeKey, out floatResult, airTime))
                airTime = floatResult;
            if (reader.TryReadValue(k_IsGroundedKey, out boolResult, isGrounded))
                isGrounded = boolResult;
            if (reader.TryReadValue(k_GroundNormalKey, out vector3Result, groundNormal))
                groundNormal = vector3Result;
            if (reader.TryReadValue(k_GroundSurfaceNormalKey, out vector3Result, groundSurfaceNormal))
                groundSurfaceNormal = vector3Result;
        }

#endregion

#region DEBUGGING
        
        public Vector3 debugExternalForceMove
        {
            get;
            private set;
        }
        public bool debugSnapToGround
        {
            get;
            private set;
        }
        public int debugMoveIterations
        {
            get { return m_MoveIterations; }
        }
        public int debugDepenetrationCount
        {
            get { return m_Depenetrations; }
        }

#endregion
    }
}