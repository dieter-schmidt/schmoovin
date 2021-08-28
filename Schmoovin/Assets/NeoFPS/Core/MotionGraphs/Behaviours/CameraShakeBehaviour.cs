using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS
{
    [MotionGraphElement("Camera/CameraShakeBehaviour", "CameraShakeBehaviour")]
    public class CameraShakeBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("An optional float parameter to multiply the shake value by. This allows for increasing shake while falling, etc.")]
        private FloatParameter m_ShakeMultiplier = null;

        [SerializeField, Range(0f, 1f), Tooltip("The strength of the jiggle effect (max angle is set in the Additive Jiggle component on the camera spring transform).")]
        private float m_ShakeStrength = 0.25f;

        private CameraShake m_Shake = null;

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            var character = controller.GetComponent<ICharacter>();
            if (character != null)
                m_Shake = character.headTransformHandler.GetComponent<CameraShake>();
        }

        public override void OnEnter()
        {
            if (m_Shake != null)
            {
                if (m_ShakeMultiplier != null)
                    m_Shake.continuousShake = m_ShakeStrength * m_ShakeMultiplier.value;
                else
                    m_Shake.continuousShake = m_ShakeStrength;
            }
        }

        public override void Update()
        {
            if (m_ShakeMultiplier != null && m_Shake != null)
                m_Shake.continuousShake = m_ShakeStrength * m_ShakeMultiplier.value;
        }

        public override void OnExit()
        {
            if (m_Shake != null)
                m_Shake.continuousShake = 0f;
        }
    }
}