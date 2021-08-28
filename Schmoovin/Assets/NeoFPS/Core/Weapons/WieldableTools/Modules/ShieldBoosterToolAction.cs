using UnityEngine;

namespace NeoFPS.WieldableTools
{
    public class ShieldBoosterToolAction : BaseWieldableToolModule
    {
        [SerializeField, FlagsEnum, Tooltip("When should the shield booster be applied.")]
        private WieldableToolOneShotTiming m_Timing = WieldableToolOneShotTiming.Start;
        [SerializeField, Tooltip("The number of shield steps to recharge.")]
        private int m_StepCount = 1;

        private IShieldManager m_ShieldManager = null;

        public override bool isValid
        {
            get { return m_Timing != 0; }
        }

        private void OnValidate()
        {
            m_StepCount = Mathf.Clamp(m_StepCount, 1, 100);
        }

        public override WieldableToolActionTiming timing
        {
            get { return (WieldableToolActionTiming)m_Timing; }
        }

        public override void Initialise(IWieldableTool t)
        {
            base.Initialise(t);

            m_ShieldManager = t.wielder.GetComponent<IShieldManager>();
            if (m_ShieldManager == null)
                enabled = false;
        }

        public override void FireStart()
        {
            m_ShieldManager.FillShieldSteps(m_StepCount);
        }

        public override void FireEnd(bool success)
        {
            m_ShieldManager.FillShieldSteps(m_StepCount);
        }

        public override bool TickContinuous()
        {
            return true;
        }
    }
}