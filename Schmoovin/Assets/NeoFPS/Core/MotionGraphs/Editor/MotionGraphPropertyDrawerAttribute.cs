using System;
using UnityEngine;

namespace NeoFPSEditor.CharacterMotion
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MotionGraphPropertyDrawerAttribute : Attribute
    {
        public Type propertyType { get; private set; }

        public MotionGraphPropertyDrawerAttribute (Type t)
        {
            propertyType = t;
        }
    }
}