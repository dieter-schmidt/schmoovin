using System;
using UnityEngine;

namespace NeoFPSEditor.CharacterMotion
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MotionGraphBehaviourEditorAttribute : Attribute
    {
        public Type behaviourType { get; private set; }

        public MotionGraphBehaviourEditorAttribute(Type t)
        {
            behaviourType = t;
        }
    }
}