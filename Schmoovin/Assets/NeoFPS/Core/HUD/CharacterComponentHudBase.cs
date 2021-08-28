using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS
{
    public abstract class CharacterComponentHudBase<ComponentType> : MonoBehaviour, IPlayerCharacterSubscriber where ComponentType : class
    {
        IPlayerCharacterWatcher m_Watcher = null;

        protected virtual bool isValid
        {
            get { return true; }
        }

        protected ComponentType targetComponent
        {
            get;
            private set;
        }

        protected virtual void Awake()
        {
            if (isValid)
            {
                m_Watcher = GetComponentInParent<IPlayerCharacterWatcher>();
                if (m_Watcher == null)
                    Debug.LogError("Player inventory HUD items require a component that implements IPlayerCharacterWatcher in the parent heirarchy", gameObject);
            }
            else
                gameObject.SetActive(false);
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
            OnPlayerCharacterChanged(null);
        }

        public void OnPlayerCharacterChanged(ICharacter c)
        {
            if (targetComponent != null)
                DetachFromComponent(targetComponent);

            if (c as Component != null)
                targetComponent = c.GetComponent<ComponentType>();
            else
                targetComponent = null;

            if (targetComponent != null)
                AttachToComponent(targetComponent);

            ResetUI();
        }

        protected abstract void AttachToComponent(ComponentType target);
        protected abstract void DetachFromComponent(ComponentType target);
        protected abstract void ResetUI();
    }
}
