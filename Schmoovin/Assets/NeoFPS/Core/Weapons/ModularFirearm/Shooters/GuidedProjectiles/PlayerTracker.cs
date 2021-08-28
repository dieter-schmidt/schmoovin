using NeoCC;
using NeoFPS.SinglePlayer;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-playertracker.html")]
    public class PlayerTracker : MonoBehaviour, IGuidedProjectileTargetTracker, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The time from firing to starting to steer towards the player character.")]
        private float m_TrackingDelay = 5f;

        private INeoCharacterController m_Controller = null;
        private Transform m_ControllerTransform = null;
        private float m_Timer = 0f;

        void OnEnable ()
        {
            FpsSoloCharacter.onLocalPlayerCharacterChange += OnPlayerCharacterChanged;
            m_Timer = m_TrackingDelay;

            // Lock onto existing player
            if (FpsSoloCharacter.localPlayerCharacter != null)
            {
                m_Controller = FpsSoloCharacter.localPlayerCharacter.motionController.characterController;
                m_ControllerTransform = m_Controller.transform;
            }
            else
            {
                m_Controller = null;
                m_ControllerTransform = null;
            }
        }

        void OnDisable()
        {
            FpsSoloCharacter.onLocalPlayerCharacterChange -= OnPlayerCharacterChanged;
            OnPlayerCharacterChanged(null);
        }

        void OnPlayerCharacterChanged(FpsSoloCharacter character)
        {
            m_Controller = null;
            m_ControllerTransform = null;
        }

        public bool GetTargetPosition(out Vector3 targetPosition)
        {
            m_Timer -= Time.deltaTime;
            if (m_Timer < 0f && m_Controller != null)
            {
                targetPosition = m_ControllerTransform.position + m_Controller.up * (m_Controller.height * 0.5f);
                return true;
            }
            else
            {
                targetPosition = Vector3.zero;
                return false;
            }
        }

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_TimerKey = new NeoSerializationKey("timer");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_TimerKey, m_Timer);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_TimerKey, out m_Timer, m_Timer);
        }

        #endregion
    }
}
