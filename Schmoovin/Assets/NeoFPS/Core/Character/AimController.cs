using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoCC;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
	public abstract class AimController : MonoBehaviour, IAimController, INeoSerializableComponent
    {
        [SerializeField, NeoObjectInHierarchyField(false, required = true), Tooltip("The transform to yaw when aiming. This should be a parent of the pitch transform.")]
        private Transform m_YawTransform = null;

        [SerializeField, NeoObjectInHierarchyField(false), Tooltip("This optional transform detaches the character direction from the aim direction.")]
        private Transform m_AimYawTransform = null;

        [SerializeField, Range(0f, 1f), Tooltip("The time taken to turn the character to the aim-yaw direction (if Aim Yaw Transform is set). 0 = call LerpYawToAim() manually, 1 = instant.")]
        private float m_SteeringRate = 0.5f;

        [SerializeField, Tooltip("The transform to pitch when aiming. This should be a child of the yaw transform.")]
        private Transform m_PitchTransform = null;

        [Header("Constraints")]
        
        [SerializeField, Range(0f, 89f), Tooltip("The maximum pitch angle (in degrees) from horizontal the aimer can rotate.")]
        private float m_MaxPitch = 89f;

		[SerializeField, Range(0f, 1f), Tooltip("The amount of damping applied when rotating the camera to match constraints.")]
        private float m_ConstraintsDamping = 0.5f;

        [SerializeField, Tooltip("Once the angle outside constraints goes below this value, the camera will snap to the constraints. Larger values will have a visible effect.")]
        private float m_ConstraintsTolerance = 0.25f;

        [SerializeField, Tooltip("An angle range from the yaw constraint limits where the input falls off. This gives the effect of softer constraint limits instead of hitting an invisible wall.")]
        private float m_YawConstraintsFalloff = 10f;

        private const float k_MaxConstraintsMatchMult = 20f;
        private const float k_MinConstraintsMatchMult = 1f;

        private static readonly NeoSerializationKey k_TurnMultKey = new NeoSerializationKey("turnMult");
        private static readonly NeoSerializationKey k_ConstraintsMultKey = new NeoSerializationKey("constraintsMult");
        private static readonly NeoSerializationKey k_YawConstraintKey = new NeoSerializationKey("yawConstraint");
        private static readonly NeoSerializationKey k_YawLimitKey = new NeoSerializationKey("yawLimit");
        private static readonly NeoSerializationKey k_PitchKey = new NeoSerializationKey("pitch");

        private bool m_DisconnectAimFromYaw = false;
        private float m_ConstraintsMatchMult = 0f;
        private bool m_YawConstrained = false;
        private Vector3 m_YawHeadingConstraint = Vector3.zero;
        private Quaternion m_YawLocalRotation = Quaternion.identity;
        private float m_YawLimit = 0f;
        private float m_PendingYaw = 0f;
        private float m_CurrentPitch = 0f;
        private float m_PitchLimitMin = -89f;
        private float m_PitchLimitMax = 89f;
        private float m_PendingPitch = 0f;

        protected bool isValid
        {
            get;
            private set;
        }

        public Quaternion rotation
        {
            get { return m_PitchTransform.rotation; }
            set { m_PitchTransform.rotation = value; }
        }

        public Vector3 aimHeading
        {
            get { return m_AimYawTransform.forward; }
        }

        public Vector3 heading
        {
            get { return m_YawTransform.forward; }
        }

        public Vector3 forward
		{
			get { return m_PitchTransform.forward; }
        }

        public Vector3 yawUp
        {
            get { return m_YawTransform.up; }
        }

        public float pitch
        {
            get
            {
                return -(Mathf.Repeat(m_PitchTransform.eulerAngles.x + 180f, 360f) - 180f);
            }
        }

        public float aimYawDiff
        {
            get
            {
                if (m_DisconnectAimFromYaw)
                    return m_YawLocalRotation.eulerAngles.y;
                else
                    return 0f;
            }
        }

        public float constraintsSmoothing
        {
            get { return m_ConstraintsDamping; }
            set
            {
                m_ConstraintsDamping = Mathf.Clamp01(value);
                CalculateSmoothingMultiplier();
            }
        }

        public Quaternion yawLocalRotation
        {
            get
            {
                if (m_DisconnectAimFromYaw)
                    return m_YawTransform.localRotation;
                else
                    return m_YawLocalRotation;
            }
        }
        public Quaternion pitchLocalRotation
        {
            get;
            private set;
        }

        private float currentPitch
        {
            get { return m_CurrentPitch; }
            set
            {
                m_CurrentPitch = value;
                if (m_CurrentPitch > 180f)
                    m_CurrentPitch -= 360f;
            }
        }

        public float steeringRate
        {
            get { return m_SteeringRate; }
            set { m_SteeringRate = Mathf.Clamp01(value); }
        }

        private float m_TurnRateMultiplier = 1f;
		public float turnRateMultiplier
		{
			get { return m_TurnRateMultiplier; }
			set { m_TurnRateMultiplier = Mathf.Clamp01 (value); }
		}

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            m_MaxPitch = Mathf.Clamp(m_MaxPitch, 0f, 90f);
            m_ConstraintsTolerance = Mathf.Clamp(m_ConstraintsTolerance, 0f, 90f);
            m_YawConstraintsFalloff = Mathf.Clamp(m_YawConstraintsFalloff, 0f, 45f);
            CalculateSmoothingMultiplier();

            if (m_AimYawTransform == null)
            {
                if (m_YawTransform != null && !IsChildOf(m_PitchTransform, m_YawTransform))
                {
                    m_PitchTransform = null;
                    Debug.LogError("Pitch transform must be a child of the yaw transform.");
                }
            }
            else
            {
                if (m_YawTransform != null && !IsChildOf(m_AimYawTransform, m_YawTransform))
                {
                    m_AimYawTransform = null;
                    Debug.LogError("Aim-yaw transform transform must be a child of the yaw transform.");
                }
                if (m_AimYawTransform != null && !IsChildOf(m_PitchTransform, m_AimYawTransform))
                {
                    m_PitchTransform = null;
                    Debug.LogError("Pitch transform transform must be a child of the aim-yaw transform.");
                }
            }
        }

        bool IsChildOf(Transform c, Transform p)
        {
            Transform t = c.parent;
            while (t != null)
            {
                if (t == p)
                    return true;
                t = t.parent;
            }
            return false;
        }
#endif

        void CalculateSmoothingMultiplier ()
        {
            float lerp = 1f - m_ConstraintsDamping;
            lerp *= lerp;
            m_ConstraintsMatchMult = Mathf.Lerp(k_MinConstraintsMatchMult, k_MaxConstraintsMatchMult, lerp);
        }

        protected virtual void Awake ()
        {
            ResetYawConstraints();
            ResetPitchConstraints();

            if (m_YawTransform == null || m_PitchTransform == null)
            {
                isValid = false;
#if UNITY_EDITOR
                Debug.LogError("AimController has invalid yaw and pitch transforms. Pitch transform should be a child of the yaw transform.");
#endif
                m_YawLocalRotation = Quaternion.identity;
                pitchLocalRotation = Quaternion.identity;
            }
            else
            {
                m_YawLocalRotation = m_YawTransform.localRotation;
                pitchLocalRotation = m_PitchTransform.localRotation;
                isValid = true;
            }
        }

        protected virtual void Start ()
        {
            if (!isValid)
                return;

            if (m_AimYawTransform != null && m_AimYawTransform != m_YawTransform)
                m_DisconnectAimFromYaw = true;
            else
                m_AimYawTransform = m_YawTransform;

            currentPitch = m_PitchTransform.localEulerAngles.x;
            CalculateSmoothingMultiplier();
        }

		protected virtual void LateUpdate ()
		{
            if (!isValid)
                return;
            
			UpdateAimInput ();
            UpdateYaw();
            UpdatePitch();

            if (m_DisconnectAimFromYaw && m_SteeringRate > 0.0001f)
            {
                if (m_SteeringRate >= 0.999f)
                    LerpYawToAim(1f);
                else
                {
                    float lerp = Mathf.Lerp(0.0025f, 0.25f, m_SteeringRate);
                    LerpYawToAim(lerp);
                }
            }
        }

        void UpdateYaw()
        {
            if (m_YawConstrained)
            {
                Vector3 target = Vector3.ProjectOnPlane(m_YawHeadingConstraint, m_AimYawTransform.up).normalized;
                if (target != Vector3.zero)
                {
                    // Get the signed yaw angle from the constraint target
                    float angle = Vector3.SignedAngle(target, m_AimYawTransform.forward, m_AimYawTransform.up);

                    // Get the min and max turn
                    float minTurn = -m_YawLimit - angle;
                    float maxTurn = m_YawLimit - angle;

                    // Check if outside bounds
                    bool outsideBounds = false;
                    if (minTurn > 0f)
                    {
                        // Get damped rotation towards constraints
                        float y = minTurn;
                        if (minTurn > m_ConstraintsTolerance)
                            y *= Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

                        // Set pending yaw if above reaches constraints faster
                        if (m_PendingYaw < y)
                            m_PendingYaw = y;

                        // Prevent overshoot
                        if (m_PendingYaw > maxTurn)
                            m_PendingYaw = maxTurn;

                        // Falloff
                        //m_YawConstraintsFalloff

                        outsideBounds = true;
                    }
                    if (maxTurn < 0f)
                    {
                        // Get damped rotation towards constraints
                        float y = maxTurn;
                        if (maxTurn < -m_ConstraintsTolerance)
                            y *= Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

                        // Set pending yaw if above reaches constraints faster
                        if (m_PendingYaw > y)
                            m_PendingYaw = y;

                        // Prevent overshoot
                        if (m_PendingYaw < minTurn)
                            m_PendingYaw = minTurn;

                        outsideBounds = true;
                    }

                    if (!outsideBounds)
                    {
                        // Apply falloff
                        if (m_YawConstraintsFalloff > 0.0001f)
                        {
                            if (m_PendingYaw >= 0f)
                                m_PendingYaw *= Mathf.Clamp01(maxTurn / m_YawConstraintsFalloff);
                            else
                                m_PendingYaw *= Mathf.Clamp01(-minTurn / m_YawConstraintsFalloff);
                        }
                        
                        // Clamp the rotation
                        m_PendingYaw = Mathf.Clamp(m_PendingYaw, minTurn, maxTurn);
                    }
                }
            }

            // Apply yaw rotation
            m_YawLocalRotation *= Quaternion.Euler(0f, m_PendingYaw, 0f);
            m_AimYawTransform.localRotation = m_YawLocalRotation;

            // Reset pending yaw
            m_PendingYaw = 0f; 
        }

        void UpdatePitch()
        {
            // Check if outside bounds already
            bool outsideBounds = false;
            if (currentPitch > m_PitchLimitMax)
            {
                // Get damped rotation towards constraints
                float p = (m_PitchLimitMax - currentPitch) * Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

                // Set pending pitch if above reaches constraints faster
                if (m_PendingPitch > p)
                    m_PendingPitch = p;
                
                // Assign & prevent overshoot
                currentPitch += m_PendingPitch;
                if (currentPitch < m_PitchLimitMin)
                    currentPitch = m_PitchLimitMin;

                outsideBounds = true;
            }
            if (currentPitch < m_PitchLimitMin)
            {
                // Get damped rotation towards constraints
                float p = (m_PitchLimitMin - currentPitch) * Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

                // Set pending pitch if above reaches constraints faster
                if (m_PendingPitch < p)
                    m_PendingPitch = p;

                // Assign & prevent overshoot
                currentPitch += m_PendingPitch;
                if (currentPitch > m_PitchLimitMax)
                    currentPitch = m_PitchLimitMax;

                outsideBounds = true;
            }
            
            // Clamp the rotation
            if (!outsideBounds)
                currentPitch = Mathf.Clamp(currentPitch + m_PendingPitch, m_PitchLimitMin, m_PitchLimitMax);

            // Apply the pitch
            pitchLocalRotation = Quaternion.Euler(currentPitch, 0f, 0f);
            m_PitchTransform.localRotation = pitchLocalRotation;

            // Reset pending pitch
            m_PendingPitch = 0f;
        }

		public virtual void UpdateAimInput ()
		{
			// Override if using custom. Sample aimer uses events instead
        }

		public void AddYaw (float y)
        {
            if (enabled)
                m_PendingYaw += y * m_TurnRateMultiplier;
        }

        public void ResetYawLocal()
        {
            if (isValid)
            {
                if (!m_DisconnectAimFromYaw)
                    m_YawLocalRotation = Quaternion.identity;
                m_YawTransform.localRotation = Quaternion.identity;
            }
        }

        public void LerpYawToAim(float amount)
        {
            if (!m_DisconnectAimFromYaw || amount <= 0f)
                return;

            if (amount >= 1f)
            {
                m_YawTransform.localRotation *= m_YawLocalRotation;
                m_YawLocalRotation = Quaternion.identity;
                m_AimYawTransform.localRotation = m_YawLocalRotation;
            }
            else
            {
                var lerped = Quaternion.Lerp(Quaternion.identity, m_YawLocalRotation, amount);
                m_YawTransform.localRotation *= lerped;

                m_YawLocalRotation *= Quaternion.Inverse(lerped);
                m_AimYawTransform.localRotation = m_YawLocalRotation;
            }
        }

        public void AddPitch (float p)
        {
            if (enabled)
            m_PendingPitch += p * m_TurnRateMultiplier;
        }
       
        public void ResetPitchLocal()
        {
            if (isValid)
            {
                currentPitch = Mathf.Clamp(0f, m_PitchLimitMin, m_PitchLimitMax);
                pitchLocalRotation = Quaternion.Euler(-currentPitch, 0f, 0f);
                m_PitchTransform.localRotation = pitchLocalRotation;
            }
        }

        public void AddRotation (float y, float p)
        {
            AddYaw(y);
            AddPitch(p);
        }

        public void AddRotationInput(Vector2 input, Transform relativeTo)
        {
            if (enabled)
            {
                // Get the corrected rotation
                Quaternion inputRotation = relativeTo.localRotation * Quaternion.Euler(input.y, input.x, 0f);
                Vector3 euler = inputRotation.eulerAngles;

                // Get the modified pitch & yaw (wrapped)
                float modifiedYaw = euler.y;
                if (modifiedYaw > 180f)
                    modifiedYaw -= 360f;
                float modifiedPitch = euler.x;
                if (modifiedPitch > 180f)
                    modifiedPitch -= 360f;

                // Get the vertical amount of the aimer (tilt has less effect closer to the vertical)
                float vertical = Mathf.Abs(Vector3.Dot(m_PitchTransform.forward, m_YawTransform.up));

                // Lerp between modified rotation and standard as it gets closer to vertical
                AddYaw(Mathf.Lerp(modifiedYaw, input.x, vertical));
                AddPitch(Mathf.Lerp(modifiedPitch, input.y, vertical));
            }
        }

        public void SetYawConstraints(Vector3 center, float range)
        {
            if (range >= 360f)
            {
                ResetYawConstraints();
                return;
            }

            // Clamp the yaw limit
            m_YawLimit = Mathf.Clamp(range * 0.5f, 0f, 180f);

            m_YawHeadingConstraint = center;

            m_YawConstrained = true;
        }

        public void SetPitchConstraints(float min, float max)
        {
            m_PitchLimitMin = Mathf.Clamp(-max, -m_MaxPitch, m_MaxPitch);
            m_PitchLimitMax = Mathf.Clamp(-min, -m_MaxPitch, m_MaxPitch);
        }

        public void ResetYawConstraints()
        {
            m_YawConstrained = false;
        }

        public void ResetPitchConstraints()
        {
            m_PitchLimitMin = -m_MaxPitch;
            m_PitchLimitMax = m_MaxPitch;
        }
        
        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            // Write multipliers
            writer.WriteValue(k_TurnMultKey, m_TurnRateMultiplier);
            writer.WriteValue(k_ConstraintsMultKey, m_ConstraintsMatchMult);

            // Write yaw constraints
            if (m_YawConstrained)
            {
                writer.WriteValue(k_YawConstraintKey, m_YawHeadingConstraint);
                writer.WriteValue(k_YawLimitKey, m_YawLimit);
            }

            // Write pitch constraints
            Vector3 pitchValues = new Vector3(
                currentPitch,
                m_PitchLimitMin,
                m_PitchLimitMax
                );
            writer.WriteValue(k_PitchKey, pitchValues);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            // Read multipliers
            reader.TryReadValue(k_TurnMultKey, out m_TurnRateMultiplier, m_TurnRateMultiplier);
            reader.TryReadValue(k_ConstraintsMultKey, out m_ConstraintsMatchMult, m_ConstraintsMatchMult);

            // Read yaw constraints
            if (reader.TryReadValue(k_YawConstraintKey, out m_YawHeadingConstraint, m_YawHeadingConstraint))
            {
                m_YawConstrained = true;
                reader.TryReadValue(k_YawLimitKey, out m_YawLimit, m_YawLimit);
                m_PendingYaw = 0f;
            }
            else
                m_YawConstrained = false;

            // Read pitch constraints
            Vector3 pitchValues;
            if (reader.TryReadValue(k_PitchKey, out pitchValues, Vector3.zero))
            {
                currentPitch = pitchValues.x;
                m_PitchLimitMin = pitchValues.y;
                m_PitchLimitMax = pitchValues.z;
                m_PendingPitch = 0f;

                // Apply the pitch
                pitchLocalRotation = Quaternion.Euler(currentPitch, 0f, 0f);
                m_PitchTransform.localRotation = pitchLocalRotation;
            }
        }
    }
}