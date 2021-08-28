using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Misc/Debug", "DebugBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-debugbehaviour.html")]
    public class DebugBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("The float parameter to read from & write to")]
        private string m_OnEnterMessage = string.Empty;

        [SerializeField, Tooltip("The float parameter to read from & write to")]
        private string m_OnExitMessage = string.Empty;

        [SerializeField, Tooltip("The float parameter to read from & write to")]
        private bool m_LogElapsedTime = true;

        public override void OnEnter()
        {
            if (!string.IsNullOrEmpty(m_OnEnterMessage))
                Debug.Log(m_OnEnterMessage);
        }

        public override void OnExit()
        {
            if (!string.IsNullOrEmpty(m_OnExitMessage))
                Debug.Log(m_OnExitMessage);

            if (m_LogElapsedTime)
            {
                if (owner is MotionGraphState)
                    Debug.Log("Time in state: " + owner.elapsedTime.ToString("F5"));
                else
                    Debug.Log("Time in sub-graph: " + owner.elapsedTime.ToString("F5"));
            }
        }
    }
}