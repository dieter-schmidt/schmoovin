using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Parameters
{
    [MotionGraphPropertyDrawer(typeof(TransformParameter))]
    public class TransformParameterDrawer : MotionGraphPropertyDrawer
    {
        protected override void DrawValue(Rect r)
        {
            EditorGUI.LabelField(r, "transform");
        }
    }
}