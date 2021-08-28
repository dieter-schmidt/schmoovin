using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using NeoFPS.CharacterMotion.Conditions;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(HeightRestrictionCondition))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgc-heightrestrictioncondition.html")]
    public class HeightRestrictionConditionDrawer : MotionGraphConditionDrawer
    {
        protected override int numLines
        {
            get { return 2; }
        }

        protected override void Inspect (Rect line1)
        {                
            Rect r1 = line1;
            Rect r2 = r1;
            r1.width *= 0.5f;
            r2.width *= 0.5f;
            r2.x += r1.width;

            EditorGUI.LabelField(r1, "Target Height");
            EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_TargetHeight"), GUIContent.none);

            r1.y += lineOffset;
            r2.y += lineOffset;

            EditorGUI.LabelField(r1, "Is Blocked");
            EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_Blocked"), GUIContent.none);
        }
    }
}
