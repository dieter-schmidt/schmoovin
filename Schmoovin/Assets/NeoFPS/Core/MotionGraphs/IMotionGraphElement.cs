using System;

namespace NeoFPS.CharacterMotion
{
    public interface IMotionGraphElement
    {
        void CheckReferences(IMotionGraphMap map);
    }
}
