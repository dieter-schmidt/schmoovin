using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Audio/PlayAudioClip", "PlayAudioClipBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-playaudioclipbehaviour.html")]
    public class PlayAudioClipBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The audio clip to play.")]
        private AudioClip m_Clip = null;

        [SerializeField, Range(0,1), Tooltip("The volume to play the clip at.")]
        private float m_Volume = 1f;

        [SerializeField, Tooltip("The offset from the character controller transform position to play the clip at.")]
        private Vector3 m_Where = Vector3.zero;

        [SerializeField, Tooltip("When should the clip be played. Options are **EnterAndExit**, **EnterOnly**, **ExitOnly**.")]
        private When m_When = When.OnEnter;

        public enum When
        {
            OnEnter,
            OnExit,
            Both
        }

        public override void OnEnter()
        {
            if (m_Clip != null && m_When != When.OnExit)
            {
                Vector3 position = controller.localTransform.position;
                position += controller.localTransform.rotation * m_Where;
                AudioSource.PlayClipAtPoint(m_Clip, position, m_Volume);
            }
        }

        public override void OnExit()
        {
            if (m_Clip != null && m_When != When.OnEnter)
            {
                Vector3 position = controller.localTransform.position;
                position += controller.localTransform.rotation * m_Where;
                AudioSource.PlayClipAtPoint(m_Clip, position, m_Volume);
            }
        }
    }
}