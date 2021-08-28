using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(DirectionCondition))]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgc-directioncondition.html")]
    public class DirectionConditionDrawer : MotionGraphConditionDrawer
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

            EditorGUI.LabelField(r1, "Target Direction");
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(r2, graphRoot, serializedObject.FindProperty("m_VectorParameter"));

            r1.y += lineOffset;
            r2.y += lineOffset;
            Rect r3 = r2;
            r2.width *= 0.4f;
            r3.width *= 0.6f;
            r3.x += r2.width + 2f;
            r3.width -= 2f;
            r1.width -= 2f;

            EditorGUI.PropertyField(r1, serializedObject.FindProperty("m_Comparison"), new GUIContent());

            // Draw the comparison type dropdown
            var comparisonTypeString = GetComparisonTypeString(serializedObject.FindProperty("m_LessThan").boolValue);
            if (EditorGUI.DropdownButton(r2, new GUIContent(comparisonTypeString), FocusType.Passive))
            {
                GenericMenu gm = new GenericMenu();
                gm.AddItem(new GUIContent(GetComparisonTypeString(true)), false, OnComparisonTypeDropdownSelect, true);
                gm.AddItem(new GUIContent(GetComparisonTypeString(false)), false, OnComparisonTypeDropdownSelect, false);
                gm.ShowAsContext();
            }

            // Draw the compare value property
            EditorGUI.PropertyField(r3, serializedObject.FindProperty("m_Angle"), new GUIContent());
        }

        void OnComparisonTypeDropdownSelect (object o)
        {
            bool lessThan = (bool)o;
            serializedObject.FindProperty("m_LessThan").boolValue = lessThan;
            serializedObject.ApplyModifiedProperties();
        }

        string GetComparisonTypeString (bool lessThan)
        {
            if (lessThan)
                return "<";
            else
                return ">";
        }
    }
}