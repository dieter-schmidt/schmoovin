using UnityEngine;

namespace NeoFPS
{
    interface IPickAngleLockpickPopup
    {
        void ApplyInput(float pickRotation, bool tension);
        void Cancel();
    }
}
