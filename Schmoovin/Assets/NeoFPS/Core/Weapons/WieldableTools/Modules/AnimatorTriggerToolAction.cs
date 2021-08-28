using UnityEngine;

namespace NeoFPS.WieldableTools
{
    public class AnimatorTriggerToolAction : BaseWieldableToolModule
    {
        [SerializeField, FlagsEnum, Tooltip("When should the trigger fire")]
        private WieldableToolOneShotTiming m_Timing = WieldableToolOneShotTiming.Start;
        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Trigger, true, false), Tooltip("The animator trigger parameter to fire")]
        private string m_ParameterKey = string.Empty;

        private Animator m_Animator = null;
        private int m_ParameterHash = -1;

        public override bool isValid
        {
            get { return !string.IsNullOrWhiteSpace(m_ParameterKey) && m_Timing != 0; }
        }

        public override WieldableToolActionTiming timing
        {
            get { return (WieldableToolActionTiming)m_Timing; }
        }

        public override void Initialise(IWieldableTool t)
        {
            base.Initialise(t);

            m_Animator = GetComponentInChildren<Animator>();
            if (m_Animator != null && !string.IsNullOrWhiteSpace(m_ParameterKey))
                m_ParameterHash = Animator.StringToHash(m_ParameterKey);

            if (m_ParameterHash == -1 || m_Timing == 0)
                enabled = false;
        }

        public override void FireStart()
        {
            if (m_ParameterHash != -1)
                m_Animator.SetTrigger(m_ParameterHash);
        }

        public override void FireEnd(bool success)
        {
            if (m_ParameterHash != -1)
                m_Animator.SetTrigger(m_ParameterHash);
        }

        public override bool TickContinuous()
        {
            return true;
        }
    }
}