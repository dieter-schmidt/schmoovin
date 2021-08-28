using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    public class MotionGraphDataKeyAttribute : PropertyAttribute
    {
        public MotionGraphDataType dataType
        {
            get;
            private set;
        }

        public MotionGraphDataKeyAttribute(MotionGraphDataType dataType)
        {
            this.dataType = dataType;
        }
    }
}
