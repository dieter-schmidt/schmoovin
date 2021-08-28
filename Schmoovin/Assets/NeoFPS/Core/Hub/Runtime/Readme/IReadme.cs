using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoFPS.Hub
{
    public interface IReadme
    {
        ReadmeHeader header { get; }
        ReadmeSection[] sections { get; }
    }
}
