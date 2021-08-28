using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPSEditor.CharacterMotion
{
    public interface IMotionGraphEditorInspector
    {
        void OnInspectorGUI (MotionGraphEditor editor);
    }
}