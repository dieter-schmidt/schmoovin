using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion.Conditions
{
    public interface IScriptedComponentCondition
    {
        string key { get; }

        bool CheckCondition();
    }
}