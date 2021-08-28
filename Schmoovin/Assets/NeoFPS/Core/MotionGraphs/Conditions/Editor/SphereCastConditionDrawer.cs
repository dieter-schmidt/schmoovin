using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(SphereCastCondition))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgc-spherecastcondition.html")]
    public class SphereCastConditionDrawer : MotionGraphConditionDrawer
    {
        protected override int numLines
        {
            get { return 4; }
        }

        protected override void Inspect (Rect line1)
        {
            Rect r1 = line1;
            Rect r2 = r1;
            r1.width *= 0.5f;
            r2.width *= 0.5f;
            r2.x += r1.width;

            EditorGUI.LabelField (r1, "Source Height");
            EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_NormalisedHeight"), new GUIContent());

            r1.y += lineOffset;
            r2.y += lineOffset;

            EditorGUI.LabelField (r1, "Cast Vector");
            EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_CastVector"), new GUIContent());

            r1.y += lineOffset;
            r2.y += lineOffset;

            EditorGUI.LabelField (r1, "Layer Mask");
            EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_LayerMask"), new GUIContent());

            r1.y += lineOffset;
            r2.y += lineOffset;

            EditorGUI.LabelField (r1, "Does Hit");
            EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_DoesHit"), new GUIContent());
        }
    }
}