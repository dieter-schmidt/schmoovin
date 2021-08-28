using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    public abstract class MotionGraphData<ValueType> : MotionGraphDataBase
    {
        [SerializeField] protected ValueType m_Value = default(ValueType);

        public abstract ValueType value { get; }
    }
}