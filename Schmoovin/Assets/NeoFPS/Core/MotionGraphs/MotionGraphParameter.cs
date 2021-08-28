using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    public class MotionGraphParameter : ScriptableObject, IMotionGraphElement
    {
        public virtual void ResetValue () {}
        public virtual void CheckReferences(IMotionGraphMap map) {}
    }
}