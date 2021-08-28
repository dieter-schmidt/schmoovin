using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(TransformCondition))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgc-transformcondition.html")]
    public class TransformConditionDrawer : MotionGraphConditionDrawer
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

            MotionGraphEditorGUI.ParameterDropdownField<TransformParameter>(r1, graphRoot, serializedObject.FindProperty("m_Property"));

            // Draw the comparison type dropdown
            EditorGUI.LabelField (r2, "is null?");

            // Draw the compare value property
            EditorGUI.PropertyField(r3, serializedObject.FindProperty("m_IsNull"), new GUIContent());
        }
    }
}