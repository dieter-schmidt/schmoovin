using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoFPS.Constants;

namespace NeoFPS
{
    public class InputMenu : FpsInput
    {
        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Menu; }
        }

        protected override void UpdateInput() { }
    }
}
