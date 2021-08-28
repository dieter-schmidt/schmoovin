using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoFPS
{
    public interface IPlayerCharacterWatcher
    {
        void AttachSubscriber(IPlayerCharacterSubscriber subscriber);
        void ReleaseSubscriber(IPlayerCharacterSubscriber subscriber);
    }
}
