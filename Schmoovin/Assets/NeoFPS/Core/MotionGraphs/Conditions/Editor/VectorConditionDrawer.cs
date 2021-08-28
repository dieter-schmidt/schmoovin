using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(VectorCondition))]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgc-vectorcondition.html")]
    public class VectorConditionDrawer : MotionGraphConditionDrawer
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

            EditorGUI.LabelField(r1, "Parameter");
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(r2, graphRoot, serializedObject.FindProperty("m_Property"));

            r1.y += lineOffset;
            r2.y += lineOffset;
            Rect r3 = r2;
            r2.width *= 0.4f;
            r3.width *= 0.6f;
            r3.x += r2.width + 2f;
            r3.width -= 2f;
            r1.width -= 2f;

            EditorGUI.PropertyField(r1, serializedObject.FindProperty("m_What"), new GUIContent());

            // Draw the comparison type dropdown
            var comparisonTypeString = GetComparisonTypeString(serializedObject.FindProperty("m_ComparisonType").enumValueIndex);
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

        void OnComparisonTypeDropdownSelect (object o)
        {
            int index = (int)o;
            serializedObject.FindProperty("m_ComparisonType").enumValueIndex = index;
            serializedObject.ApplyModifiedProperties();
        }

        string GetComparisonTypeString (int i)
        {
            switch (i)
            {
                case 0:
                    return "=";
                case 1:
                    return "!=";
                case 2:
                    return ">";
                case 3:
                    return "> or =";
                case 4:
                    return "<";
                case 5:
                    return "< or =";
            }
            return "=";
        }
    }
}