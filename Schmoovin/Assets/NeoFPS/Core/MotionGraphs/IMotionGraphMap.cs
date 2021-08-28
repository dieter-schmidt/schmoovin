using System;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    public interface IMotionGraphMap
    {
        T Swap<T> (T original) where T : ScriptableObject;
    }
}

