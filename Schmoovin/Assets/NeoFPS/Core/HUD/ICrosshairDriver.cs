using System;
using UnityEngine;
using UnityEngine.Events;
using NeoFPS.Constants;

namespace NeoFPS
{
    public interface ICrosshairDriver
    {
        FpsCrosshair crosshair { get; }

        float accuracy { get; }

        void HideCrosshair();
        void ShowCrosshair();

        event UnityAction<FpsCrosshair> onCrosshairChanged;
        event UnityAction<float> onAccuracyChanged;
    }
}
