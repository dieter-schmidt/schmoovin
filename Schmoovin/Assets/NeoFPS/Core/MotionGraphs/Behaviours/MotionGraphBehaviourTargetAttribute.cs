using System;

namespace NeoFPS.CharacterMotion
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MotionGraphBehaviourTargetAttribute : Attribute
    {
#if UNITY_EDITOR
        public ValidConnectables validConnectable { get; private set; }

        public MotionGraphBehaviourTargetAttribute(ValidConnectables validFor)
        {
            validConnectable = validFor;
        }
#else
        public MotionGraphBehaviourTargetAttribute(ValidConnectables validFor) {}
#endif
    }

    public enum ValidConnectables
    {
        Both,
        State,
        Graph
    }
}
