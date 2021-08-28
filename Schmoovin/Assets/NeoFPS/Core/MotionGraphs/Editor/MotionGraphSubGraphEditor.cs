using UnityEditor;
using NeoFPS.CharacterMotion;

namespace NeoFPSEditor.CharacterMotion
{
    [CustomEditor(typeof(MotionGraph), true)]
    public class MotionGraphSubGraphEditor : MotionGraphConnectableEditor
    {
        public MotionGraph graph { get; private set; }

        public override MotionGraphContainer container
        {
            get { return graph.container; }
        }

        public override string typeName
        {
            get { return string.Empty; }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Get graph
            graph = connectable as MotionGraph;
        }

        protected override void OnInspectorGUIInternal() {}
    }
}
