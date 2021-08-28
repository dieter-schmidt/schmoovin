using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-thrownweaponprojectile.html")]
    [RequireComponent(typeof(PooledObject))]
    public abstract class ThrownWeaponProjectile : MonoBehaviour, INeoSerializableComponent
    {
        protected PooledObject pooledObject
        {
            get;
            private set;
        }

        protected IDamageSource damageSource
        {
            get;
            set;
        }

        protected virtual void Awake ()
        {
            pooledObject = GetComponent<PooledObject>();
        }

        public virtual void Throw (Vector3 velocity, IDamageSource source)
        {
            damageSource = source;
        }

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
        }
    }
}
