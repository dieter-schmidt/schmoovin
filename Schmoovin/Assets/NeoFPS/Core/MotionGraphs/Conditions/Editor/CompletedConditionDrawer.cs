using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(CompletedCondition))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgc-completedcondition.html")]
    public class CompletedConditionDrawer : MotionGraphConditionDrawer
    {
        protected override int numLines
        {
            get { return 0; }
        }
    }
}