using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public interface IHudTargetLock
    {
        void SetLockTarget(Collider c);

        void SetPartialLockTarget(Collider c, float strength);

        void SetLockStrength(float strength);
    }
}
