using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;

namespace NeoFPSEditor.CharacterMotion
{
    [CustomEditor(typeof(MotionGraphContainer), true)]
    public class MotionGraphContainerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Show Motion Graph Editor"))
                MotionGraphEditor.CreateWindow();
        }
    }
}