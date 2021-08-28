using System;
using UnityEngine;


namespace NeoFPS
{
    public class NeoPrefabFieldAttribute : PropertyAttribute
    {
        public Type[] filterTypes
        {
            get;
            private set;
        }

        public bool required
        {
            get;
            set;
        }

        public NeoPrefabFieldAttribute()
        {
            filterTypes = null;
        }

        public NeoPrefabFieldAttribute(Type filterType)
        {
            filterTypes = new Type[] { filterType };
        }

        public NeoPrefabFieldAttribute(Type[] filterTypes)
        {
            this.filterTypes = filterTypes;
        }
    }
}
