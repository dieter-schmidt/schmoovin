using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(InputVectorCondition))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgc-inputvectorcondition.html")]
    public class InputVectorConditionDrawer : MotionGraphConditionDrawer
    {
        protected override void Inspect(Rect line1)
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

            EditorGUI.PropertyField(r1, serializedObject.FindProperty("m_Compare"), new GUIContent());

            // Draw the comparison type dropdown
            var comparisonTypeString = GetComparisonTypeString(serializedObject.FindProperty("m_Comparison").enumValueIndex);
            if (EditorGUI.DropdownButton(r2, new GUIContent(comparisonTypeString), FocusType.Passive))
            {
                GenericMenu gm = new GenericMenu();
                for (int i = 0; i < 6; ++i)
                    gm.AddItem(new GUIContent(GetComparisonTypeString(i)), false, OnComparisonTypeDropdownSelect, i);
                gm.ShowAsContext();
            }

            // Draw the compare value property
            EditorGUI.PropertyField(r3, serializedObject.FindProperty("m_CompareValue"), new GUIContent());
        }

        void OnComparisonTypeDropdownSelect(object o)
        {
            int index = (int)o;
            serializedObject.FindProperty("m_Comparison").enumValueIndex = index;
            serializedObject.ApplyModifiedProperties();
        }

        string GetComparisonTypeString(int i)
        {
            if (i == 0)
                return ">";
            else
                return "<";
        }
    }
}