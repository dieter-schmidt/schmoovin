using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Audio/LadderAudio", "LadderAudioBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-ladderaudiobehaviour.html")]
    public class LadderAudioBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The surface audio library for the slide audio clips.")]
		private SurfaceAudioData m_AudioData = null;

        [SerializeField, Tooltip("The transform property holding the ladder transform.")]
        private TransformParameter m_LadderTransform = null;

        [SerializeField, Tooltip("How many rungs apart to play a sound. This is based on the ladder spacing property.")]
        private float m_SpacingMultiplier = 3f;

		[SerializeField, Tooltip("The speed below which no audio will be played.")]
		private float m_MinimumSpeed = 0.01f;

        private static readonly NeoSerializationKey k_CycleKey = new NeoSerializationKey("cycle");

        private ICharacterAudioHandler m_AudioHandler = null;
        private ILadder m_Ladder = null;
        private float m_StepCycle = 0f;
        private FpsSurfaceMaterial m_Surface = FpsSurfaceMaterial.Default;

        public override void OnValidate()
        {
			// Check spacing multiplier
			m_SpacingMultiplier = Mathf.Clamp (m_SpacingMultiplier, 0.1f, 10f);
			// Check minimum speed
			m_MinimumSpeed = Mathf.Clamp (m_MinimumSpeed, 0f, 10f);
        }

        public override void OnEnter()
        {
            if (m_LadderTransform.value != null)
            {
                Transform lt = m_LadderTransform.value;

                // Get the ladder info
                m_Ladder = lt.GetComponent<ILadder>();

                // Get the ladder surface type
				BaseSurface s = lt.GetComponent<BaseSurface> ();
				if (s != null)
					m_Surface = s.GetSurface ();
                else
                    m_Surface = 0;
            }

            // Get audio handler
            if (m_AudioHandler == null)
                m_AudioHandler = controller.localTransform.GetComponent<ICharacterAudioHandler>();

            m_StepCycle = 0f;
        }

        public override void OnExit()
        {
            m_Ladder = null;
        }

        public override void Update()
        {
            if (m_Ladder == null || m_AudioHandler == null || m_AudioData == null)
                return;

            float speed = Mathf.Abs(Vector3.Dot(controller.characterController.velocity, m_Ladder.up));
            if (speed > m_MinimumSpeed)
            {
                m_StepCycle += (speed * Time.deltaTime) / (m_Ladder.spacing * m_SpacingMultiplier);
                if (m_StepCycle > 1f)
                {
                    m_StepCycle -= 1f;
                    PlayAudio (m_Surface);
                }
            }
        }

        void PlayAudio(FpsSurfaceMaterial surface)
        {
            float volume = 1f;
            AudioClip clip = m_AudioData.GetAudioClip(surface, out volume);
            if (clip != null)
                m_AudioHandler.PlayClip(clip, FpsCharacterAudioSource.Body, volume);
        }
        
        public override void CheckReferences(IMotionGraphMap map)
        {
            m_LadderTransform = map.Swap(m_LadderTransform);
        }

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_CycleKey, m_StepCycle);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_CycleKey, out m_StepCycle, m_StepCycle);
        }
    }
}