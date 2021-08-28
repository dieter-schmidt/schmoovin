using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public interface IModularFirearmModeSwitcher
    {
        string currentMode { get; }

        void GetStartingMode();
        void SwitchModes();
    }
}