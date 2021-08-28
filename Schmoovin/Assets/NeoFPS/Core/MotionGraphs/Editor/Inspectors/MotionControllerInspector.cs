using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPSEditor.CharacterMotion.Debugger;

namespace NeoFPSEditor.CharacterMotion
{
    [CustomEditor (typeof (MotionController), true)]
    public class MotionControllerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            // Show motion graph editor button
            if (GUILayout.Button("Show Motion Graph Editor"))
                MotionGraphEditor.CreateWindow();

            // Show motion controller debugger button
            if (GUILayout.Button("Attach Debugger"))
            {
                var controller = target as MotionController;
                MotionControllerDebugger.CreateWindow(controller);
            }

            base.OnInspectorGUI();
        }
    }
}