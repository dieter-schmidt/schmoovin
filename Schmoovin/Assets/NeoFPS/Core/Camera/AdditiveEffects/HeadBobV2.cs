using UnityEngine;
using NeoFPS.CharacterMotion;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-headbobv2.html")]
    public class HeadBobV2 : MonoBehaviour, IAdditiveTransform
    {
        [SerializeField, Tooltip("The maximum position offset along the horizontal axis in either direction.")]
        private float m_HorizontalDistance = 0.075f;

        [SerializeField, Tooltip("The maximum position offset along the vertical axis in either direction.")]
        private float m_VerticalDistance = 0.1f;

        [SerializeField, Tooltip("The maximum angle to roll the camera in either direction.")]
        private float m_RollAngle = 0.25f;

        [SerializeField, Tooltip("The horizontal position curve over one step cycle for the weapon bob.")]
        private AnimationCurve m_HorizontalCurve = new AnimationCurve(
            new Keyframe(-1f, 0f), new Keyframe(-0.5f, 1f),
            new Keyframe(0f, 0f), new Keyframe(0.5f, -1f),
            new Keyframe(1f, 0f));

        [SerializeField, Tooltip("The vertical position curve over one step cycle for the weapon bob.")]
        private AnimationCurve m_VerticalCurve = new AnimationCurve(
            new Keyframe(-1f, 1f), new Keyframe(-0.65f, -0.6f), new Keyframe(-0.4f, 0f),
            new Keyframe(0f, 1f), new Keyframe(0.35f, -0.6f), new Keyframe(0.6f, 0f),
            new Keyframe(1f, 1f));

        [SerializeField, Tooltip("The roll curve over one step cycle for the weapon bob.")]
        private AnimationCurve m_RollCurve = new AnimationCurve(
            new Keyframe(-1f, 1f), new Keyframe(-0.85f, -0.15f), new Keyframe(-0.8f, 0.05f), new Keyframe(-0.75f, 0f),
            new Keyframe(0f, -1f), new Keyframe(0.15f, 0.15f), new Keyframe(0.2f, -0.05f), new Keyframe(0.25f, 0f),
            new Keyframe(1f, 1f));

        [SerializeField, Range(0f, 5f), Tooltip("At or below this speed the bob will be scaled to zero.")]
        private float m_MinLerpSpeed = 0.5f;

        [SerializeField, Range(0.25f, 10f), Tooltip("At or above this speed the bob will have its full effect.")]
        private float m_MaxLerpSpeed = 4f;

        [Header("Aim Compensation")]

        [SerializeField, Tooltip("Aim compensation involves rotating the camera so that the crosshair stays fixed on the same point in space.")]
        private bool m_UseAimCompensation = true;

        [SerializeField, Tooltip("The transform to use for camera/crosshair casting.")]
        private Transform m_AimTransform = null;

        [SerializeField, Tooltip("The layers to check against for aim depth.")]
        private LayerMask m_AimLayers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Delayed, Tooltip("The minimum distance to compensate against. At very close distances, the bob will have to rotate larger angles to keep the crosshair fixed on target.")]
        private float m_MinDistance = 2.5f;

        [SerializeField, Delayed, Tooltip("The maximum distance to compensate against.")]
        private float m_MaxDistance = 200f;

        [SerializeField, Range(0f, 1f), Tooltip("A damping against the crosshair target. Prevents objects crossing the camera at close ranges from causing sudden shifts.")]
        private float m_Damping = 0.25f;

        private const float k_FadeLerp = 0.05f;

        private Vector3 m_BobPosition = Vector3.zero;
        private Quaternion m_BobRotation = Quaternion.identity;
        private float m_SettingsStrength = 1f;

        private float m_Weight = 1f;
        private float m_TargetWeight = 0f;
        private float m_CompensationDistance = 200f;
        private float m_CompenstationSpeed = 0f;

        private IAdditiveTransformHandler m_Handler = null;
        private MotionController m_Controller = null;

        public Quaternion rotation
        {
            get { return Quaternion.Slerp(Quaternion.identity, m_BobRotation, m_SettingsStrength); }
        }

        public Vector3 position
        {
            get { return m_BobPosition * m_SettingsStrength; }
        }

        public bool bypassPositionMultiplier
        {
            get { return false; }
        }

        public bool bypassRotationMultiplier
        {
            get { return false; }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            m_HorizontalDistance = Mathf.Clamp(m_HorizontalDistance, 0f, 0.5f);
            m_VerticalDistance = Mathf.Clamp(m_VerticalDistance, 0f, 0.5f);
            m_MinDistance = Mathf.Clamp(m_MinDistance, 1f, m_MaxDistance);
            m_MaxDistance = Mathf.Clamp(m_MaxDistance, m_MinDistance, 100f);

            if (m_AimTransform == null)
                m_AimTransform = transform;
        }
#endif

        void Awake()
        {
            m_Controller = GetComponentInParent<MotionController>();
            m_Handler = GetComponent<IAdditiveTransformHandler>();
            if (m_AimTransform == null)
                m_AimTransform = transform;

            FpsSettings.gameplay.onHeadBobChanged += OnHeadBobSettingsChanged;
            OnHeadBobSettingsChanged(FpsSettings.gameplay.headBob);
        }

        void OnDestroy()
        {
            FpsSettings.gameplay.onHeadBobChanged -= OnHeadBobSettingsChanged;
        }

        private void OnHeadBobSettingsChanged(float bobStrength)
        {
            m_SettingsStrength = bobStrength;
            if (bobStrength < 0.01f)
                enabled = false;
            else
                enabled = true;
        }

        void OnEnable()
        {
            m_Handler.ApplyAdditiveEffect(this);

            m_CompensationDistance = m_MaxDistance;
            m_CompenstationSpeed = 0f;
        }

        void OnDisable()
        {
            m_Handler.RemoveAdditiveEffect(this);
        }

        void FixedUpdate()
        {
            m_Weight = Mathf.Lerp(m_Weight, m_TargetWeight, k_FadeLerp);
        }

        public void UpdateTransform()
        {
            float speed = m_Controller.smoothedStepRate;
            if (m_Controller.strideLength == 0f || speed < m_MinLerpSpeed)
                m_TargetWeight = 0f;
            else
                m_TargetWeight = (m_Controller.characterController.velocity.magnitude - m_MinLerpSpeed) / (m_MaxLerpSpeed - m_MinLerpSpeed);

            if (m_Weight > 0.0001f)
            {
                float xCurve = m_HorizontalCurve.Evaluate(m_Controller.stepCounter);
                float yCurve = m_VerticalCurve.Evaluate(m_Controller.stepCounter);
                float rCurve = m_RollCurve.Evaluate(m_Controller.stepCounter);

                float hRange = Mathf.Lerp(0f, m_HorizontalDistance, m_Weight);
                float vRange = Mathf.Lerp(0f, m_VerticalDistance, m_Weight);
                float rRange = Mathf.Lerp(0f, m_RollAngle, m_Weight);

                m_BobPosition = new Vector3(xCurve * hRange, yCurve * vRange, 0f);
                m_BobRotation = Quaternion.Euler(0f, 0f, rCurve * rRange);

                // Handle the aim compensation
                if (m_UseAimCompensation)
                {
                    float crosshairDistance = m_MaxDistance;

                    // Get the clamped crosshair distance
                    var ray = new Ray(m_AimTransform.position, m_AimTransform.forward);
                    var hit = new RaycastHit();
                    if (PhysicsExtensions.RaycastNonAllocSingle(ray, out hit, m_MaxDistance, m_AimLayers, m_AimTransform.root, QueryTriggerInteraction.Ignore))
                        crosshairDistance = Mathf.Clamp(hit.distance, m_MinDistance, m_MaxDistance);

                    // Damp with previous
                    m_CompensationDistance = Mathf.SmoothDamp(m_CompensationDistance, crosshairDistance, ref m_CompenstationSpeed, Mathf.Lerp(0.05f, 1f, m_Damping));

                    // Get the rotation to point in to center
                    Vector3 compensationTarget = -m_BobPosition;
                    compensationTarget.z = m_CompensationDistance;
                    m_BobRotation *= Quaternion.LookRotation(compensationTarget);
                }
            }
            else
            {
                m_BobPosition = Vector3.zero;
                m_BobRotation = Quaternion.identity;
            }
        }
    }
}
