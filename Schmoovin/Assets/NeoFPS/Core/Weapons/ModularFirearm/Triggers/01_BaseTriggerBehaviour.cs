using UnityEngine;
using System;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
	public abstract class BaseTriggerBehaviour : BaseFirearmModuleBehaviour, ITrigger, IFirearmModuleValidity, INeoSerializableComponent
    {
        private static readonly NeoSerializationKey k_BlockedKey = new NeoSerializationKey("blocked");

        private bool m_Blocked = false;
		public bool blocked
		{
			get { return m_Blocked; }
			set { OnSetBlocked (value); }
		}

        virtual public bool cancelOnReload
        {
            get { return false; }
        }
        
		void FixedUpdate ()
		{
			if (!blocked)
				FixedTriggerUpdate ();
		}

		protected virtual void OnEnable ()
		{
			firearm.SetTrigger (this);
		}

		protected virtual void OnSetBlocked (bool to)
		{
			m_Blocked = to;
		}

		public abstract bool pressed { get; }
		public virtual void Press ()
        {
            if (onStateChanged != null)
                onStateChanged(true);
        }

		public virtual void Release ()
        {
            if (onStateChanged != null)
                onStateChanged(false);
        }

        public virtual void Cancel ()
        {
            if (pressed)
                Release();
        }

		protected abstract void FixedTriggerUpdate ();

		public event UnityAction onShoot;
        public event UnityAction<bool> onStateChanged;
        public event UnityAction<bool> onShootContinuousChanged;

        protected void Shoot ()
		{
			if (onShoot != null)
				onShoot ();
        }

		protected void StartShootContinuous()
        {
			if (onShootContinuousChanged != null)
				onShootContinuousChanged(true);
		}

		protected void StopShootContinuous()
		{
			if (onShootContinuousChanged != null)
				onShootContinuousChanged(false);
		}
        
        public virtual bool isModuleValid
        {
            get { return true; }
        }

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (saveMode == SaveMode.Default)
                writer.WriteValue(k_BlockedKey, m_Blocked);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_BlockedKey, out m_Blocked, m_Blocked);
        }
    }
}