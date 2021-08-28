using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS
{
    public abstract class PlayerCharacterHudBase : MonoBehaviour, IPlayerCharacterSubscriber
    {
        IPlayerCharacterWatcher m_Watcher = null;

        protected virtual void Awake()
        {
            m_Watcher = GetComponentInParent<IPlayerCharacterWatcher>();
            if (m_Watcher == null)
                Debug.LogError("Player character HUD items require a component that implements IPlayerCharacterWatcher in the parent heirarchy", gameObject);
        }

        protected virtual void Start()
        {
            if (m_Watcher != null)
                m_Watcher.AttachSubscriber(this);
        }

        protected virtual void OnDestroy()
        {
            if (m_Watcher != null)
                m_Watcher.ReleaseSubscriber(this);
        }
        
        public abstract void OnPlayerCharacterChanged(ICharacter character);
    }
}
