using UnityEngine;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Misc/Null", "Null")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-nullstate.html")]
    public class NullState : MotionGraphState
    {
        public override bool completed
        {
            get { return true; }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Debug.LogError("Entering null motion state. There should always be a valid transition out of this state");
        }
    }
}