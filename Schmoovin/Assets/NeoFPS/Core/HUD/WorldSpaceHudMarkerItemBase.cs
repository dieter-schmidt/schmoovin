using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent (typeof (RectTransform))]
    public abstract class WorldSpaceHudMarkerItemBase : MonoBehaviour
    {
        public RectTransform localTransform
        {
            get;
            private set;
        }

        protected virtual void Awake()
        {
            localTransform = transform as RectTransform;
        }

        public abstract Vector3 GetWorldPosition();
    }
}