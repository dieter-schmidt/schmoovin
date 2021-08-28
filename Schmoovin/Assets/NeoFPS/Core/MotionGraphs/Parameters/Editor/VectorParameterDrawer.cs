using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Parameters
{
    [MotionGraphPropertyDrawer(typeof(VectorParameter))]
    public class VectorParameterDrawer : MotionGraphPropertyDrawer
    {
        protected override void DrawValue(Rect r)
        {
            r.width -= (r.height + 2);
            EditorGUI.LabelField(r, "vector");

            r.x += r.width;
            r.width = r.height + 2;

            if (GUI.Button(r, "+"))
                PopupWindow.Show(r, new VectorParameterPopupContent(serializedObject));
        }
    }
}