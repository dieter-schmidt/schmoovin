using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(WaterCondition))]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgc-watercondition.html")]
    public class WaterConditionDrawer : MotionGraphConditionDrawer
    {
        private const float k_LabelWidth = 100f;

        protected override int numLines
        {
            get { return 2; }
        }

        protected override void Inspect (Rect line1)
        {
            Rect r1 = line1;
            Rect r2 = r1;
            r1.width = k_LabelWidth;
            r2.width -= k_LabelWidth;
            r2.x += r1.width;

            EditorGUI.LabelField(r1, "Water Zone");
            MotionGraphEditorGUI.ParameterDropdownField<TransformParameter>(r2, graphRoot, serializedObject.FindProperty("m_WaterZoneTransform"));
            
            r1.y += lineOffset;
            r2.y += lineOffset;
            r2.width *= 0.5f;
            Rect r3 = r2;
            r3.x += r3.width + 2f;
            r3.width -= 2f;

            EditorGUI.LabelField(r1, "Check");
            var prop = serializedObject.FindProperty("m_CheckType");
            EditorGUI.PropertyField(r2, prop, new GUIContent());
            if (prop.enumValueIndex != 0)
                EditorGUI.PropertyField(r3, serializedObject.FindProperty("m_CheckValue"), new GUIContent());
            else
                EditorGUI.LabelField(r3, "...");
        }
    }
}