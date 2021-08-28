using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoFPS.ModularFirearms;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/samplesref-mb-turretseeker.html")]
    public class TurretSeeker : CameraSeeker, IModularFirearm, IDamageSource
	{
		#region IDamageSource implementation

		private DamageFilter m_OutDamageFilter = DamageFilter.AllDamageAllTeams;
		public DamageFilter outDamageFilter
		{
			get
			{
				return m_OutDamageFilter;
			}
			set
			{
				m_OutDamageFilter = value;
			}
		}

		public IController controller
		{
			get
			{
				return null;
			}
		}

		public Transform damageSourceTransform
		{
			get
			{
				return transform;
			}
		}

		public string description
		{
			get
			{
				return name;
			}
		}

		#endregion

		#region IModularFirearm implementation

		public void SetTrigger (ITrigger to)
		{
			if (trigger != to)
			{
				// Disable previous trigger
				if (trigger != null)
				{
					// Record and remove block
					trigger.blocked = false;
					// Remove event handlers
					trigger.onShoot -= Shoot;
					// Disable
					trigger.Disable ();
				}
				// Set new trigger
				trigger = to;
				// Enable new trigger
				if (trigger != null)
				{
					// Add event handlers
					trigger.onShoot += Shoot;
					// Enable
					trigger.Enable ();
                    // Add block if required
                    trigger.blocked = (m_TriggerBlockers.Count > 0);
                }
                // Invoke event
                if (onTriggerChange != null)
                    onTriggerChange(this, trigger);

            }
		}
		public void SetShooter (IShooter to)
		{
			if (shooter != to)
			{
				// Disable previous shooter
				if (shooter != null)
					shooter.Disable ();
				// Set new shooter
				shooter = to;
				// Enable new shooter
				if (shooter != null)
					shooter.Enable ();
                // Invoke event
                if (onShooterChange != null)
                    onShooterChange(this, shooter);
            }
		}
		public void SetAmmo (IAmmo to)
		{
			if (ammo != to)
			{
				// Disable previous ammo
				if (ammo != null)
					ammo.Disable ();
				// Set new ammo
				ammo = to;
				// Enable new ammo
				if (ammo != null)
					ammo.Enable ();
                // Invoke event
                if (onAmmoChange != null)
                    onAmmoChange(this, ammo);
            }
		}
		public void SetReloader (IReloader to)
		{
			if (reloader != to)
			{
				// Disable previous reloader
				if (reloader != null)
					reloader.Disable ();
				// Set new reloader
				reloader = to;
				// Enable new reloader
				if (reloader != null)
					reloader.Enable ();
                // Invoke event
                if (onReloaderChange != null)
                    onReloaderChange(this, reloader);
            }
		}
		public void SetAimer (IAimer to)
		{
			if (aimer != to)
			{
				// Disable previous aimer
				if (aimer != null)
					aimer.Disable ();
				// Set new aimer
				aimer = to;
				// Enable new aimer
				if (aimer != null)
					aimer.Enable ();
                // Invoke event
                if (onAimerChange != null)
                    onAimerChange(this, aimer);
            }
		}
		public void SetEjector (IEjector to)
		{
			if (ejector != to)
			{
				// Disable previous ejector
				if (ejector != null)
					ejector.Disable ();
				// Set new ejector
				ejector = to;
				// Enable new ejector
				if (ejector != null)
					ejector.Enable ();
            }
		}
		public void SetMuzzleEffect (IMuzzleEffect to)
		{
			if (muzzleEffect != to)
			{
				// Disable previous muzzle effect
				if (muzzleEffect != null)
					muzzleEffect.Disable ();
				// Set new muzzle effect
				muzzleEffect = to;
				// Enable new muzzle effect
				if (muzzleEffect != null)
					muzzleEffect.Enable ();
			}
		}

        public void SetHandling(IRecoilHandler to)
        {
            if (recoilHandler != to)
            {
                // Disable previous muzzle effect
                if (recoilHandler != null)
                    recoilHandler.Disable();
                // Set new muzzle effect
                recoilHandler = to;
                // Enable new muzzle effect
                if (recoilHandler != null)
                    recoilHandler.Enable();
            }
        }

        public ITrigger trigger { get; private set; }
		public IShooter shooter { get; private set; }
		public IAmmo ammo { get; private set; }
		public IReloader reloader { get; private set; }
		public IAimer aimer { get; private set; }
		public IEjector ejector { get; private set; }
		public IMuzzleEffect muzzleEffect { get; private set; }
        public IRecoilHandler recoilHandler { get; private set; }
        public string mode { get { return string.Empty; } }

		public event UnityAction<IModularFirearm, ITrigger> onTriggerChange;
		public event UnityAction<IModularFirearm, IShooter> onShooterChange;
		public event UnityAction<IModularFirearm, IAmmo> onAmmoChange;
		public event UnityAction<IModularFirearm, IReloader> onReloaderChange;
		public event UnityAction<IModularFirearm, IAimer> onAimerChange;
		public event UnityAction<IModularFirearm, string> onModeChange;

		public FirearmDelayType raiseDelayType { get { return FirearmDelayType.None; } }
		public void ManualWeaponRaised () {}

        #endregion

        List<UnityEngine.Object> m_TriggerBlockers = new List<UnityEngine.Object>(2);

        protected override void Start ()
		{
			base.Start ();

            m_AudioSource = GetComponent<AudioSource>();

            onStateChanged += (from, to) => 
			{
				if (from == State.Engaged)
					trigger.Release ();
				if (to == State.Engaged)
					trigger.Press ();
			};
		}

		private void Shoot ()
        {
            if (!reloader.empty)
            {
                // Shoot
                if (shooter != null)
                    shooter.Shoot(1f, ammo.effect);

                // Show the muzzle effect & play sound
                if (muzzleEffect != null)
                    muzzleEffect.Fire();

                // Eject shell
                if (ejector != null && ejector.ejectOnFire)
                    ejector.Eject();

                // Decrement ammo
                reloader.DecrementMag(1);
            }
            else
                Reload();

        }

        public void AddTriggerBlocker(UnityEngine.Object o)
        {
            if (!m_TriggerBlockers.Contains(o) || o == null)
                m_TriggerBlockers.Add(o);

            if (trigger != null)
                trigger.blocked = true;
        }

        public void RemoveTriggerBlocker(UnityEngine.Object o)
        {
            int index = m_TriggerBlockers.LastIndexOf(o);
            if (index != -1)
            {
                m_TriggerBlockers.RemoveAt(index);
                trigger.blocked = (m_TriggerBlockers.Count > 0);
            }
        }

#pragma warning disable 0067
        public event UnityAction<ICharacter> onWielderChanged;
        public ICharacter wielder
		{
			get { return null; }
        }
#pragma warning restore 0067

        public Animator animator
		{
			get { return null; }
		}
		public ToggleOrHold aimToggleHold
        {
            get { return null; }
        }

		public void SetRecoilMultiplier (float move, float rotation)
		{
		}
		public void HideGeometry ()
		{
		}
		public void ShowGeometry ()
		{
		}
		public bool Reload ()
		{
			if (reloader != null)
			{
				if (reloader.isReloading)
					return false;
				if (!reloader.canReload)
					return false;

				StartCoroutine (ReloadCoroutine ());
				return true;
			}
			else
				return false;
		}

		public IModularFirearmModeSwitcher modeSwitcher
		{
			get;
			set;
		}

		public void SwitchMode ()
		{
            if (onModeChange != null)
                onModeChange(this, string.Empty);
		}

        private static readonly NeoSerializationKey k_ReloadingKey = new NeoSerializationKey("reloading");
        private static readonly NeoSerializationKey k_OutDamageFilterKey = new NeoSerializationKey("outDamageFilter");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

            writer.WriteValue(k_ReloadingKey, m_Reloading);
            writer.WriteValue(k_OutDamageFilterKey, m_OutDamageFilter);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            if (reader.TryReadValue(k_ReloadingKey, out m_Reloading, m_Reloading) && m_Reloading)
                Reload();

            int intResult = 0;
            if (reader.TryReadValue(k_OutDamageFilterKey, out intResult, m_OutDamageFilter))
                m_OutDamageFilter = (DamageFilter)intResult;
        }

        #region RELOAD

        private bool m_Reloading = false;

		IEnumerator ReloadCoroutine ()
		{
			// Initiate reload (waitable is an object that can be yielded to)
			Waitable reload = reloader.Reload ();
			m_Reloading = !reload.isComplete;

			// Check if reloading (will be false for full mag, or already reloading, etc).
			if (m_Reloading)
			{
                // Block the trigger
                AddTriggerBlocker(this);

				// Yield until reloaded
				yield return reload;

                // Unblock the trigger
                RemoveTriggerBlocker(this);
            }
			else
				yield return null;

			// Complete
			m_Reloading = false;
		}

		#endregion

		#region AUDIO

        private AudioSource m_AudioSource = null;

		public void PlaySound (AudioClip clip, float volume = 1f)
		{
            m_AudioSource.PlayOneShot (clip, volume);
		}

		#endregion
	}
}