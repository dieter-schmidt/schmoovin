using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS
{
	[HelpURL("https://docs.neofps.com/manual/inputref-mb-inputcharacterslowmo.html")]
	[RequireComponent (typeof (ICharacter))]
	public class InputCharacterTimeDS : CharacterInputBase
    {
        [Header("Features")]

        [SerializeField, Tooltip("The time-scale to use for ability based slow-mo.")]
        private float m_SlowTimeScale = 0.15f;
        [SerializeField, Tooltip("The time-scale to use for ability based fast-mo.")]
        private float m_FastTimeScale = 1.5f;
        [SerializeField, Tooltip("The rate to drain slow-mo charge (time scale will return to normal when charge reaches zero).")]
        private float m_DrainRate = 0.5f;

        private ISlowMoSystem m_SlowMoSystem = null;

        //getters
        public float M_SlowTimeScale { get { return m_SlowTimeScale; } }
        public float M_FastTimeScale { get { return m_FastTimeScale; } }

        private enum TimeState{
            Default,
            Fast,
            Slow
        }
        private TimeState timeState = TimeState.Default;


        void OnValidate()
        {
            m_SlowTimeScale = Mathf.Clamp(m_SlowTimeScale, 0.01f, 2f);
            m_FastTimeScale = Mathf.Clamp(m_FastTimeScale, 0.01f, 2f);
            if (m_DrainRate < 0f)
                m_DrainRate = 0f;
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            m_SlowMoSystem = GetComponent<ISlowMoSystem>();
        }

        protected override void UpdateInput()
        {
            if (m_SlowMoSystem != null && GetButtonDown(FpsInputButton.PickUp))
            {
                if (m_SlowMoSystem.isTimeScaled)
                {
                    if (timeState == TimeState.Slow)
                    {
                        timeState = TimeState.Default;
                        m_SlowMoSystem.ResetTimescale();
                        //play audio
                        GameObject.Find("AudioSlomoOut").GetComponent<AudioSource>().Play();
                    }
                    else if (timeState == TimeState.Fast)
                    {
                        timeState = TimeState.Slow;
                        m_SlowMoSystem.SetTimeScale(m_SlowTimeScale, m_DrainRate);
                        //play audio
                        GameObject.Find("AudioSlomoIn").GetComponent<AudioSource>().Play();
                    }

                }
                else
                {
                    timeState = TimeState.Slow;
                    m_SlowMoSystem.SetTimeScale(m_SlowTimeScale, m_DrainRate);
                    //play audio
                    GameObject.Find("AudioSlomoIn").GetComponent<AudioSource>().Play();
                }
            }
            else if (m_SlowMoSystem != null && GetButtonDown(FpsInputButton.LeanRight))
            {
                if (m_SlowMoSystem.isTimeScaled)
                {
                    if (timeState == TimeState.Fast)
                    {
                        timeState = TimeState.Default;
                        m_SlowMoSystem.ResetTimescale();
                        //play audio
                        GameObject.Find("AudioSlomoIn").GetComponent<AudioSource>().Play();
                    }
                    else if (timeState == TimeState.Slow)
                    {
                        timeState = TimeState.Fast;
                        m_SlowMoSystem.SetTimeScale(m_FastTimeScale, m_DrainRate);
                        //play audio
                        GameObject.Find("AudioSlomoOut").GetComponent<AudioSource>().Play();
                    }

                }
                else
                {
                    timeState = TimeState.Fast;
                    m_SlowMoSystem.SetTimeScale(m_FastTimeScale, m_DrainRate);
                    //play audio
                    GameObject.Find("AudioSlomoOut").GetComponent<AudioSource>().Play();
                }
            }
        }
	}
}