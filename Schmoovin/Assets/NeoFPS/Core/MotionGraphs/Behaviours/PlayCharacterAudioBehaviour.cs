using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Audio/PlayCharacterAudio", "PlayCharacterAudioBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-playcharacteraudiobehaviour.html")]
    public class PlayCharacterAudioBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The audio ID to play (generated constant).")]
        private FpsCharacterAudio m_Audio = FpsCharacterAudio.Undefined;

        [SerializeField, Tooltip("When should the target height be set.")]
        private When m_When = When.OnEnter;

        private ICharacterAudioHandler m_AudioHandler = null;

        public enum When
        {
            OnEnter,
            OnExit,
            Both
        }

        public override void OnEnter()
        {
            // Get audio handler
            if (m_AudioHandler == null)
                m_AudioHandler = controller.localTransform.GetComponent<ICharacterAudioHandler>();

            // Play the clip
            if (m_AudioHandler != null && m_When != When.OnExit)
                m_AudioHandler.PlayAudio(m_Audio);
        }

        public override void OnExit()
        {
            // Play the clip
            if (m_AudioHandler != null && m_When != When.OnEnter)
                m_AudioHandler.PlayAudio(m_Audio);
        }
    }
}