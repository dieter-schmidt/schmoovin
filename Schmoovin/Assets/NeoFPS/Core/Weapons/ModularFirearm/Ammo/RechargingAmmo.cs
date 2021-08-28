using UnityEngine;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-rechargingammo.html")]
	public class RechargingAmmo : BaseAmmoBehaviour
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

        [SerializeField, Tooltip("The time in seconds between recharge steps.")]
        private float m_RechargeSpacing = 3f;

        [SerializeField, Tooltip("The amount of ammo to add each recharge step.")]
        private int m_RechargeAmount = 100;

        [SerializeField, Tooltip("Should the recharge timer reset each time the ammo quantity changes. If not, the recharge timer will start as soon as the ammo dips below max and stop when it is filled to maximum again.")]
        private bool m_ResetOnChange = false;

        private int m_CurrentAmmo = 0;
        private float m_RechargeTimer = 0f;

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
            m_RechargeSpacing = Mathf.Clamp(m_RechargeSpacing, 0.01f, 100f);
            m_RechargeAmount = Mathf.Clamp(m_RechargeAmount, 1, m_MaxAmmo);
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

        void FixedUpdate()
        {
            if (m_RechargeTimer > 0f)
            {
                m_RechargeTimer -= Time.deltaTime;
                if (m_RechargeTimer < 0f)
                {
                    m_RechargeTimer += m_RechargeSpacing;
                    IncrementAmmo(m_RechargeAmount);
                }
            }
        }

        public override void DecrementAmmo (int amount)
		{
			m_CurrentAmmo = Mathf.Clamp (m_CurrentAmmo - amount, 0, m_MaxAmmo);

            if (m_CurrentAmmo != m_MaxAmmo)
            {
                if (m_RechargeTimer == 0f || m_ResetOnChange)
                    m_RechargeTimer = m_RechargeSpacing;
            }

            SendAmmoChangeEvent ();
		}

		public override void IncrementAmmo (int amount)
		{
			m_CurrentAmmo = Mathf.Clamp (m_CurrentAmmo + amount, 0, m_MaxAmmo);

            if (m_CurrentAmmo == m_MaxAmmo)
                m_RechargeTimer = 0f;

			SendAmmoChangeEvent ();
		}

        private static readonly NeoSerializationKey k_CountKey = new NeoSerializationKey("count");
        private static readonly NeoSerializationKey k_RechargeKey = new NeoSerializationKey("recharge");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);
            writer.WriteValue(k_CountKey, m_CurrentAmmo);
            writer.WriteValue(k_RechargeKey, m_RechargeTimer);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);
            if (reader.TryReadValue(k_CountKey, out m_CurrentAmmo, m_CurrentAmmo))
            {
                m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo, 0, m_MaxAmmo);
                SendAmmoChangeEvent();
            }
            reader.TryReadValue(k_RechargeKey, out m_RechargeTimer, m_RechargeTimer);
        }
    }
}