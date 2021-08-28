using UnityEngine;

namespace NeoFPS.WieldableTools
{
    public class PlayAudioToolAction : BaseWieldableToolModule
    {
        [SerializeField, FlagsEnum, Tooltip("When should the clip be played.")]
        private WieldableToolOneShotTiming m_Timing = WieldableToolOneShotTiming.Start;
        [SerializeField, Tooltip("The clip to play")]
        private AudioClip m_Clip = null;
        [SerializeField, Range(0f, 1f), Tooltip("The volume for the clip.")]
        private float m_Volume = 1f;

        public override bool isValid
        {
            get { return m_Timing != 0 && m_Clip != null; }
        }

        public override WieldableToolActionTiming timing
        {
            get { return (WieldableToolActionTiming)m_Timing; }
        }

        public override void Initialise(IWieldableTool t)
        {
            base.Initialise(t);

            if (m_Clip == null || m_Timing == 0)
                enabled = false;
        }

        public override void FireStart()
        {
            if (tool.wielder.audioHandler != null)
                tool.wielder.audioHandler.PlayClip(m_Clip, m_Volume);
            else
                AudioSource.PlayClipAtPoint(m_Clip, transform.position, m_Volume);
        }

        public override void FireEnd(bool success)
        {
            if (success)
            {
                if (tool.wielder.audioHandler != null)
                    tool.wielder.audioHandler.PlayClip(m_Clip, m_Volume);
                else
                    AudioSource.PlayClipAtPoint(m_Clip, transform.position, m_Volume);
            }
        }

        public override bool TickContinuous()
        {
            return true;
        }
    }
}