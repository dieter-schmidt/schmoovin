using System;
using UnityEngine;


namespace NeoFPS
{
    public class ComponentOnObjectAttribute : PropertyAttribute
    {
        public Type componentType
        {
            get;
            private set;
        }

        public bool allowSelf
        {
            get;
            private set;
        }

        public bool required
        {
            get;
            private set;
        }

        public ComponentOnObjectAttribute(Type t)
        {
            componentType = t;
            required = true;
            allowSelf = false;
        }

        public ComponentOnObjectAttribute(Type t, bool allowSelf)
        {
            componentType = t;
            required = true;
            this.allowSelf = allowSelf;
        }

        public ComponentOnObjectAttribute(Type t, bool allowSelf, bool required)
        {
            componentType = t;
            this.allowSelf = allowSelf;
            this.required = required;
        }
    }
}
