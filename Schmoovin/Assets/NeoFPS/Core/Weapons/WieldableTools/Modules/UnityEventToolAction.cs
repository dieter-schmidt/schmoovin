using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.WieldableTools
{
    public class UnityEventToolAction : BaseWieldableToolModule
    {
        [SerializeField, FlagsEnum, Tooltip("When should the event fire")]
        private WieldableToolOneShotTiming m_Timing = WieldableToolOneShotTiming.Start;
        [SerializeField, Tooltip("The event to fire")]
        private UnityEvent m_Event = new UnityEvent();

        public override bool isValid
        {
            get { return  m_Timing != 0; ; }
        }

        public override void Initialise(IWieldableTool t)
        {
            base.Initialise(t);

            if (m_Timing == 0)
                enabled = false;
        }

        public override WieldableToolActionTiming timing
        {
            get { return (WieldableToolActionTiming)m_Timing; }
        }

        public override void FireStart()
        {
            m_Event.Invoke();
        }

        public override void FireEnd(bool success)
        {
            m_Event.Invoke();
        }

        public override bool TickContinuous()
        {
            return true;
        }
    }
}