using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoFPS
{
    public interface IPlayerCharacterSubscriber
    {
        void OnPlayerCharacterChanged(ICharacter character);
    }
}
