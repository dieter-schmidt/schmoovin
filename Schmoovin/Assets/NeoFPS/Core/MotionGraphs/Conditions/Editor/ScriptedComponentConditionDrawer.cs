using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(ScriptedComponentCondition))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgc-scriptedcomponentcondition.html")]
    public class ScriptedComponentConditionDrawer : MotionGraphConditionDrawer
    {
        protected override void Inspect (Rect line1)
        {
            EditorGUI.PropertyField (line1, serializedObject.FindProperty("m_Key"));
        }
    }
}