using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(ClimbableCondition))]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgc-climbablecondition.html")]
    public class ClimbableConditionDrawer : MotionGraphConditionDrawer
    {
        protected override int numLines
        {
            get
            {
                if (MotionGraphEditorGUI.DataReferenceFieldNotSet(serializedObject.FindProperty("m_CheckDistance")))
                    return 8;
                else
                    return 7;
            }
        }

        protected override void Inspect(Rect line1)
        {
            Rect r1 = line1;
            Rect r2 = r1;
            r1.width *= 0.5f;
            r2.width *= 0.5f;
            r2.x += r1.width;

            // Draw wall normal parameter dropdown
            EditorGUI.LabelField(r1, "Output Wall Normal");
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(r2, graphRoot, serializedObject.FindProperty("m_OutputWallNormal"));

            // Draw check direction
            line1.y += lineOffset;
            EditorGUI.PropertyField(line1, serializedObject.FindProperty("m_CheckDirection"));

            // Draw check distance data dropdown (and value if required)
            line1.y += lineOffset;
            if (MotionGraphEditorGUI.FloatDataReferenceField(line1, graphRoot, serializedObject.FindProperty("m_CheckDistance")))
                line1.y += lineOffset; // Had to draw value, add another line spacing

            line1.y += lineOffset;
            EditorGUI.PropertyField(line1, serializedObject.FindProperty("m_WallCollisionMask"));
            line1.y += lineOffset;
            EditorGUI.PropertyField(line1, serializedObject.FindProperty("m_MaxClimbHeight"));
            line1.y += lineOffset;
            EditorGUI.PropertyField(line1, serializedObject.FindProperty("m_ClimbForward"));

            // Draw climb height parameter dropdown
            line1.y += lineOffset;
            r1 = line1;
            r2 = r1;
            r1.width *= 0.5f;
            r2.width *= 0.5f;
            r2.x += r1.width;
            EditorGUI.LabelField(r1, "Output Climb Height");
            MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(r2, graphRoot, serializedObject.FindProperty("m_OutputClimbHeight"));
        }
    }
}
