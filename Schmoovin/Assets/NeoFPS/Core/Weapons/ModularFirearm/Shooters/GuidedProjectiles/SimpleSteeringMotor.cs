using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-simplesteeringmotor.html")]
    public class SimpleSteeringMotor : MonoBehaviour, IGuidedProjectileMotor, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The turn rate of the projectile.")]
        private float m_BaseTurnRate = 30f;

        [Range(0f, 100f), SerializeField, Tooltip("An increase to the turn rate based on elapsed time in flight (base turn rate + turn grow rate * elapsed time).")]
        private float m_TurnGrowRate = 50f;

        private Transform m_LocalTransform = null;
        private float m_Speed = 0f;
        private float m_ElapsedTime = 0f;

        void OnValidate()
        {
            m_BaseTurnRate = Mathf.Clamp(m_BaseTurnRate, 1f, 1080f);
        }

        void Awake()
        {
            m_LocalTransform = transform;
        }

        public void SetStartingVelocity(Vector3 v)
        {
            m_ElapsedTime = 0f;
            m_Speed = v.magnitude;
        }

        public Vector3 GetVelocity(Vector3 currentPosition)
        {
            m_ElapsedTime += Time.deltaTime;
            return m_LocalTransform.forward * m_Speed;
        }

        public Vector3 GetVelocity(Vector3 currentPosition, Vector3 targetPosition)
        {
            var forwards = m_LocalTransform.forward;
            var targetDirection = (targetPosition - currentPosition).normalized;

            // Steer
            m_ElapsedTime += Time.deltaTime;
            forwards = Vector3.RotateTowards(forwards, targetDirection, (m_BaseTurnRate + m_ElapsedTime * m_TurnGrowRate) * Time.deltaTime * Mathf.Deg2Rad, 0f);

            // Apply rotation
            //m_LocalTransform.rotation = Quaternion.LookRotation(forwards);

            // Get velocity
            return forwards * m_Speed;
        }

        public void OnTeleport(Vector3 position, Quaternion rotation, bool relativeRotation = true)
        {
            // Nothing required
        }

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_SpeedKey = new NeoSerializationKey("speed");
        private static readonly NeoSerializationKey k_ElapsedKey = new NeoSerializationKey("elapsed");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_SpeedKey, m_Speed);
            writer.WriteValue(k_ElapsedKey, m_ElapsedTime);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_SpeedKey, out m_Speed, m_Speed);
            reader.TryReadValue(k_ElapsedKey, out m_ElapsedTime, m_ElapsedTime);
        }

        #endregion
    }
}