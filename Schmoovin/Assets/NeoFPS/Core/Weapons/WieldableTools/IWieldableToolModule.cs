using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.WieldableTools
{
    public interface IWieldableToolModule
    {
        bool initialised { get; }
        bool isValid { get; }

        WieldableToolActionTiming timing { get; }

        bool blocking { get; }

        void Initialise(IWieldableTool tool);
        void FireStart();
        void FireEnd(bool success);
        bool TickContinuous();
    }

    [Flags]
    public enum WieldableToolActionTiming
    {
        Start = 1,
        End = 2,
        Continuous = 4
    }

    [Flags]
    public enum WieldableToolOneShotTiming
    {
        Start = 1,
        End = 2
    }
}