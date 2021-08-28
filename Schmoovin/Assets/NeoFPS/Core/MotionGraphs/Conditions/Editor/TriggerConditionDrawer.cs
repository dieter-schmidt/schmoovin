using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(TriggerCondition))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgc-triggercondition.html")]
    public class TriggerConditionDrawer : MotionGraphConditionDrawer
    {
        protected override void Inspect (Rect line1)
        {
            Rect r1 = line1;
            Rect r2 = r1;
            r1.width *= 0.5f;
            r2.width *= 0.5f;
            r2.x += r1.width + 6f;
            r1.width -= 2f;

            MotionGraphEditorGUI.ParameterDropdownField<TriggerParameter>(r1, graphRoot, serializedObject.FindProperty("m_Property"));

            EditorGUI.LabelField(r2, "triggered");
        }
    }
}