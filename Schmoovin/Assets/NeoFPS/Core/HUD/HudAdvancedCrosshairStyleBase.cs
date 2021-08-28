using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class HudAdvancedCrosshairStyleBase : MonoBehaviour
    {
        public abstract void SetAccuracy(float accuracy);
        public abstract void ShowHitMarker(bool critical);
        public abstract void SetColour(Color c);
    }
}