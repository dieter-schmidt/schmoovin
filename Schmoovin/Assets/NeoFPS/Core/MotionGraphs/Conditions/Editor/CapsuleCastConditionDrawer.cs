using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(CapsuleCastCondition))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgc-capsulecastcondition.html")]
    public class CapsuleCastConditionDrawer : MotionGraphConditionDrawer
    {
        protected override int numLines
        {
            get { return 3; }
        }

        protected override void Inspect (Rect line1)
        {
            Rect r1 = line1;
            Rect r2 = r1;
            r1.width *= 0.5f;
            r2.width *= 0.5f;
            r2.x += r1.width;

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