using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    public interface ISlowMoSystem
    {
        float charge
        {
            get;
            set;
        }

        bool isTimeScaled
        {
            get;
        }

        event UnityAction<float> onChargeChanged;

        void SetTimeScale(float ts, float drainRate = 0f);

        void ResetTimescale();
    }
}
