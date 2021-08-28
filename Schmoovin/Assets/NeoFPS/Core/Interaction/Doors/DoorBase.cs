using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Events;

namespace NeoFPS
{
    public abstract class DoorBase : MonoBehaviour, INeoSerializableComponent
    {
        private static readonly NeoSerializationKey k_NormalisedOpenKey = new NeoSerializationKey("normalisedOpen");
        private static readonly NeoSerializationKey k_StateKey = new NeoSerializationKey("state");
        private static readonly NeoSerializationKey k_LockedKey = new NeoSerializationKey("locked");

        public event UnityAction onIsLockedChanged;

        public virtual bool reversible
        {
            get { return false; }
        }

        public abstract float normalisedOpen
        {
            get;
            protected set;
        }

        public DoorState state
        {
            get;
            protected set;
        }

        public bool isLocked
        {
            get;
            private set;
        }

        public void Open(bool reverse = false)
        {
            if (state == DoorState.Closed)
            {
                if (isLocked)
                    OnTryOpenLocked();
                else
                    OnOpen(reverse);
            }
            else
                OnOpen(reverse);
        }

        public void Close()
        {
            if (state != DoorState.Closed)
                OnClose();
        }

        protected abstract void OnOpen(bool reverse);
        protected abstract void OnClose();
        protected abstract void OnTryOpenLocked();

        public void Lock()
        {
            if (!isLocked)
            {
                isLocked = true;
                SetIsLocked();
            }
        }

        public void LockSilent()
        {
            if (!isLocked)
                isLocked = true;
        }

        public void Unlock()
        {
            if (isLocked)
            {
                isLocked = false;
                SetIsLocked();
            }
        }

        public void UnlockSilent()
        {
            if (isLocked)
                isLocked = false;
        }

        void SetIsLocked ()
        {
            OnLockedStateChanged(isLocked);
            if (onIsLockedChanged != null)
                onIsLockedChanged();
        }

        protected abstract void OnLockedStateChanged(bool locked);

        public virtual bool IsTransformInFrontOfDoor(Transform t)
        {
            return true;
        }

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_NormalisedOpenKey, normalisedOpen);
            writer.WriteValue(k_StateKey, (int)state);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            float floatResult = 0f;
            if (reader.TryReadValue(k_NormalisedOpenKey, out floatResult, normalisedOpen))
                normalisedOpen = floatResult;

            int intResult = 0;
            if (reader.TryReadValue(k_StateKey, out intResult, (int)state))
                state = (DoorState)intResult;

            bool locked;
            if (reader.TryReadValue(k_LockedKey, out locked, isLocked))
                isLocked = locked;
        }
    }
}