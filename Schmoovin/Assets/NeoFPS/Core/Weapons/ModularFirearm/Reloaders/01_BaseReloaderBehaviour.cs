using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
	public abstract class BaseReloaderBehaviour : BaseFirearmModuleBehaviour, IReloader, IFirearmModuleValidity, INeoSerializableComponent
    {
        [Header ("Reloader Settings")]

        [SerializeField, Delayed, Tooltip("The number of rounds that can be fit in the magazine at once.")]
		private int m_MagazineSize = 1;

        [SerializeField, Delayed, Tooltip("The number of rounds in the magazine on initialisation.")]
		private int m_StartingMagazine = 1;

        public event UnityAction<IModularFirearm, int> onCurrentMagazineChange;
		public event UnityAction<IModularFirearm> onReloadStart;
		public event UnityAction<IModularFirearm> onReloadComplete;

        private int m_CurrentMagazine = -1;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (m_MagazineSize < 1)
                m_MagazineSize = 1;
            m_StartingMagazine = Mathf.Clamp(m_StartingMagazine, 0, m_MagazineSize);
        }
#endif
        
		protected virtual void Start ()
		{
            // Set starting size (check if changed by serialization first)
            if (currentMagazine == -1)
                currentMagazine = startingMagazine;
		}

		protected virtual void Update ()
		{
		}

		protected virtual void OnEnable ()
		{
			firearm.SetReloader (this);
		}
		protected virtual void OnDisable ()
		{
		}

		public bool empty { get { return currentMagazine == 0; } }
		public bool full { get { return currentMagazine == magazineSize; } }
		public bool canReload { get { return !full && (firearm.ammo == null || firearm.ammo.available); } }

		public int magazineSize
		{
			get { return m_MagazineSize; }
			protected set
			{
				m_MagazineSize = value;
				if (m_CurrentMagazine > m_MagazineSize)
					currentMagazine = m_MagazineSize;
			}
		}

		public int startingMagazine
		{
			get { return m_StartingMagazine; }
            set { m_StartingMagazine = Mathf.Clamp(value, 0, m_MagazineSize); }
		}

		public int currentMagazine
		{
			get
            {
                if (m_CurrentMagazine == -1)
                    return m_StartingMagazine;
                else
                    return m_CurrentMagazine;
            }
			set
			{
                int oldValue = m_CurrentMagazine;
				m_CurrentMagazine = Mathf.Clamp (value, 0, m_MagazineSize);
                OnCurrentMagazineChange(oldValue, m_CurrentMagazine);
            }
		}

        public virtual void DecrementMag (int amount)
		{
			currentMagazine -= amount;
		}

		public abstract bool isReloading { get; }
		public abstract Waitable Reload ();

        public abstract FirearmDelayType reloadDelayType { get; }
        public virtual void ManualReloadPartial() { }
		public abstract void ManualReloadComplete ();

        public virtual bool interruptable
        {
            get { return false; }
        }

        public virtual void Interrupt()
        { }

        protected virtual void OnCurrentMagazineChange(int from, int to)
        {
            if (onCurrentMagazineChange != null)
                onCurrentMagazineChange(firearm, m_CurrentMagazine);
        }

        protected void SendReloadStartedEvent ()
		{
			if (onReloadStart != null)
				onReloadStart (firearm);
		}
		protected void SendReloadCompletedEvent ()
		{
			// Fire completed event
			if (onReloadComplete != null)
				onReloadComplete (firearm);
        }

        public virtual bool isModuleValid
        {
            get { return true; }
        }

        private static readonly NeoSerializationKey k_MagSizeKey = new NeoSerializationKey("magazineSize");
        private static readonly NeoSerializationKey k_StartingMagKey = new NeoSerializationKey("startingMag");
        private static readonly NeoSerializationKey k_CurrentMagKey = new NeoSerializationKey("currentMag");

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            // Write magazine info
            writer.WriteValue(k_MagSizeKey, magazineSize);
            writer.WriteValue(k_StartingMagKey, startingMagazine);
            writer.WriteValue(k_CurrentMagKey, currentMagazine);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            // Read magazine info
            int intResult = 0;
            if (reader.TryReadValue(k_MagSizeKey, out intResult, m_MagazineSize))
                magazineSize = intResult;
            if (reader.TryReadValue(k_StartingMagKey, out intResult, m_StartingMagazine))
                startingMagazine = intResult;
            if (reader.TryReadValue(k_CurrentMagKey, out intResult, m_CurrentMagazine))
                currentMagazine = intResult;
        }
    }
}
