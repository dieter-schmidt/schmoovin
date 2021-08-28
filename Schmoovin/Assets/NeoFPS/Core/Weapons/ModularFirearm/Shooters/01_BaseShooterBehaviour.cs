using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
	public abstract class BaseShooterBehaviour : BaseFirearmModuleBehaviour, IShooter, IFirearmModuleValidity, INeoSerializableComponent
    {
		public event UnityAction<IModularFirearm> onShoot;

		protected virtual void OnEnable ()
		{
			firearm.SetShooter (this);
		}

		public virtual void Shoot (float accuracy, IAmmoEffect effect)
		{
			SendOnShootEvent ();
		}

		protected void SendOnShootEvent ()
		{
			if (onShoot != null)
				onShoot (firearm);
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