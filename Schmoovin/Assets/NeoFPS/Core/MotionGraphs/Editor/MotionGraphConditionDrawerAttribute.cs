using System;
using UnityEngine;

namespace NeoFPSEditor.CharacterMotion
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MotionGraphConditionDrawerAttribute : Attribute
    {
        public Type conditionType { get; private set; }

        public MotionGraphConditionDrawerAttribute(Type t)
        {
            conditionType = t;
        }
    }
}