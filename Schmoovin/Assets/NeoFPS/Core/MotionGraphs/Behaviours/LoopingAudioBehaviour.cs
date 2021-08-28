using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Audio/LoopingAudio", "LoopingAudioBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-loopingaudiobehaviour.html")]
    public class LoopingAudioBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The looping audio clip to play.")]
		private AudioClip m_Clip = null;

        [SerializeField, Tooltip("The source ID to play from ([generated constant][1]).")]
        private FpsCharacterAudioSource m_Source = FpsCharacterAudioSource.Head;

        [SerializeField, Range(0f, 1f), Tooltip("The volume of the loop.")]
        private float m_Volume = 1f;

        [SerializeField, Range (0f, 2f), Tooltip("The pitch of the loop.")]
		private float m_Pitch = 1f;

        private ICharacterAudioHandler m_AudioHandler = null;

        public override void OnEnter()
        {
            // Get audio handler
            if (m_AudioHandler == null)
                m_AudioHandler = controller.localTransform.GetComponent<ICharacterAudioHandler>();

            // Start the loop
            if (m_AudioHandler != null)
                m_AudioHandler.StartLoop(m_Clip, m_Source, m_Volume, m_Pitch);
        }

        public override void OnExit()
        {
            if (m_AudioHandler != null)
                m_AudioHandler.StopLoop(m_Source);
        }
    }
}