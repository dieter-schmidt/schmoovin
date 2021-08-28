using UnityEngine.Events;

namespace NeoFPS
{
    public interface IWieldable
    {
        ICharacter wielder { get; }
        void Select();
        void DeselectInstant();
        Waitable Deselect();

        event UnityAction<ICharacter> onWielderChanged;

        T GetComponent<T>();
    }
}