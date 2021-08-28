using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoCC
{
    public interface INeoCharacterControllerHitHandler
    {
        bool enabled { get; set; }

        void OnNeoCharacterControllerHit(NeoCharacterControllerHit hit);
    }
}
