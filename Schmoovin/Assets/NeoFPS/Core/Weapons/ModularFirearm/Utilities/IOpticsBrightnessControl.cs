
namespace NeoFPS
{
    public interface IOpticsBrightnessControl
    {
        void SetBrightness(int index);
        void IncrementBrightness(bool looping = false);
        void DecrementBrightness(bool looping = false);
    }
}
