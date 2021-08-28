using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS
{
    [MotionGraphElement("Camera/CameraKickSpringBehaviour", "CameraKickSpringBehaviour")]
    public class CameraKickSpringBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("An optional switch condition that defines if the kick should be triggered.")]
        private SwitchParameter m_SwitchCondition = null;
        [SerializeField, Tooltip("An optional trigger condition that defines if the kick should be triggered.")]
        private TriggerParameter m_TriggerCondition = null;
        [SerializeField, Tooltip("When should the camera kick spring be triggered.")]
        private When m_When = When.OnEnter;
        [SerializeField, Tooltip("The position offset for the camera at the strongest point of the kick. Keep these values small (cm, not meters) or you risk clipping scenery or weapon geometry.")]
        private Vector3 m_PositionKick = Vector3.zero;
        [SerializeField, Tooltip("The rotation offset for the camera at the strongest point of the kick. Positive X nods forwards. Positive Y turns right. Positive Z tilts counter-clockwise.")]
        private Vector3 m_RotationKick = Vector3.zero;
        [SerializeField, Tooltip("The amount of time the kick effect should last.")]
        private float m_KickDuration = 0.5f;

        private AdditiveKicker m_Kicker = null;

        enum When
        {
            OnEnter,
            OnExit,
            Both
        }

        public override void OnValidate()
        {
            m_KickDuration = Mathf.Clamp(m_KickDuration, 0.1f, 10f);
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            var character = controller.GetComponent<ICharacter>();
            if (character != null)
                m_Kicker = character.headTransformHandler.GetComponent<AdditiveKicker>();
        }

        void Kick()
        {
            if (m_Kicker != null)
            {
                bool kick = true;
                if (m_SwitchCondition != null)
                    kick &= m_SwitchCondition.on;
                if (m_TriggerCondition != null)
                    kick &= m_TriggerCondition.CheckTrigger();

                if (kick)
                {
                    m_Kicker.KickPosition(m_PositionKick, m_KickDuration);
                    m_Kicker.KickRotation(Quaternion.Euler(m_RotationKick), m_KickDuration);
                }
            }
        }

        public override void OnEnter()
        {
            if (m_When != When.OnExit)
                Kick();
        }

        public override void OnExit()
        {
            if (m_When != When.OnEnter)
                Kick();
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_SwitchCondition = map.Swap(m_SwitchCondition);
            m_TriggerCondition = map.Swap(m_TriggerCondition);
        }
    }
}