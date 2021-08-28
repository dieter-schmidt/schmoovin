using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public interface ITargetingSystem
    {
        void RegisterTracker(ITargetTracker tracker);
    }
}
