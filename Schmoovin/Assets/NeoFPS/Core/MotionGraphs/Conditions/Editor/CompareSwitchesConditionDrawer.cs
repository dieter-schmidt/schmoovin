using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(CompareSwitchesCondition))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgc-compareswitchescondition.html")]
    public class CompareSwitchesConditionDrawer : MotionGraphConditionDrawer
    {
        protected override void Inspect (Rect line1)
        {
            Rect r1 = line1;
            Rect r2 = r1;
            Rect r3 = r1;
            r1.width *= 0.4f;
            r2.width *= 0.2f;
            r3.width *= 0.4f;
            r2.x += r1.width;
            r3.x += r1.width + r2.width + 2f;
            r3.width -= 2f;
            r1.width -= 2f;

            MotionGraphEditorGUI.ParameterDropdownField<SwitchParameter>(r1, graphRoot, serializedObject.FindProperty("m_PropertyA"));

            // Draw the comparison type dropdown
            var comparisonTypeString = GetComparisonTypeString(serializedObject.FindProperty("m_Comparison").enumValueIndex);
            if (EditorGUI.DropdownButton(r2, new GUIContent(comparisonTypeString), FocusType.Passive))
            {
                GenericMenu gm = new GenericMenu();
                for (int i = 0; i < 2; ++i)
                    gm.AddItem(new GUIContent(GetComparisonTypeString(i)), false, OnComparisonTypeDropdownSelect, i);
                gm.ShowAsContext();
            }

            MotionGraphEditorGUI.ParameterDropdownField<SwitchParameter>(r3, graphRoot, serializedObject.FindProperty("m_PropertyB"));
        }

        void OnComparisonTypeDropdownSelect (object o)
        {
            int index = (int)o;
            serializedObject.FindProperty("m_Comparison").enumValueIndex = index;
            serializedObject.ApplyModifiedProperties();
        }

        string GetComparisonTypeString (int i)
        {
            if (i == 0)
                return "=";
            else
                return "!=";
        }
    }
}
