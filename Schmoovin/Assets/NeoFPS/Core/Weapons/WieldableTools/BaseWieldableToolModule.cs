using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.WieldableTools
{
    public abstract class BaseWieldableToolModule : MonoBehaviour, IWieldableToolModule
    {
        protected const WieldableToolActionTiming k_TimingsStartOnly = WieldableToolActionTiming.Start;
        protected const WieldableToolActionTiming k_TimingsEndOnly = WieldableToolActionTiming.End;
        protected const WieldableToolActionTiming k_TimingsStartAndEnd = WieldableToolActionTiming.Start | WieldableToolActionTiming.End;
        protected const WieldableToolActionTiming k_TimingsContinuousOnly = WieldableToolActionTiming.Continuous;
        protected const WieldableToolActionTiming k_TimingsAll = WieldableToolActionTiming.Start | WieldableToolActionTiming.Continuous | WieldableToolActionTiming.End;

        public bool initialised { get; private set; }

        public virtual bool blocking { get { return false; } }

        protected IWieldableTool tool { get; private set; }

        public virtual void Initialise(IWieldableTool t)
        {
            tool = t;
            initialised = true;
        }

        public abstract bool isValid { get; }

        public abstract WieldableToolActionTiming timing { get; }

        public abstract void FireStart();
        public abstract void FireEnd(bool success);
        public abstract bool TickContinuous();
    }
}