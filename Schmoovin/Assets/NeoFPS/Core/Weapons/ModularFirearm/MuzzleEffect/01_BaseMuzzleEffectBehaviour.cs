using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
	public abstract class BaseMuzzleEffectBehaviour : BaseFirearmModuleBehaviour, IMuzzleEffect, IFirearmModuleValidity, INeoSerializableComponent
    {
		protected virtual void OnEnable ()
		{
			firearm.SetMuzzleEffect (this);
		}

		public abstract void Fire ();
		public abstract void StopContinuous ();
		public abstract void FireContinuous ();

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