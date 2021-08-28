using UnityEngine;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-customammo.html")]
	public class CustomAmmo : BaseAmmoBehaviour
    {
        [Header("Ammo Settings")]

        [SerializeField, Tooltip("The name to show on the HUD.")]
        private string m_PrintableName = string.Empty;

		[SerializeField, ComponentOnObject(typeof(BaseAmmoEffect)), Tooltip("The effect of the ammo when it hits something.")]
		private BaseAmmoEffect m_Effect = null;

        [SerializeField, Delayed, Tooltip("The amount of ammo the weapon starts with.")]
		private int m_StartingAmmo = 100;

		[SerializeField, Delayed, Tooltip("The maximum amount of ammo the weapon can carry.")]
		private int m_MaxAmmo = 100;

        private int m_CurrentAmmo = 0;

		public override string printableName { get { return m_PrintableName; } }

		public override IAmmoEffect effect
		{
			get { return m_Effect; }
		}

		public override int maxAmmo { get { return m_MaxAmmo; } }
		public override int currentAmmo { get { return m_CurrentAmmo; } }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (m_Effect == null)
                m_Effect = GetComponent<BaseAmmoEffect>();

            if (m_MaxAmmo < 0)
                m_MaxAmmo = 0;
            m_StartingAmmo = Mathf.Clamp(m_StartingAmmo, 0, m_MaxAmmo);
        }
#endif

        public override bool isModuleValid
        {
            get { return m_Effect != null; }
        }

        void Start ()
		{
			m_CurrentAmmo = m_StartingAmmo;
            SendAmmoChangeEvent();
        }
        
		public override void DecrementAmmo (int amount)
		{
			m_CurrentAmmo = Mathf.Clamp (m_CurrentAmmo - amount, 0, m_MaxAmmo);
			SendAmmoChangeEvent ();
		}

		public override void IncrementAmmo (int amount)
		{
			m_CurrentAmmo = Mathf.Clamp (m_CurrentAmmo + amount, 0, m_MaxAmmo);
			SendAmmoChangeEvent ();
		}

        private static readonly NeoSerializationKey k_CountKey = new NeoSerializationKey("count");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);
            writer.WriteValue(k_CountKey, m_CurrentAmmo);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);
            if (reader.TryReadValue(k_CountKey, out m_CurrentAmmo, m_CurrentAmmo))
            {
                m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo, 0, m_MaxAmmo);
                SendAmmoChangeEvent();
            }
        }
    }
}