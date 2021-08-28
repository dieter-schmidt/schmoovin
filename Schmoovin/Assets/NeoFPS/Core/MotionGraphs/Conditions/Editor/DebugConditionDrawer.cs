using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(DebugCondition))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgc-debugcondition.html")]
    public class DebugConditionDrawer : MotionGraphConditionDrawer
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

            EditorGUI.LabelField(r1, "Message");
            EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_Message"), new GUIContent());

            r1.y += lineOffset;
            r2.y += lineOffset;

            EditorGUI.LabelField(r1, "Result");
            EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_Result"), new GUIContent());
        }
    }
}
