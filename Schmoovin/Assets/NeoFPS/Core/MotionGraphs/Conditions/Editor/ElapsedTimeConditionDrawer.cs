using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(ElapsedTimeCondition))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgc-elapsedtimecondition.html")]
    public class ElapsedTimeConditionDrawer : MotionGraphConditionDrawer
    {
        protected override void Inspect (Rect line1)
        {
            Rect r1 = line1;
            r1.width *= 0.6f;

            // Draw the property selection dropdown
            EditorGUI.LabelField(r1, "Elapsed time (seconds)");

            SerializedProperty prop = serializedObject.FindProperty("m_TimeoutProperty");
            if (prop.objectReferenceValue == null)
            {
                Rect r2 = line1;
                Rect r3 = line1;
                r2.width *= 0.28f;
                r2.x += r1.width + 2f;
                r2.width -= 2f;
                r3.width *= 0.12f;
                r3.x += r1.width + r2.width + 4f;
                r3.width -= 4f;

                // Draw the timeout value property
                EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_TimeoutValue"), new GUIContent());
                
                MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(r3, graphRoot, prop);
            }
            else
            {
                Rect r2 = line1;
                r2.width *= 0.4f;
                r2.x += r1.width + 2f;
                r2.width -= 2f;
                
                MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(r2, graphRoot, prop);
            }
        }
    }
}
