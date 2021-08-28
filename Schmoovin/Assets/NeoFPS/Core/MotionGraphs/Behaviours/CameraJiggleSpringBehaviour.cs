using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS
{
    [MotionGraphElement("Camera/CameraJiggleSpringBehaviour", "CameraJiggleSpringBehaviour")]
    public class CameraJiggleSpringBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("An optional switch condition that defines if the kick should be triggered.")]
        private SwitchParameter m_SwitchCondition = null;
        [SerializeField, Tooltip("An optional trigger condition that defines if the kick should be triggered.")]
        private TriggerParameter m_TriggerCondition = null;
        [SerializeField, Tooltip("When should the camera jiggle spring be triggered.")]
        private When m_When = When.OnEnter;
        [SerializeField, Range(-1f, 1f), Tooltip("The strength of the jiggle effect (max angle is set in the Additive Jiggle component on the camera spring transform).")]
        private float m_JiggleStrength = 1f;
        [SerializeField, Tooltip("Should the CW/CCW direction of the jiggle be chosen at random each time.")]
        private bool m_RandomiseDirection = true;

        private AdditiveJiggle m_Jiggle = null;

        enum When
        {
            OnEnter,
            OnExit,
            Both
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            var character = controller.GetComponent<ICharacter>();
            if (character != null)
                m_Jiggle = character.headTransformHandler.GetComponent<AdditiveJiggle>();
        }

        void Jiggle()
        {
            if (m_Jiggle != null)
            {
                bool jiggle = true;
                if (m_SwitchCondition != null)
                    jiggle &= m_SwitchCondition.on;
                if (m_TriggerCondition != null)
                    jiggle &= m_TriggerCondition.CheckTrigger();

                if (jiggle)
                {
                    if (m_RandomiseDirection)
                        m_Jiggle.Jiggle(m_JiggleStrength * Mathf.Sign(Random.value));
                    else
                        m_Jiggle.Jiggle(m_JiggleStrength);
                }
            }
        }

        public override void OnEnter()
        {
            if (m_When != When.OnExit)
                Jiggle();
        }

        public override void OnExit()
        {
            if (m_When != When.OnEnter)
                Jiggle();
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_SwitchCondition = map.Swap(m_SwitchCondition);
            m_TriggerCondition = map.Swap(m_TriggerCondition);
        }
    }
}