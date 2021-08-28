using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Graph/Completed")]
    public class CompletedCondition : MotionGraphCondition 
    {
        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            var state = connectable as MotionGraphState;
            if (state == null)
                return false;
            else
                return state.completed;
        }
    }
}