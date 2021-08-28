using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-incrementalreloader.html")]
    public class IncrementalReloader : BaseReloaderBehaviour
    {
        [SerializeField, Tooltip("Can the reload be interrupted by firing (ending at the current increment).")]
        private bool m_CanInterrupt = true;

        [SerializeField, Tooltip("The number of rounds to load into the gun each increment.")]
        private int m_RoundsPerIncrement = 1;

        [SerializeField, Tooltip("If true, requires an external trigger such as a script or animation event to increment and complete the reload. If false, uses a timer.")]
        private bool m_UseExternalTriggers = false;

        [SerializeField, Tooltip("The time from starting the reload animations to adding the first shell.")]
        private float m_ReloadStartDuration = 1f;

        [SerializeField, Tooltip("The time between shells in the reload animation.")]
        private float m_ReloadIncrementDuration = 1f;

        //[SerializeField, Tooltip("The time from adding a shell to the end of the increment animation clip. Used when cancelling as timer will act as though full increment, but animator will act as though completed.")]
        //private float m_ReloadIncrementTail = 0.5f;

        [SerializeField, Tooltip("The time from the last shell being added to the reload animation completing.")]
        private float m_ReloadEndDuration = 1f;

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Trigger, true, true), Tooltip("The animator controller trigger key for the reload animation.")]
        private string m_ReloadAnimTrigger = "Reload";

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Int, true, true), Tooltip("The animator controller parameter key for the reload count.")]
        private string m_ReloadAnimCountProp = "ReloadCount";

        [Header("Audio")]

        [SerializeField, Tooltip("The audio clip to play when the reload starts.")]
        private AudioClip m_ReloadAudioStart = null;

        [SerializeField, Tooltip("The audio clip to play during an increment of the reload.")]
        private AudioClip m_ReloadAudioIncrement = null;

        [SerializeField, Tooltip("The audio clip to play when the reload ends.")]
        private AudioClip m_ReloadAudioEnd = null;

        [SerializeField, Range(0f, 1f), Tooltip("The volume that reloading sounds are played at.")]
        private float m_Volume = 1f;

        private bool m_WaitingOnExternalTrigger = false;
        private bool m_Interrupted = false;
        private int m_ReloadAnimTriggerHash = 0;
        private int m_ReloadCountHash = -1;
        private float m_ReloadTimeout = 0f;

        private int m_ReloadCount = 0;
		public int reloadCount
		{
			get { return m_ReloadCount; }
			set
			{
                if (m_ReloadCount == value)
                    return;

				m_ReloadCount = value;
				if (m_ReloadCount < 0)
					m_ReloadCount = 0;

				// Set animator property
				if (firearm.animator != null && gameObject.activeInHierarchy && m_ReloadCountHash != -1)
					firearm.animator.SetInteger (m_ReloadCountHash, m_ReloadCount);
			}
        }

        public override bool interruptable
        {
            get { return m_CanInterrupt; }
        }
        
        public override FirearmDelayType reloadDelayType
		{
            get
            {
                if (m_UseExternalTriggers)
                    return FirearmDelayType.ExternalTrigger;
                else
                    return FirearmDelayType.ElapsedTime;
            }
		}

		private class WaitForReload : Waitable
		{
			// Store the reloadable owner
			readonly IncrementalReloader m_Owner;
			public WaitForReload (IncrementalReloader owner)	{ m_Owner = owner; }

			// Check for timeout
			protected override bool CheckComplete () { return !m_Owner.m_WaitingOnExternalTrigger && m_Owner.m_ReloadTimeout <= 0f; }
		}

		WaitForReload m_WaitForReload = null;
        private WaitForReload waitForReload
        {
            get
            {
                if (m_WaitForReload == null)
                    m_WaitForReload = new WaitForReload(this);
                return m_WaitForReload;
            }
        }

        public override bool isReloading
        {
            get { return !waitForReload.isComplete; }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_ReloadStartDuration < 0.1f)
                m_ReloadStartDuration = 0.1f;
            if (m_ReloadIncrementDuration < 0.1f)
                m_ReloadIncrementDuration = 0.1f;
            if (m_ReloadEndDuration < 0f)
                m_ReloadEndDuration = 0f;
            //m_ReloadIncrementTail = Mathf.Clamp(m_ReloadIncrementTail, 0f, m_ReloadIncrementDuration);
        }
#endif

        protected override void Start ()
		{
			m_WaitForReload = new WaitForReload(this);

            // Get animator controller property key hashes
			if (m_ReloadAnimTrigger == string.Empty)
				m_ReloadAnimTriggerHash = -1;
			else
				m_ReloadAnimTriggerHash = Animator.StringToHash (m_ReloadAnimTrigger);
            if (m_ReloadAnimCountProp == string.Empty)
                m_ReloadCountHash = -1;
            else
                m_ReloadCountHash = Animator.StringToHash(m_ReloadAnimCountProp);

			base.Start ();
		}

        protected override void Update ()
		{
            if (m_UseExternalTriggers)
                return;

            // Iterate reload
            if (m_ReloadTimeout > 0f)
            {
                m_ReloadTimeout -= Time.deltaTime;
                if (m_ReloadTimeout <= 0f)
                {
                    if (reloadCount > 0)
                    {
                        ReloadPartialInternal(m_RoundsPerIncrement);
                    }
                    else
                    {
                        m_ReloadTimeout = 0f;

                        // Fire completed event
                        SendReloadCompletedEvent();
                    }
                }
            }
		}

        protected override void OnEnable()
        {
            base.OnEnable();

            // Set animator property
            if (firearm.animator != null && m_ReloadCountHash != -1)
                firearm.animator.SetInteger(m_ReloadCountHash, m_ReloadCount);
        }

        protected override void OnDisable ()
		{
			reloadCount = 0;
            m_Interrupted = false;
            m_ReloadTimeout = 0f;
            m_WaitingOnExternalTrigger = false;
			base.OnDisable ();
		}

        public override void ManualReloadPartial ()
		{
            if (!m_WaitingOnExternalTrigger)
            {
                Debug.LogError("Attempting to manually signal weapon partial reload while reloader is not set to use external triggers");
                return;
            }
            ReloadPartialInternal(m_RoundsPerIncrement);
		}

        void ReloadPartialInternal (int count)
        {
            // Play audio
            if (m_ReloadAudioIncrement != null)
                firearm.PlaySound(m_ReloadAudioIncrement, m_Volume);

            // Record the old magazine count (to check against)
            int oldMagazineSize = currentMagazine;

            // Cap the increment to available ammo
            if (count > firearm.ammo.currentAmmo)
                count = firearm.ammo.currentAmmo;

            // Get the new magazine count (clamped in property)
            currentMagazine += count;

            // Decrement the ammo
            int reloaded = currentMagazine - oldMagazineSize;
            firearm.ammo.DecrementAmmo (reloaded);

            // interrupt
            if (m_Interrupted)
                reloadCount = 0;
            else
                reloadCount -= reloaded;

            // Check if that was the last shell
            if (reloadCount == 0 || !firearm.ammo.available)
            {
                // Play audio
                if (m_ReloadAudioEnd != null)
                    Invoke("PlayReloadEndSound", m_ReloadTimeout + m_ReloadIncrementDuration);

                if (!m_UseExternalTriggers)
                    m_ReloadTimeout += m_ReloadEndDuration;
            }
            else
            {
                if (!m_UseExternalTriggers)
                    m_ReloadTimeout += m_ReloadIncrementDuration;
            }
        }

        void PlayReloadEndSound()
        {
            firearm.PlaySound(m_ReloadAudioEnd, m_Volume);
        }

        void ReloadFullInternal ()
		{
			// Record the old magazine count (to check against)
			int oldMagazineSize = currentMagazine;

			// Get the new magazine count (clamped in property)
			currentMagazine += firearm.ammo.currentAmmo;

			// Decrement the ammo
			firearm.ammo.DecrementAmmo (currentMagazine - oldMagazineSize);

            m_WaitingOnExternalTrigger = false;
            m_ReloadTimeout = 0f;
        }

		public override void ManualReloadComplete ()
		{
			if (!m_WaitingOnExternalTrigger)
            {
                Debug.LogError("Attempting to manually signal weapon reload completed while reloader is not set to use external triggers");
                return;
            }

            reloadCount = 0;
			m_WaitingOnExternalTrigger = false;
            m_ReloadTimeout = 0f;
            m_Interrupted = false;
        }
        
        public override void Interrupt ()
        {
            m_Interrupted = true;
        }

        public override Waitable Reload ()
		{
			if (!full && firearm.ammo.available)
            {
                m_Interrupted = false;

                // Fire started event
                SendReloadStartedEvent ();

                // Timer or trigger?
                if (m_UseExternalTriggers)
                    m_WaitingOnExternalTrigger = true;
                else
                    m_ReloadTimeout = m_ReloadStartDuration;

                // Set total reload count
                if (reloadCount == 0)
                {
                    reloadCount = Mathf.Min(magazineSize - currentMagazine, firearm.ammo.currentAmmo);
                    // Trigger animation
                    if (firearm.animator != null && m_ReloadAnimTriggerHash != -1)
                        firearm.animator.SetTrigger(m_ReloadAnimTriggerHash);
                }

                // Play audio
                if (m_ReloadAudioStart != null)
                    firearm.PlaySound(m_ReloadAudioStart, m_Volume);
			}
			return waitForReload;
		}

        #region INeoSerializableComponent implementation

        private static readonly NeoSerializationKey k_TimeoutKey = new NeoSerializationKey("timeout");
        private static readonly NeoSerializationKey k_ReloadCountKey = new NeoSerializationKey("reloadCount");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (saveMode == SaveMode.Default)
            {
                writer.WriteValue(k_TimeoutKey, m_ReloadTimeout);

                if (m_ReloadCountHash != -1)
                    writer.WriteValue(k_ReloadCountKey, m_ReloadCount);
            }
            base.WriteProperties(writer, nsgo, saveMode);
        }
       
        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_TimeoutKey, out m_ReloadTimeout, m_ReloadTimeout);

            int count;
            if (reader.TryReadValue(k_ReloadCountKey, out count, m_ReloadCount) && count > 0)
            {
                m_ReloadCountHash = Animator.StringToHash(m_ReloadAnimCountProp);
                reloadCount = count;
            }

            base.ReadProperties(reader, nsgo);
        }

        #endregion
    }
}