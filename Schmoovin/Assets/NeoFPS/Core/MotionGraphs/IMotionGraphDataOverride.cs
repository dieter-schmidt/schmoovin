using System;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPS.CharacterMotion
{
    public interface IMotionGraphDataOverride
    {
        Func<bool, bool> GetBoolOverride(BoolData data);
        Func<float, float> GetFloatOverride(FloatData data);
        Func<int, int> GetIntOverride(IntData data);
    }
}
