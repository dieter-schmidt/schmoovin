using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class FpsInventoryKeyAttribute : PropertyAttribute
    {
        public bool showLabel
        {
            get;
            private set;
        }

        public bool required
        {
            get;
            set;
        }

        public FpsInventoryKeyAttribute()
        {
            showLabel = true;
        }

        public FpsInventoryKeyAttribute(bool showLabel)
        {
            this.showLabel = showLabel;
        }
    }
}