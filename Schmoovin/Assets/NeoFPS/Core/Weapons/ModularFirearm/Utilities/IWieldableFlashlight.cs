using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    public interface IWieldableFlashlight
    {
        bool on { get; set; }
        void Toggle();

        float brightness { get; set; }
    }
}