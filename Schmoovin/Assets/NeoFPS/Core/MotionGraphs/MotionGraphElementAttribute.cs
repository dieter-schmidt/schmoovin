using System;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MotionGraphElementAttribute : Attribute
    {
#if UNITY_EDITOR
        public string menuPath { get; private set; }
        public string defaultName { get; private set; }

        public MotionGraphElementAttribute(string menuPath)
        {
            this.menuPath = menuPath;
            this.defaultName = string.Empty;
        }
        public MotionGraphElementAttribute(string menuPath, string defaultName)
        {
            this.menuPath = menuPath;
            this.defaultName = defaultName;
        }
#else
        public MotionGraphElementAttribute(string menuPath) {}
        public MotionGraphElementAttribute(string menuPath, string defaultName) {}
#endif
    }
}