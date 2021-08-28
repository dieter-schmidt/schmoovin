using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(GroundContactCondition))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgc-groundcontactcondition.html")]
    public class GroundContactConditionDrawer : MotionGraphConditionDrawer
    {
        protected override void Inspect (Rect line1)
        {
            Rect r1 = line1;
            Rect r2 = r1;
            Rect r3 = r1;
            r1.width *= 0.5f;
            r2.width *= 0.2f;
            r3.width *= 0.3f;
            r2.x += r1.width;
            r3.x += r1.width + r2.width + 2f;
            r3.width -= 2f;
            r1.width -= 2f;

            EditorGUI.LabelField(r1, "Ground Contact");
            EditorGUI.LabelField(r2, "equals", MotionGraphEditorStyles.labelCentered);
            EditorGUI.PropertyField(r3, serializedObject.FindProperty("m_Comparison"), new GUIContent(""));
        }
    }
}