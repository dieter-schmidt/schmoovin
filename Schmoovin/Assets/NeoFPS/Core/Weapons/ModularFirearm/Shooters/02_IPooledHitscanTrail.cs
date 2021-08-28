using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public interface IPooledHitscanTrail
    {
        void Show(Vector3 start, Vector3 end, float size, float time);
    }
}
