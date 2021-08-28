using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-infiniteammo.html")]
    public class InfiniteAmmo : BaseFirearmModuleBehaviour, IAmmo, IFirearmModuleValidity, INeoSerializableComponent
    {
        [Header("Ammo Settings")]

        [SerializeField, Tooltip("The name to show on the HUD.")]
        private string m_PrintableName = string.Empty;

        [SerializeField, Tooltip("The ammo quantity available to any reloaders - must be >= to the magazine size.")]
        private int m_FixedSize = 999;

        [SerializeField, ComponentOnObject(typeof(BaseAmmoEffect)), Tooltip("The effect of the ammo when it hits something.")]
        private BaseAmmoEffect m_Effect = null;

        public string printableName
        {
            get { return m_PrintableName; }
        }

        public IAmmoEffect effect
        {
            get { return m_Effect; }
        }

        public int maxAmmo { get { return m_FixedSize; } }
        public int currentAmmo { get { return m_FixedSize; } }
        public bool available { get { return true; } }
        public bool atMaximum { get { return true; } }
        
        #pragma warning disable 0067

        public event UnityAction<IModularFirearm, int> onCurrentAmmoChange;

        #pragma warning restore 0067

        public bool isModuleValid
        {
            get { return m_Effect != null; }
        }

        void OnValidate()
        {
            if (m_FixedSize < 1)
                m_FixedSize = 1;
        }

        void OnEnable ()
        {
            firearm.SetAmmo(this);
        }

        public void IncrementAmmo(int amount) { }
        public void DecrementAmmo(int amount) { }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            // Having this means the enabled state will be saved
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
        }
    }
}
