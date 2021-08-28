using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public interface IKeyRing
    {
        void AddKey(string id);
        void RemoveKey(string id);
        bool ContainsKey(string id);

        string[] GetKeys();
        void Merge(IKeyRing other);
    }
}