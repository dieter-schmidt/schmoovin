using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public interface IFirearmModuleValidity
    {
        bool isModuleValid { get; }
    }
}