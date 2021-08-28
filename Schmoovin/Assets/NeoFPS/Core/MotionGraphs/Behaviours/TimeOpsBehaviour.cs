using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/TimeOps", "TimeOpsBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-timeopsbehaviour.html")]
    public class TimeOpsBehaviour : MotionGraphBehaviour
	{
		[SerializeField, Tooltip("The float parameter to read from & write to")]
		private FloatParameter m_Parameter = null;

        [SerializeField, Tooltip("What operation should be performed.")]
        private What m_What = What.AddElapsedTime;

        [SerializeField, Tooltip("The multiplier for the elapsed time")]
        private float m_Multiplier = 1f;

        public enum What
        {
            AddElapsedTime,
            AddElapsedTimeScaled,
            RecordEntryTime,
            RecordExitTime,
            RecordTime
        }

		public override void OnEnter()
		{
            if (m_What == What.RecordEntryTime && m_Parameter != null)
                m_Parameter.value = Time.time;
		}

		public override void OnExit()
        {
            if (m_What == What.RecordExitTime && m_Parameter != null)
                m_Parameter.value = Time.time;
        }

        public override void Update()
        {
            if (m_Parameter != null)
            {
                switch (m_What)
                {
                    case What.AddElapsedTime:
                        m_Parameter.value += Time.deltaTime;
                        break;
                    case What.AddElapsedTimeScaled:
                        m_Parameter.value += Time.deltaTime * m_Multiplier;
                        break;
                    case What.RecordTime:
                        m_Parameter.value = Time.time;
                        break;
                }
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
		{
            m_Parameter = map.Swap(m_Parameter);
        }
	}
}