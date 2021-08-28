using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
	public abstract class BaseEjectorBehaviour : BaseFirearmModuleBehaviour, IEjector, IFirearmModuleValidity, INeoSerializableComponent
    {
		protected virtual void OnEnable ()
		{
			firearm.SetEjector (this);
		}

		public abstract bool ejectOnFire { get; }

		public abstract void Eject ();

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