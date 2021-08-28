using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-simplereloader.html")]
	public class SimpleReloader : BaseReloaderBehaviour
	{
		[SerializeField, Tooltip("The delay type between starting and completing a reload.")]
		private FirearmDelayType m_ReloadDelayType = FirearmDelayType.ElapsedTime;

		[SerializeField, Tooltip("The time taken to reload.")]
		private float m_ReloadDuration = 2f;
        
        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Trigger, true, true), Tooltip("The animator controller trigger key for the reload animation.")]
		private string m_ReloadAnimTrigger = "Reload";

		[SerializeField, Tooltip("The audio clip to play while reloading.")]
        private AudioClip m_ReloadAudio = null;

		[SerializeField, Range(0f, 1f), Tooltip("The volume that reloading sounds are played at.")]
		private float m_Volume = 1f;

		private bool m_WaitingOnExternalTrigger = false;
		private int m_ReloadAnimTriggerHash = 0;
        private float m_ReloadTimeout = 0f;

		private class WaitForReload : Waitable
		{
			// Store the reloadable owner
			readonly SimpleReloader m_Owner;
			public WaitForReload (SimpleReloader owner)	{ m_Owner = owner; }

			// Check for timeout
			protected override bool CheckComplete () { return !m_Owner.m_WaitingOnExternalTrigger && m_Owner.m_ReloadTimeout == 0f; }
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

        public override FirearmDelayType reloadDelayType
		{
			get { return m_ReloadDelayType; }
		}

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_ReloadDuration < 0f)
                m_ReloadDuration = 0f;
        }
#endif

        protected override void Start ()
		{
			m_WaitForReload = new WaitForReload(this);
			if (m_ReloadAnimTrigger == string.Empty)
				m_ReloadAnimTriggerHash = -1;
			else
				m_ReloadAnimTriggerHash = Animator.StringToHash (m_ReloadAnimTrigger);
			base.Start ();
		}

		protected override void Update ()
		{			
			if (m_ReloadTimeout > 0f)
			{
				m_ReloadTimeout -= Time.deltaTime;
				if (m_ReloadTimeout <= 0f)
				{
					ReloadInternal ();
					m_ReloadTimeout = 0f;
				}
			}
		}

        protected override void OnDisable()
        {
            base.OnDisable();
            m_ReloadTimeout = 0f;
            m_WaitingOnExternalTrigger = false;
        }

        void ReloadInternal ()
		{
			// Record the old magazine count (to check against)
			int oldMagazineSize = currentMagazine;
			// Get the new magazine count (clamped in property)
			currentMagazine += firearm.ammo.currentAmmo;
			// Decrement the ammo
			firearm.ammo.DecrementAmmo (currentMagazine - oldMagazineSize);
			// Fire completed event
			SendReloadCompletedEvent ();
		}

		public override void ManualReloadComplete ()
		{		
			if (reloadDelayType != FirearmDelayType.ExternalTrigger)
			{
				Debug.LogError ("Attempting to manually signal weapon reloaded when delay type is not set to custom");
				return;
			}
			if (!m_WaitingOnExternalTrigger)
			{
				Debug.LogError ("Attempting to manually signal weapon reloaded, not expected");
				return;
			}
			// Reload and reset trigger
			ReloadInternal ();
			m_WaitingOnExternalTrigger = false;
		}

		#region BaseReloaderBehaviour implementation

		public override bool isReloading
		{
			get { return !waitForReload.isComplete; }
		}

		public override Waitable Reload ()
		{
			if (waitForReload.isComplete && !full && firearm.ammo.available)
			{
				switch (reloadDelayType)
				{
					case FirearmDelayType.None:
						SendReloadStartedEvent ();
						ReloadInternal ();
						break;
					case FirearmDelayType.ElapsedTime:
						if (m_ReloadTimeout == 0f)
						{
							// Fire started event
							SendReloadStartedEvent ();
							// Set timer
							m_ReloadTimeout = m_ReloadDuration;
							// Trigger animation
							if (firearm.animator != null && m_ReloadAnimTriggerHash != -1)
								firearm.animator.SetTrigger (m_ReloadAnimTriggerHash);
						}
						break;
					case FirearmDelayType.ExternalTrigger:
						if (!m_WaitingOnExternalTrigger)
						{
							// Fire started event
							SendReloadStartedEvent ();
							// Set trigger
							m_WaitingOnExternalTrigger = true;
							// Trigger animation
							if (firearm.animator != null && m_ReloadAnimTriggerHash != -1)
								firearm.animator.SetTrigger (m_ReloadAnimTriggerHash);
                        }
						break;
                }
                // Play audio
                if (m_ReloadAudio != null)
                    firearm.PlaySound(m_ReloadAudio, m_Volume);
			}
			return waitForReload;
		}

        private static readonly NeoSerializationKey k_TimeoutKey = new NeoSerializationKey("timeout");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (saveMode == SaveMode.Default)
                writer.WriteValue(k_TimeoutKey, m_ReloadTimeout);
            base.WriteProperties(writer, nsgo, saveMode);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_TimeoutKey, out m_ReloadTimeout, m_ReloadTimeout);
            base.ReadProperties(reader, nsgo);
        }

        #endregion
    }
}