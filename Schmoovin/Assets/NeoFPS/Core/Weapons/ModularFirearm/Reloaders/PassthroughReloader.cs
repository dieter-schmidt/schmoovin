using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-passthroughreloader.html")]
    public class PassthroughReloader : BaseFirearmModuleBehaviour, IReloader, IFirearmModuleValidity, INeoSerializableComponent
    {
        [Header ("Reloader Settings")]

        [SerializeField, Tooltip("The value it should report for its magazine size if any modules query it.")]
        private int m_MagazineSize = 1;

        public bool isModuleValid
        {
            get { return true; }
        }
        
        public int currentMagazine
        {
            get
            {
                if (firearm.ammo != null)
                    return Mathf.Min(m_MagazineSize, firearm.ammo.currentAmmo);
                else
                    return 0;
            }
            set { }
        }

        public int startingMagazine
        {
            get { return m_MagazineSize; }
            set { }
        }

        public bool empty { get { return !firearm.ammo.available; } }

        public bool full { get { return firearm.ammo.available; } }

        public bool canReload { get { return false; } }

        public int magazineSize { get { return 1; } }

        public bool isReloading { get { return false; } }

        public bool interruptable { get { return false; } }

        public FirearmDelayType reloadDelayType { get { return FirearmDelayType.None; } }

#pragma warning disable 0067

        public event UnityAction<IModularFirearm, int> onCurrentMagazineChange;
        public event UnityAction<IModularFirearm> onReloadStart;
        public event UnityAction<IModularFirearm> onReloadComplete;

#pragma warning restore 0067

        public void DecrementMag(int amount)
        {
            firearm.ammo.DecrementAmmo(1);
        }

        void OnEnable()
        {
            firearm.SetReloader(this);
        }

        public void Interrupt() { }
        public void ManualReloadComplete() { }
        public void ManualReloadPartial() { }
        public Waitable Reload() { return null; }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            // Having this means the enabled state will be saved
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
        }
    }
}