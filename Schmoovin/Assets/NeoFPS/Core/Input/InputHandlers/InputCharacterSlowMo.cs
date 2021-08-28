using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS
{
	[HelpURL("https://docs.neofps.com/manual/inputref-mb-inputcharacterslowmo.html")]
	[RequireComponent (typeof (ICharacter))]
	public class InputCharacterSlowMo : CharacterInputBase
    {
		[Header ("Features")]

		[SerializeField, Tooltip("The time-scale to use for ability based slow-mo.")]
        private float m_TimeScale = 0.25f;
        [SerializeField, Tooltip("The rate to drain slow-mo charge (time scale will return to normal when charge reaches zero).")]
        private float m_DrainRate = 0.5f;

        private ISlowMoSystem m_SlowMoSystem = null;

        void OnValidate()
        {
            m_TimeScale = Mathf.Clamp(m_TimeScale, 0.01f, 2f);
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
            if (m_SlowMoSystem != null && GetButton(FpsInputButton.Ability))
            {
                if (m_SlowMoSystem.isTimeScaled)
                    m_SlowMoSystem.ResetTimescale();
                else
                    m_SlowMoSystem.SetTimeScale(m_TimeScale, m_DrainRate);
            }
        }
	}
}