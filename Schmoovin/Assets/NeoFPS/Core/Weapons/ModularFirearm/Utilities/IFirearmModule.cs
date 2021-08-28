using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public interface IFirearmModule
    {
        IModularFirearm firearm { get; }

        void Enable();
        void Disable();
    }
}