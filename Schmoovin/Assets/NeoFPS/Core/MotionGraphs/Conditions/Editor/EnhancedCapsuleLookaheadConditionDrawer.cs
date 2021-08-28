using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(EnhancedCapsuleLookaheadCondition))]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgc-enhancedcapsulelookaheadcondition.html")]
    public class EnhancedCapsuleLookaheadConditionDrawer : MotionGraphConditionDrawer
    {
        protected override int numLines
        {
            get { return 7; }
        }

        protected override void Inspect(Rect line1)
        {
            Rect r1 = line1;
            Rect r2 = r1;
            r1.width *= 0.5f;
            r2.width *= 0.5f;
            r2.x += r1.width;

            var typeProp = serializedObject.FindProperty("m_Lookahead");
            EditorGUI.LabelField(r1, "Lookahead");
            EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_Lookahead"), new GUIContent());

            r1.y += lineOffset;
            r2.y += lineOffset;

            var lookaheadType = (LookaheadType)typeProp.enumValueIndex;
            if (lookaheadType == LookaheadType.VelocityAllAxes || lookaheadType == LookaheadType.VelocityHorizontalPlane || lookaheadType == LookaheadType.VelocityVertical)
                EditorGUI.LabelField(r1, "Lookahead Time");
            else
                EditorGUI.LabelField(r1, "Lookahead Distance");
            EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_MultiplyValue"), new GUIContent());

            r1.y += lineOffset;
            r2.y += lineOffset;

            EditorGUI.LabelField(r1, "Layer Mask");
            EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_LayerMask"), new GUIContent());

            r1.y += lineOffset;
            r2.y += lineOffset;

            EditorGUI.LabelField(r1, "Does Hit");
            EditorGUI.PropertyField(r2, serializedObject.FindProperty("m_DoesHit"), new GUIContent());

            r1.y += lineOffset;
            r2.y += lineOffset;

            EditorGUI.LabelField(r1, "Hit Point Output");
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(r2, graphRoot, serializedObject.FindProperty("m_OutputPoint"));

            r1.y += lineOffset;
            r2.y += lineOffset;

            EditorGUI.LabelField(r1, "Hit Normal Output");
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(r2, graphRoot, serializedObject.FindProperty("m_OutputNormal"));

            r1.y += lineOffset;
            r2.y += lineOffset;

            EditorGUI.LabelField(r1, "Hit Transform Output");
            MotionGraphEditorGUI.ParameterDropdownField<TransformParameter>(r2, graphRoot, serializedObject.FindProperty("m_OutputTransform"));
        }
    }
}
