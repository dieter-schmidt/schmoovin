using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(NullState))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgs-nullstate.html")]
    public class NullStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.HelpBox("Null states are only intended as a means to branch transitions and don't actually do anything by themselves.\n\n" +
                "Make sure that one of the transitions is always valid. This state always returns true for completed, so it is recommended that you use a \"Completed\" condition for the last transition.", MessageType.Info);
        }
    }
}