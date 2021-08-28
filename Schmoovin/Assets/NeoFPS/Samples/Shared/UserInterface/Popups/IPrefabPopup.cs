using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace NeoFPS.Samples
{
    public interface IPrefabPopup
    {
        Selectable startingSelection { get; }
        BaseMenu menu { get; }

        void OnShow(BaseMenu m);

        void Back();
    }
}