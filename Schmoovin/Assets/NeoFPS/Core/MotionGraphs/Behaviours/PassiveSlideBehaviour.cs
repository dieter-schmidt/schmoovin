using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/PassiveSlide", "PassiveSlideBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-passiveslidebehaviour.html")]
    public class PassiveSlideBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("The angle at which a character will start sliding if no input is detected.")]
        private FloatDataReference m_SlideAngle = new FloatDataReference(30f);

        [SerializeField, Range(0f, 1f), Tooltip("The slope friction to apply when sliding.")]
        private float m_SlideFriction = 0.5f;

        private static readonly NeoSerializationKey k_SlideKey = new NeoSerializationKey("slide");
        private static readonly NeoSerializationKey k_EntryFrictionKey = new NeoSerializationKey("entryFriction");
        private const float k_SlideMoveDistance = 0.01f;

        private float m_SlideAmount = 0f;
        private float m_EntryFriction = 0f;
        
        public float slideAmount
        {
            get { return m_SlideAmount; }
            private set
            {
                if (m_SlideAmount != value && m_SlideFriction < m_EntryFriction)
                {
                    m_SlideAmount = value;
                    controller.characterController.slopeFriction = Mathf.Lerp(m_EntryFriction, m_SlideFriction, m_SlideAmount);
                }
            }
        }

        public override void OnEnter()
        {
            m_EntryFriction = controller.characterController.slopeFriction;
            slideAmount = 0f;
        }

        public override void OnExit()
        {
            controller.characterController.slopeFriction = m_EntryFriction;
            m_SlideAmount = -1f;
        }

        public override void Update()
        {
            if (controller.inputMoveScale > k_SlideMoveDistance)
                slideAmount = 0f;
            else
            {
                float angle = Vector3.Angle(controller.characterController.groundSurfaceNormal, controller.characterController.up);
                if (angle > m_SlideAngle.value)
                    slideAmount = 1f;
                else
                    slideAmount = 0f;
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            base.CheckReferences(map);
            m_SlideAngle.CheckReference(map);
        }

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_SlideKey, m_SlideAmount);
            writer.WriteValue(k_EntryFrictionKey, m_EntryFriction);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_SlideKey, out m_SlideAmount, m_SlideAmount);
            reader.TryReadValue(k_EntryFrictionKey, out m_EntryFriction, m_EntryFriction);
        }
    }
}