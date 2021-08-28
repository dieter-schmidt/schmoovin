using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;
using System.Collections;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Misc/SetTimeScale", "SetTimeScaleBehaviour")]
    public class SetTimeScaleBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("The target timescale to set. Will be reset to 1 on exit.")]
        private float m_TimeScale = 0.5f;
        [SerializeField, Tooltip("The amount of charge drained per (real, unscaled) second.")]
        private float m_ChargeDrain = 0.2f;

        private ISlowMoSystem m_SlowMoSystem = null;

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);
            m_SlowMoSystem = controller.GetComponent<ISlowMoSystem>();
        }

        public override void OnEnter()
        {
            if (m_SlowMoSystem != null)
                m_SlowMoSystem.SetTimeScale(m_TimeScale, m_ChargeDrain);
        }

        public override void OnExit()
        {
            if (m_SlowMoSystem != null)
                m_SlowMoSystem.ResetTimescale();
        }
    }
}