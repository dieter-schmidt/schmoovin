using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Conditions;
using NeoCC;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(CollisionFlagsCondition))]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgc-collisionflagscondition.html")]
    public class CollisionFlagsConditionDrawer : MotionGraphConditionDrawer
    {
        protected override void Inspect (Rect line1)
        {
            Rect r1 = line1;
            Rect r2 = r1;
            r1.width *= 0.5f;
            r2.width *= 0.5f;
            r2.x += r1.width;

            // Draw the properties
            EditorGUI.PropertyField(r1, serializedObject.FindProperty("m_FilterType"), new GUIContent());

            var flags = (NeoCharacterCollisionFlags)serializedObject.FindProperty("m_Filter").intValue;
            var result = (NeoCharacterCollisionFlags)EditorGUI.EnumFlagsField(r2, new GUIContent(), flags);
            if (flags != result)
                serializedObject.FindProperty("m_Filter").intValue = (int)result;
        }
    }
}