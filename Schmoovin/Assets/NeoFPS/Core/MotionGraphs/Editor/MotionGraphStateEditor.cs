using UnityEditor;
using NeoFPS.CharacterMotion;

namespace NeoFPSEditor.CharacterMotion
{
    [CustomEditor(typeof(MotionGraphState), true)]
    public class MotionGraphStateEditor : MotionGraphConnectableEditor
    {
        static readonly char[] k_PathSeparators = new char[] { '\\', '/' };

        private string m_TypeName = string.Empty;

        public MotionGraphState state { get; private set; }

        public override MotionGraphContainer container
        {
            get { return state.parent.container; }
        }

        public override string typeName
        {
            get { return m_TypeName; }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Get state
            state = connectable as MotionGraphState;

            // Get state display name
            var attributes = state.GetType().GetCustomAttributes(false);
            foreach(var attr in attributes)
            {
                var element = attr as MotionGraphElementAttribute;
                if (element == null)
                    continue;

                var split = element.menuPath.Split(k_PathSeparators);
                m_TypeName = split[split.Length - 1];
                break;
            }
        }

        protected override void OnInspectorGUIInternal()
        {
            // Draw all visible properties after "stateName" (currently the last property in the base state class)
            var iterator = serializedObject.FindProperty("stateName");
            while (iterator.NextVisible(false))
                EditorGUILayout.PropertyField(iterator, true);
        }
    }
}
