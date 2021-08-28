using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoFPS.ModularFirearms
{
    public interface IUseCameraAim
    {
        UseCameraAim useCameraAim { get; set; }
    }

    public enum UseCameraAim
    {
        HipFireOnly,
        HipAndAimDownSights,
        AimDownSightsOnly,
        Never
    }
}
