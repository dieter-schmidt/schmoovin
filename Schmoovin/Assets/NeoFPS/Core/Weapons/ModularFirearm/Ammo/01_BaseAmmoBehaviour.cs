using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
	public abstract class BaseAmmoBehaviour : BaseFirearmModuleBehaviour, IAmmo, IFirearmModuleValidity, INeoSerializableComponent
    { 
		public event UnityAction<IModularFirearm, int> onCurrentAmmoChange;
        
		protected virtual void OnEnable ()
		{
			firearm.SetAmmo (this);
		}

		public abstract string printableName { get; }

		public abstract IAmmoEffect effect { get; }

		public abstract int maxAmmo { get; }
		public abstract int currentAmmo { get; }

		public bool available
		{
			get { return currentAmmo > 0; }
		}

		public bool atMaximum
		{
			get { return currentAmmo >= maxAmmo; }
		}

		public abstract void DecrementAmmo (int amount);
		public abstract void IncrementAmmo (int amount);

		protected void SendAmmoChangeEvent ()
		{
			if (onCurrentAmmoChange != null)
				onCurrentAmmoChange (firearm, currentAmmo);
        }

        public virtual bool isModuleValid
        {
            get { return true; }
        }

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
        }
    }
}