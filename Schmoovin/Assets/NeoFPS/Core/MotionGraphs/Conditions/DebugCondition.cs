using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Graph/Debug")]
    public class DebugCondition : MotionGraphCondition
    {
        [SerializeField] private bool m_Result = true;
        [SerializeField] private string m_Message = "...";

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (!string.IsNullOrEmpty(m_Message))
                Debug.Log(string.Format("Checking condition on connection from {0}: {1}", connectable.name, m_Message));
            return m_Result;
        }
    }
}