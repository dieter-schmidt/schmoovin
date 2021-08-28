using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-drunkmissilemotor.html")]
    public class DrunkMissileMotor : MonoBehaviour, IGuidedProjectileMotor, INeoSerializableComponent
    {
        [Header ("Turn Rate")]
        [SerializeField, Tooltip("The turn rate of the projectile.")]
        private float m_TurnRate = 90f;

        [Range(0f, 100f), SerializeField, Tooltip("An increase to the turn rate based on elapsed time in flight (base turn rate + turn grow rate * elapsed time).")]
        private float m_TurnGrowRate = 50f;

        [Header ("Tracking")]

        [SerializeField, Tooltip("The minimum amount of time between tracking ticks (when the projectile updates its target location).")]
        private float m_MinInterval = 0.1f;

        [SerializeField, Tooltip("The maximum amount of time between tracking ticks (when the projectile updates its target location).")]
        private float m_MaxInterval = 0.25f;

        [SerializeField, Tooltip("The maximum angle off the forwards direction that the tracker can swivel to look at the target each tick.")]
        private float m_MaxTrackingAngle = 90f;

        [Header("Deviation")]

        [SerializeField, Tooltip("The maximum deviation from the tracker angle that the projectile will steer to add the drunken effect")]
        private float m_MaxDeviation = 90f;

        [SerializeField, Tooltip("The distance from the target below which the projectile will switch to a more consistent tracking (reduces projectiles overshooting).")]
        private float m_AccurateDistance = 10f;

        [Header("Speed")]

        [SerializeField, Tooltip("Should the projectile's speed vary with occasional boosts")]
        private bool m_VarySpeed = true;

        [SerializeField, Tooltip("The target boost speed (added to the base speed of the projectile when fired).")]
        private float m_SpeedBoost = 10f;

        [SerializeField, Tooltip("The maximum amount of time a boost will last.")]
        private float m_MaxBoostTime = 0.25f;

        [SerializeField, Tooltip("The maximum amount of time between boosts.")]
        private float m_MaxSlowTime = 1.5f;

        [SerializeField, Range(0f, 1f), Tooltip("The damping when accelerating up to boost speed.")]
        private float m_AccelerationDamping = 0.1f;

        [SerializeField, Range(0f, 1f), Tooltip("The damping when decelerating from boost back down to base speed.")]
        private float m_DecelerationDamping = 0.75f;

        private Transform m_LocalTransform = null;
        private Vector3 m_LastTickPosition = Vector3.zero;
        private Vector3 m_LastTickDirection = Vector3.zero;
        private Vector3 m_CurrentSteerDirection = Vector3.zero;
        private float m_BaseSpeed = 0f;
        private float m_Speed = 0f;
        private float m_SteerTimer = 0f;
        private float m_ElapsedTime = 0f;
        private bool m_Fast = false;
        private float m_SpeedSwitchTimer = 0f;

        void Awake()
        {
            m_LocalTransform = transform;
        }

        void OnValidate()
        {
            m_TurnRate = Mathf.Clamp(m_TurnRate, 1f, 1080f);
            m_MinInterval = Mathf.Clamp(m_MinInterval, 0f, 10f);
            m_MaxInterval = Mathf.Clamp(m_MaxInterval, 0.1f, 10f);
            m_MaxTrackingAngle = Mathf.Clamp(m_MaxTrackingAngle, 0f, 179f);
            m_MaxDeviation = Mathf.Clamp(m_MaxDeviation, 0f, 180f);
            m_AccurateDistance = Mathf.Clamp(m_AccurateDistance, 0f, 100f);
            m_SpeedBoost = Mathf.Clamp(m_SpeedBoost, 0f, 100f);
            m_MaxBoostTime = Mathf.Clamp(m_MaxBoostTime, 0.1f, 10f);
            m_MaxSlowTime = Mathf.Clamp(m_MaxSlowTime, 0.1f, 10f);
        }

        public void SetStartingVelocity(Vector3 v)
        {
            // Reset steer timer
            m_SteerTimer = Random.Range(m_MinInterval, m_MaxInterval);
            m_BaseSpeed = m_Speed = v.magnitude;
            m_SpeedSwitchTimer = Random.Range(1, m_MaxSlowTime);
            m_Fast = false;
            m_CurrentSteerDirection = v.normalized;
            m_ElapsedTime = 0f;

            m_LastTickPosition = m_LocalTransform.position;
            m_LastTickDirection = m_CurrentSteerDirection;
        }
        
        public Vector3 GetVelocity(Vector3 currentPosition)
        {
            var forwards = m_LocalTransform.forward;
            var steerRate = m_TurnRate;
            
            m_SteerTimer -= Time.deltaTime;
            if (m_SteerTimer < 0f)
            {
                var v = currentPosition - m_LastTickPosition;
                var d = Vector3.Dot(v, m_LastTickDirection);
                var deltaPos = m_LastTickPosition - currentPosition + m_LastTickDirection * (25f * d);

                // Reset steer timer
                m_SteerTimer = Random.Range(m_MinInterval, m_MaxInterval);

                // turn tracked towards target
                var targetDirection = deltaPos.normalized;
                var center = Vector3.RotateTowards(forwards, targetDirection, m_MaxTrackingAngle * Mathf.Deg2Rad, 0f);

                // Get random direction within tracked cone
                Quaternion randomRot = Random.rotationUniform;
                m_CurrentSteerDirection = Quaternion.Slerp(Quaternion.identity, randomRot, m_MaxDeviation / 360f) * center;
            }

            return GetVelocityBasedOnSteering(forwards, steerRate);
        }

        public Vector3 GetVelocity(Vector3 currentPosition, Vector3 targetPosition)
        {
            var forwards = m_LocalTransform.forward;
            var deltaPos = targetPosition - currentPosition;
            var steerRate = m_TurnRate;

            if (deltaPos.magnitude > m_AccurateDistance)
            {
                m_SteerTimer -= Time.deltaTime;
                if (m_SteerTimer < 0f)
                {
                    // Reset steer timer
                    m_SteerTimer = Random.Range(m_MinInterval, m_MaxInterval);

                    // turn tracked towards target
                    var targetDirection = deltaPos.normalized;
                    var center = Vector3.RotateTowards(forwards, targetDirection, m_MaxTrackingAngle * Mathf.Deg2Rad, 0f);

                    // Get random direction within tracked cone
                    Quaternion randomRot = Random.rotationUniform;
                    m_CurrentSteerDirection = Quaternion.Slerp(Quaternion.identity, randomRot, m_MaxDeviation / 360f) * center;

                    // Record tick position and direction in case target is lost
                    m_LastTickPosition = currentPosition;
                    m_LastTickDirection = m_CurrentSteerDirection;
                }
            }
            else
            {
                m_ElapsedTime += Time.deltaTime;
                steerRate += m_ElapsedTime * m_TurnGrowRate;
                m_CurrentSteerDirection = (targetPosition - currentPosition).normalized;
            }

            return GetVelocityBasedOnSteering(forwards, steerRate);
        }

        Vector3 GetVelocityBasedOnSteering(Vector3 forwards, float steerRate)
        {
            // Steer
            forwards = Vector3.RotateTowards(forwards, m_CurrentSteerDirection, steerRate * Time.deltaTime * Mathf.Deg2Rad, 0f);

            // Hnadle speed / acceleration
            if (m_VarySpeed)
            {
                // Check speed change
                m_SpeedSwitchTimer -= Time.deltaTime;
                if (m_SpeedSwitchTimer < 0f)
                {
                    m_Fast = !m_Fast;
                    if (m_Fast)
                        m_SpeedSwitchTimer = Random.Range(0.1f, m_MaxBoostTime);
                    else
                        m_SpeedSwitchTimer = Random.Range(0.1f, m_MaxSlowTime);
                }

                if (m_Fast)
                {
                    float lerp = Mathf.Lerp(0.5f, 0.01f, m_DecelerationDamping);
                    m_Speed = Mathf.Lerp(m_Speed, m_BaseSpeed, lerp);
                }
                else
                {
                    float lerp = Mathf.Lerp(0.5f, 0.01f, m_AccelerationDamping);
                    m_Speed = Mathf.Lerp(m_Speed, m_BaseSpeed + m_SpeedBoost, lerp);
                }
            }

            // Get velocity
            return forwards * m_Speed;
        }

        public void OnTeleport(Vector3 position, Quaternion rotation, bool relativeRotation = true)
        {
            // Update the rotation and velocity direction
            if (relativeRotation)
                m_CurrentSteerDirection = rotation * m_CurrentSteerDirection;
            else
            {
                var inverse = Quaternion.Inverse(m_LocalTransform.rotation);
                m_CurrentSteerDirection = inverse * rotation * m_CurrentSteerDirection;
            }
        }

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_SteerDirectionKey = new NeoSerializationKey("direction");
        private static readonly NeoSerializationKey k_BaseSpeedKey = new NeoSerializationKey("baseSpeed");
        private static readonly NeoSerializationKey k_CurrentSpeedKey = new NeoSerializationKey("speed");
        private static readonly NeoSerializationKey k_SteerTimerKey = new NeoSerializationKey("steerTimer");
        private static readonly NeoSerializationKey k_SpeedTimerKey = new NeoSerializationKey("speedTimer");
        private static readonly NeoSerializationKey k_ElapsedKey = new NeoSerializationKey("elapsed");
        private static readonly NeoSerializationKey k_FastKey = new NeoSerializationKey("fast");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_SteerDirectionKey, m_CurrentSteerDirection);
            writer.WriteValue(k_SteerTimerKey, m_SteerTimer);
            writer.WriteValue(k_ElapsedKey, m_ElapsedTime);
            writer.WriteValue(k_BaseSpeedKey, m_BaseSpeed);

            if (m_VarySpeed)
            {
                writer.WriteValue(k_CurrentSpeedKey, m_Speed);
                writer.WriteValue(k_FastKey, m_Fast);
                writer.WriteValue(k_SpeedTimerKey, m_SpeedSwitchTimer);
            }
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_SteerDirectionKey, out m_CurrentSteerDirection, m_CurrentSteerDirection);
            reader.TryReadValue(k_SteerTimerKey, out m_SteerTimer, m_SteerTimer);
            reader.TryReadValue(k_ElapsedKey, out m_ElapsedTime, m_ElapsedTime);
            reader.TryReadValue(k_BaseSpeedKey, out m_BaseSpeed, m_BaseSpeed);

            if (m_VarySpeed)
            {
                reader.TryReadValue(k_CurrentSpeedKey, out m_Speed, m_BaseSpeed);
                reader.TryReadValue(k_FastKey, out m_Fast, m_Fast);
                reader.TryReadValue(k_SpeedTimerKey, out m_SpeedSwitchTimer, m_SpeedSwitchTimer);
            }
            else
                m_Speed = m_BaseSpeed;
        }

        #endregion
    }
}