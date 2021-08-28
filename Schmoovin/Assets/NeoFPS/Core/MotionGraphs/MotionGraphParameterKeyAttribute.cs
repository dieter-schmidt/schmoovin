using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    public class MotionGraphParameterKeyAttribute : PropertyAttribute
    {
        public MotionGraphParameterType parameterType
        {
            get;
            private set;
        }

        public MotionGraphParameterKeyAttribute(MotionGraphParameterType parameterType)
        {
            this.parameterType = parameterType;
        }
    }
}
