using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
    [DisallowMultipleComponent]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-modularfirearm.html")]
    [RequireComponent(typeof(AudioSource))]
    public class ModularFirearm : MonoBehaviour, IModularFirearm, IWieldable, IDamageSource, ICrosshairDriver, IPoseHandler, INeoSerializableComponent
    {
        [SerializeField, Tooltip("Does the firearm need to be in a character's inventory. If so, and no wielding character is found, the firearm will be destroyed.")]
        private bool m_RequiresWielder = true;

        [Header ("Animation")]

		[SerializeField, NeoObjectInHierarchyField(true), Tooltip("The weapon geometry animator (optional).")]
		private Animator m_Animator = null;

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Trigger, true, true), Tooltip("The trigger for the fire animation (blank = no animation).")]
		private string m_FireAnimTrigger = "Fire";

        [SerializeField, Tooltip("The transform to move when applying pose offsets. Default is the root of the firearm, but you may want to change this for certain effects.")]
        private Transform m_PoseTransform = null;

        [Header("Accuracy")]

        [SerializeField, Tooltip("The speed above which your accuracy hits zero.")]
        private float m_ZeroAccuracySpeed = 15f;

        [SerializeField, Range(0f, 1f), Tooltip("Smoothes changes in accuracy during sudden speed changes.")]
        private float m_MoveAccuracyDamping = 0.5f;

        [SerializeField, Range(0f, 1f), Tooltip("A multiplier for move accuracy when airborne.")]
        private float m_AirAccuracy = 0f;

        [Header ("Raise / Lower")]

		[SerializeField, Tooltip("The delay type for raising the weapon (you can use FirearmAnimEventsHandler to tie delays to animation)")]
		private FirearmDelayType m_RaiseDelayType = FirearmDelayType.ElapsedTime;

		[SerializeField, Tooltip("The duration in seconds for raising the weapon.")]
		private float m_RaiseDuration = 0.5f;

		[SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Trigger, true, true), Tooltip("The trigger for the weapon raise animation (blank = no animation).")]
		private string m_RaiseAnimTrigger = "Draw";

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Trigger, true, true), Tooltip("The trigger for the weapon lower animation (blank = no animation).")]
        private string m_LowerAnimTrigger = string.Empty;

        [SerializeField, Tooltip("The time taken to lower the item on deselection.")]
        private float m_DeselectDuration = 0f;

        private DeselectionWaitable m_DeselectionWaitable = null;
        private int m_FireAnimTriggerHash = -1;
		private int m_RaiseAnimTriggerHash = -1;
        private int m_LowerAnimTriggerHash = -1;
        private float m_RaiseTimer = 0f;
        private bool m_WaitingForExternalTrigger = false;

        public class DeselectionWaitable : Waitable
        {
            private float m_Duration = 0f;
            private float m_StartTime = 0f;

            public DeselectionWaitable(float duration)
            {
                m_Duration = duration;
            }

            public void ResetTimer()
            {
                m_StartTime = Time.time;
            }

            protected override bool CheckComplete()
            {
                return (Time.time - m_StartTime) > m_Duration;
            }
        }

        public Animator animator { get { return m_Animator; } }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_Animator == null)
                m_Animator = GetComponentInChildren<Animator>();

            if (m_PoseTransform == null)
                m_PoseTransform = transform;

            if (m_RaiseDuration < 0f)
                m_RaiseDuration = 0f;
    }
#endif

        protected virtual void Awake ()
		{
			// Initialise subsystems
			InitialiseGeometry ();

			// Get animation trigger hashes
			if (m_FireAnimTrigger != string.Empty)
				m_FireAnimTriggerHash = Animator.StringToHash (m_FireAnimTrigger);
			else
				m_FireAnimTriggerHash = -1;

			if (m_RaiseAnimTrigger != string.Empty)
				m_RaiseAnimTriggerHash = Animator.StringToHash (m_RaiseAnimTrigger);
			else
				m_RaiseAnimTriggerHash = -1;

            if (m_LowerAnimTrigger != string.Empty)
                m_LowerAnimTriggerHash = Animator.StringToHash(m_LowerAnimTrigger);
            else
                m_LowerAnimTriggerHash = -1;

            // Get the audio source
            m_AudioSource = GetComponent<AudioSource>();
			
            // Set up deselection waitable
            if (m_DeselectDuration > 0.001f)
                m_DeselectionWaitable = new DeselectionWaitable(m_DeselectDuration);

            // Set up pose handler
            m_PoseHandler = new PoseHandler(m_PoseTransform, Vector3.zero, Quaternion.identity);
        }

        protected virtual void Start ()
        {
            if (wielder == null && m_RequiresWielder)
                Destroy(gameObject);
        }

		protected virtual void Update ()
		{
            // Aiming
            // aimToggleHold.SetInput (m_AimHeld);
            // m_AimHeld = false;

            // Update pose
            m_PoseHandler.UpdatePose();
        }

        void FixedUpdate ()
		{
			// Check movement accuracy
            if (wielder != null)
            {
                var characterController = wielder.motionController.characterController;

                // Get target move accuracy
                float targetMoveAccuracy = 1f - Mathf.Clamp01(characterController.velocity.magnitude / m_ZeroAccuracySpeed);
                if (!characterController.isGrounded)
                    targetMoveAccuracy *= m_AirAccuracy;

                // Get damped (and snapped) accuracy
                float moveAccuracy = Mathf.Lerp(moveAccuracyModifier, targetMoveAccuracy, Mathf.Lerp(0.75f, 0.05f, m_MoveAccuracyDamping));
                if (moveAccuracy > 0.999f)
                    moveAccuracy = 1f;
                if (moveAccuracy < 0.001f)
                    moveAccuracy = 0f;

                // Apply
                moveAccuracyModifier = moveAccuracy;
            }
            else
            {
                moveAccuracyModifier = 1f;
            }

			// Recover accuracy
			if (currentAccuracy < 1f && recoilHandler != null)
			{
                if (aimToggleHold.on)
                    currentAccuracy += Time.deltaTime * recoilHandler.sightedAccuracyRecover;
				else
					currentAccuracy += Time.deltaTime * recoilHandler.hipAccuracyRecover;
			}
		}

		#region IModularFirearm IMPLEMENTATION

		public ITrigger trigger { get; private set; }
		public IShooter shooter { get; private set; }
		public IAmmo ammo { get; private set; }
		public IReloader reloader { get; private set; }
		public IAimer aimer { get; private set; }
		public IEjector ejector { get; private set; }
		public IMuzzleEffect muzzleEffect { get; private set; }
        public IRecoilHandler recoilHandler { get; private set; }

		public event UnityAction<IModularFirearm, ITrigger> onTriggerChange;
		public event UnityAction<IModularFirearm, IShooter> onShooterChange;
		public event UnityAction<IModularFirearm, IAmmo> onAmmoChange;
		public event UnityAction<IModularFirearm, IReloader> onReloaderChange;
		public event UnityAction<IModularFirearm, IAimer> onAimerChange;
        public event UnityAction<IModularFirearm, string> onModeChange;

		public void SetTrigger (ITrigger to)
		{
			if (trigger != to)
			{
                bool pressed = false;

				// Disable previous trigger
				if (trigger != null)
				{
                    // Record and remove block
                    trigger.blocked = false;
                    // Record and remove pressed
                    pressed = trigger.pressed;
                    if (pressed)
                        trigger.Release();
                    // Remove event handlers
                    trigger.onShoot -= Shoot;
                    trigger.onShootContinuousChanged -= ShootContinuous;
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
                    trigger.onShootContinuousChanged += ShootContinuous;
                    // Enable
                    trigger.Enable ();
					// Add block if required
					trigger.blocked = (m_TriggerBlockers.Count > 0);
                    // Press if required
                    if (pressed)
                        trigger.Press();
				}

				// Fire event
				if (onTriggerChange != null)
					onTriggerChange (this, trigger);
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

				// Fire event
				if (onShooterChange != null)
					onShooterChange (this, shooter);
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

				// Fire event
				if (onAmmoChange != null)
					onAmmoChange (this, ammo);
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

				// Fire event
				if (onReloaderChange != null)
					onReloaderChange (this, reloader);
			}
		}

		public void SetAimer (IAimer to)
		{
            if (aimer != to)
            {
                // Disable previous aimer
                if (aimer != null)
                {
                    aimer.StopAimInstant();
                    aimer.Disable();
                    aimer.onCrosshairChange -= OnCrosshairChanged;
                    aimer.onAimStateChanged -= OnAimStateChanged;
                }

                // Set new aimer
                aimer = to;

                // Enable new aimer
                if (aimer != null)
                {
                    aimer.Enable();
                    aimer.onCrosshairChange += OnCrosshairChanged;
                    aimer.onAimStateChanged += OnAimStateChanged;
                    OnCrosshairChanged(aimer.crosshair);
                    if (aimToggleHold.on)
                        aimer.Aim();
                }

                // Fire event
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

        public FirearmDelayType raiseDelayType
        {
            get { return m_RaiseDelayType; }
        }

		public void ManualWeaponRaised ()
		{
			if (m_RaiseDelayType != FirearmDelayType.ExternalTrigger)
			{
				Debug.LogError ("Attempting to manually signal weapon raised when delay type is not set to custom");
				return;
			}
			if (!m_WaitingForExternalTrigger)
			{
				Debug.LogError ("Attempting to manually signal weapon raised, not expected");
				return;
			}
			m_WaitingForExternalTrigger = false;
		}


        public void SetRecoilMultiplier(float move, float rotation)
        {
            if (recoilHandler != null)
                recoilHandler.SetRecoilMultiplier(move, rotation);
        }

        #endregion

        #region ICrosshairDriver IMPLEMENTATION

        private bool m_HideCrosshair = false;
        private float m_MinAccuracy = 0f;
        private float m_MaxAccuracy = 1f;

        public event UnityAction<FpsCrosshair> onCrosshairChanged;

        public float accuracy
        {
            get { return Mathf.Clamp(m_CurrentAccuracy * m_MoveAccuracyModifier * currentAimerAccuracy, m_MinAccuracy, m_MaxAccuracy); }
        }

        public float minAccuracy
        {
            get { return m_MinAccuracy; }
            set
            { 
                m_MinAccuracy = Mathf.Clamp(value, 0f, m_MaxAccuracy);
                if (onAccuracyChanged != null)
                    onAccuracyChanged(accuracy);
            }
        }

        public float maxAccuracy
        {
            get { return m_MaxAccuracy; }
            set
            {
                m_MaxAccuracy = Mathf.Clamp(value, m_MinAccuracy, 1f);
                if (onAccuracyChanged != null)
                    onAccuracyChanged(accuracy);
            }
        }

        public FpsCrosshair crosshair
        {
            get
            {
                if (m_HideCrosshair)
                    return FpsCrosshair.None;
                else
                {
                    if (aimer != null)
                        return aimer.crosshair;
                    else
                        return FpsCrosshair.Default;
                }
            }
        }

        void OnCrosshairChanged(FpsCrosshair c)
        {
            if (!m_HideCrosshair && onCrosshairChanged != null)
                onCrosshairChanged(c);
        }

        public void HideCrosshair()
        {
            if (!m_HideCrosshair)
            {
                bool triggerEvent = (onCrosshairChanged != null && crosshair != FpsCrosshair.None);

                m_HideCrosshair = true;

                if (triggerEvent)
                        onCrosshairChanged(FpsCrosshair.None);
            }
        }

        public void ShowCrosshair()
        {
            if (m_HideCrosshair)
            {
                // Reset
                m_HideCrosshair = false;

                // Fire event
                if (onCrosshairChanged != null && crosshair != FpsCrosshair.None)
                        onCrosshairChanged(crosshair);
            }
        }

        #endregion

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
                if (wielder != null)
                    return wielder.controller;
                else
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

        #region SHOOTING

        bool m_ContinuousShooting = false;
        WaitForFixedUpdate m_FixedYield = new WaitForFixedUpdate();

        private void Shoot ()
		{
			if (reloader != null && reloader.empty)
			{
				if (!FpsSettings.gameplay.autoReload || Reload () == false)
                    PlayDryFireSound ();
                return;
            }

            if (reloading && reloader.interruptable)
            {
                reloader.Interrupt();
                return;
            }

			// Shoot
			if (shooter != null)
				shooter.Shoot (accuracy, ammo.effect);

			// Play animation
			if (m_FireAnimTriggerHash != -1 && animator != null && animator.isActiveAndEnabled)
				animator.SetTrigger (m_FireAnimTriggerHash);

			// Handle recoil
            if (recoilHandler != null)
			    recoilHandler.Recoil ();

			// Show the muzzle effect & play firing sound
			if (muzzleEffect != null)
				muzzleEffect.Fire ();

			// Eject shell
			if (ejector != null && ejector.ejectOnFire)
				ejector.Eject ();

            // Decrease the accuracy
            if (recoilHandler != null)
            {
                if (aimToggleHold.on)
                    currentAccuracy -= recoilHandler.sightedAccuracyKick;
                else
                    currentAccuracy -= recoilHandler.hipAccuracyKick;
            }

			// Decrement ammo
			reloader.DecrementMag (1);
        }

        void ShootContinuous(bool shoot)
        {
            if (shoot)
            {
                if (reloader != null && reloader.empty)
                {
                    if (Reload() == false)
                        PlayDryFireSound();
                    return;
                }

                if (reloading && reloader.interruptable)
                {
                    reloader.Interrupt();
                    return;
                }

                if (!m_ContinuousShooting)
                {
                    m_ContinuousShooting = true;
                    StartCoroutine(ShootCoroutine());
                }
            }
            else
            {
                m_ContinuousShooting = false;
            }
        }

        IEnumerator ShootCoroutine()
        {
            // Show muzzle effect
            if (muzzleEffect != null)
                muzzleEffect.FireContinuous();

            while (m_ContinuousShooting)
            {
                if (reloader != null && reloader.empty)
                    m_ContinuousShooting = false;
                else
                {
                    // Shoot
                    if (shooter != null)
                        shooter.Shoot(accuracy, ammo.effect);

                    // Handle recoil
                    if (recoilHandler != null)
                        recoilHandler.Recoil();

                    // Decrease the accuracy
                    if (recoilHandler != null)
                    {
                        if (aimToggleHold.on)
                            currentAccuracy -= recoilHandler.sightedAccuracyKick;
                        else
                            currentAccuracy -= recoilHandler.hipAccuracyKick;
                    }

                    // Decrement ammo
                    reloader.DecrementMag(1);
                }
                yield return m_FixedYield;
            }

            if (muzzleEffect != null)
                muzzleEffect.StopContinuous();

            // Save/Load required
        }

        #endregion

        #region TRIGGER BLOCKING

        List<UnityEngine.Object> m_TriggerBlockers = new List<UnityEngine.Object>(2);

        public void AddTriggerBlocker(UnityEngine.Object o)
        {
            if (!m_TriggerBlockers.Contains(o) || o == null)
                m_TriggerBlockers.Add(o);

            if (trigger != null)
            {
                trigger.Release();
                trigger.blocked = true;
            }
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

        #endregion

        #region MODE SWITCHING

        private IModularFirearmModeSwitcher m_ModeSwitcher = null;
                
        public virtual string mode
        {
            get
            {
                if (m_ModeSwitcher != null)
                    return m_ModeSwitcher.currentMode;
                else
                    return string.Empty;
            }
        }

        public IModularFirearmModeSwitcher modeSwitcher
        {
            get { return m_ModeSwitcher; }
            set
            {
                if (m_ModeSwitcher != null && value != null)
                    Debug.LogError("Assigning a mode switcher to the weapon while one is already assigned. Weapon: " + name);

                m_ModeSwitcher = value;
                GetStartingModeInternal();
            }
        }

        public void SwitchMode ()
		{
            if (SwitchModeInternal() && onModeChange != null)
                onModeChange(this, mode);
        }

        protected virtual void GetStartingModeInternal ()
        {
            if (m_ModeSwitcher != null)
                m_ModeSwitcher.GetStartingMode();
            if (onModeChange != null)
                onModeChange(this, mode);
        }

		protected virtual bool SwitchModeInternal ()
        {
            if (m_ModeSwitcher != null)
            {
                m_ModeSwitcher.SwitchModes();
                return true;
            }
            return false;
        }

        #endregion

        #region ACCURACY

        public event UnityAction<float> onAccuracyChanged;
        
        private float m_MoveAccuracyModifier = 0f;
		private float moveAccuracyModifier
		{
			get { return m_MoveAccuracyModifier; }
			set 
			{
				float to = Mathf.Clamp01 (value);
				if (m_MoveAccuracyModifier != to)
				{
					m_MoveAccuracyModifier = to;
                    if (onAccuracyChanged != null)
                        onAccuracyChanged(accuracy);
                }
			}
		}

		private float m_CurrentAccuracy = 1f;
		private float currentAccuracy
		{
			get { return m_CurrentAccuracy; }
			set 
			{
				float to = Mathf.Clamp01 (value);
				if (m_CurrentAccuracy != to)
				{
					m_CurrentAccuracy = Mathf.Clamp01 (value);
					if (onAccuracyChanged != null)
                        onAccuracyChanged(accuracy);
				}
			}
		}

        private float currentAimerAccuracy
        {
            get
            {
                if (aimer == null)
                    return 1f;
                else
                {
                    if (aimer.isAiming)
                        return aimer.aimedAccuracyCap;
                    else
                        return aimer.hipAccuracyCap;
                }
            }
        }

        void OnAimStateChanged(IModularFirearm f, FirearmAimState s)
        {
            if (onAccuracyChanged != null)
                onAccuracyChanged(accuracy);
        }

        #endregion

        #region SELECTION

        public event UnityAction<ICharacter> onWielderChanged;

        private ICharacter m_Wielder = null;
        public ICharacter wielder
		{
			get { return m_Wielder; }
			private set
            {
                if (m_Wielder != value)
                {
                    m_Wielder = value;
                    if (onWielderChanged != null)
                        onWielderChanged(m_Wielder);
                }
            }
		}

		void OnEnable ()
		{
			wielder = GetComponentInParent<ICharacter>();
        }

		void OnDisable ()
		{
            // Stop selection coroutine
            if (m_OnSelectCoroutine != null)
            {
                StopCoroutine(m_OnSelectCoroutine);
                m_OnSelectCoroutine = null;
            }
            // Stop aiming
            aimToggleHold.on = false;
            if (aimer != null)
                aimer.StopAimInstant();
            // Stop reloading
            reloading = false;
            // Reset pose
            m_PoseHandler.OnDisable();
        }

        private void OnDestroy()
        {
            // Unset the wielder (may be dropped)
            wielder = null;
        }

        private Coroutine m_OnSelectCoroutine = null;
        private IEnumerator OnSelectCoroutine (float timer)
		{
            ShowGeometry ();

            // Show draw animation
            if (m_RaiseAnimTriggerHash != -1 && animator != null)
                animator.SetTrigger(m_RaiseAnimTriggerHash);

            // Advance one frame
            yield return null;

            // Block the trigger
            AddTriggerBlocker(this);

            // Delay completion
            switch (m_RaiseDelayType)
            {
                case FirearmDelayType.ElapsedTime:
                    m_RaiseTimer = timer;
                    while (m_RaiseTimer > 0f)
                    {
                        yield return null;
                        m_RaiseTimer -= Time.deltaTime;
                    }
                    break;
                case FirearmDelayType.ExternalTrigger:
                    m_WaitingForExternalTrigger = true;
                    while (m_WaitingForExternalTrigger)
                        yield return null;
                    break;
            }

            // Unblock the trigger
            RemoveTriggerBlocker(this);

            m_OnSelectCoroutine = null;
			// NB: For raise animation, set it as the entry state of the weapon geo's animation controller. This means
			// it will be triggered automatically at this point and does not need triggering explicitly
		}

        public void Select()
        {
            if (wielder != null || !m_RequiresWielder)
            {
                PlayWeaponRaiseSound();

                currentAccuracy = 0.75f;

                // Play raise animation
                if (m_RaiseAnimTriggerHash != -1 && m_Animator != null)
                    m_Animator.SetTrigger(m_RaiseAnimTriggerHash);

                m_OnSelectCoroutine = StartCoroutine(OnSelectCoroutine(m_RaiseDuration));
            }
        }

        public void DeselectInstant()
        {
            // Block the trigger
            AddTriggerBlocker(this);
        }

        public Waitable Deselect()
        {
            // Block the trigger
            AddTriggerBlocker(this);

            // Play lower animation
            if (m_LowerAnimTriggerHash != -1 && m_Animator != null)
                m_Animator.SetTrigger(m_LowerAnimTriggerHash);

            // Wait for deselection
            if (m_DeselectionWaitable != null)
                m_DeselectionWaitable.ResetTimer();

            return m_DeselectionWaitable;
        }

        #endregion

        #region INPUT

        [Serializable]
        private class ToggleHoldAim : ToggleOrHold
		{
			public ModularFirearm firearm { get; set; }

			protected override void OnActivate ()
			{
                if (firearm.aimer != null)
                    firearm.aimer.Aim();
            }
            protected override void OnDeactivate()
            {
                if (firearm.aimer != null)
                    firearm.aimer.StopAim();
            }

            public ToggleHoldAim(ModularFirearm f) : base(f.IsAimBlocked)
            {
                firearm = f;
            }
        }

        private ToggleHoldAim m_AimToggleHold = null;
        public ToggleOrHold aimToggleHold
        {
            get
            {
                if (m_AimToggleHold == null)
                    m_AimToggleHold = new ToggleHoldAim(this);
                return m_AimToggleHold;
            }
        }

        bool IsAimBlocked()
        {
            // Check if disabled
            if (enabled == false)
                return true;

            // Check if no aimer 
            if (aimer == null)
                return true;

            // Check if currently selecting
            if (m_OnSelectCoroutine != null)
                return true;

            // Check if currently deselecting
            if (m_DeselectionWaitable != null && !m_DeselectionWaitable.isComplete)
                return true;

            //Check if reloading and can't aim during
            if (reloading && !aimer.canAimWhileReloading)
                return true;

            return false;
        }

        #endregion

        #region RELOAD

		public bool reloading
        {
            get;
            private set;
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

		IEnumerator ReloadCoroutine ()
        {
            yield return null;
            // Initiate reload (waitable is an object that can be yielded to)
            Waitable reload = reloader.Reload ();
            if (reload != null)
            {
                reloading = !reload.isComplete;

                // Check if reloading (will be false for full mag, or already reloading, etc).
                if (reloading)
                {
                    // Block the trigger
                    AddTriggerBlocker(this);

                    yield return reload;
                }
                else
                    yield return null;
            }
            else
                yield return null;

            // Complete
            reloading = false;

            // Unblock the aimer
            if (aimer != null && !aimer.canAimWhileReloading)
            {
                // Wait and then unblock the trigger
                if (aimToggleHold.on)
                {
                    float delay = aimer.aimUpDuration;
                    while (delay > 0f)
                    {
                        yield return null;
                        delay -= Time.deltaTime;
                    }
                }
            }

            // Unblock the trigger
            RemoveTriggerBlocker(this);
        }

		#endregion

		#region GEOMETRY

		private MeshRenderer[] m_MeshRenderers = null;
        private SkinnedMeshRenderer[] m_SkinnedRenderers = null;

        void InitialiseGeometry ()
		{
			m_MeshRenderers = GetComponentsInChildren<MeshRenderer> (true);
			m_SkinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer> (true);
		}

		public void HideGeometry ()
		{
			for (int i = 0; i < m_MeshRenderers.Length; ++i)
				m_MeshRenderers [i].enabled = false;
			for (int i = 0; i < m_SkinnedRenderers.Length; ++i)
				m_SkinnedRenderers [i].enabled = false;
		}

		public void ShowGeometry ()
		{
			for (int i = 0; i < m_MeshRenderers.Length; ++i)
				m_MeshRenderers [i].enabled = true;
			for (int i = 0; i < m_SkinnedRenderers.Length; ++i)
				m_SkinnedRenderers [i].enabled = true;
		}

		#endregion

		#region AUDIO

		[Header ("Audio")]
		[Tooltip("The audio clip to play if attempting to fire while empty.")]
        [SerializeField] private AudioClip m_DryFireSound = null;
        [Tooltip("The audio clip to play when the weapon is drawn.")]
        [SerializeField] private AudioClip m_WeaponRaiseSound = null;
        [Tooltip("The volume of all weapon sounds (includes gunshots and reload - the muzzle effects and reloader modules have their own volume sliders that stack with this).")]
        [SerializeField, Range(0f, 1f)] private float m_WeaponVolume = 1f;

        private AudioSource m_AudioSource = null;

        public void PlaySound (AudioClip clip, float volume = 1f)
		{
            if (wielder != null && wielder.audioHandler != null)
                wielder.audioHandler.PlayClip(clip, volume);
            else
            {
                if (m_AudioSource != null && m_AudioSource.isActiveAndEnabled)
                    m_AudioSource.PlayOneShot(clip, volume * m_WeaponVolume);
            }
		}
        
        void PlayDryFireSound ()
        {
			if (m_DryFireSound != null)
                PlaySound(m_DryFireSound);
        }

        void PlayWeaponRaiseSound()
        {
            // Play directly as fast switching means you don't want it to continue if the weapon is disabled
            if (m_WeaponRaiseSound != null && m_AudioSource != null && m_AudioSource.isActiveAndEnabled)
                m_AudioSource.PlayOneShot(m_WeaponRaiseSound, m_WeaponVolume);
        }

        #endregion

        #region POSE

        private PoseHandler m_PoseHandler = null;

        public void SetPose(Vector3 position, Quaternion rotation, float duration)
        {
            m_PoseHandler.SetPose(position, rotation, duration);
        }

        public void SetPose(Vector3 position, CustomPositionInterpolation posInterp, Quaternion rotation, CustomRotationInterpolation rotInterp, float duration)
        {
            m_PoseHandler.SetPose(position, posInterp, rotation, rotInterp, duration);
        }

        public void ResetInstant()
        {
            m_PoseHandler.ResetInstant();
        }

        public void ResetPose(float duration)
        {
            m_PoseHandler.ResetPose(duration);
        }

        public void ResetPose(CustomPositionInterpolation posInterp, CustomRotationInterpolation rotInterp, float duration)
        {
            m_PoseHandler.ResetPose(posInterp, rotInterp, duration);
        }

        #endregion

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_MoveAccuracyKey = new NeoSerializationKey("moveAccuracy");
        private static readonly NeoSerializationKey k_CurrentAccuracyKey = new NeoSerializationKey("currentAccuracy");
        private static readonly NeoSerializationKey k_ReloadingKey = new NeoSerializationKey("reloading");
        private static readonly NeoSerializationKey k_AimKey = new NeoSerializationKey("aim");
        private static readonly NeoSerializationKey k_RaiseTimerKey = new NeoSerializationKey("raiseTimer");

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (saveMode == SaveMode.Default)
            {
                // Write accuracy
                writer.WriteValue(k_MoveAccuracyKey, moveAccuracyModifier);
                writer.WriteValue(k_CurrentAccuracyKey, currentAccuracy);

                // Write if reloading
                writer.WriteValue(k_ReloadingKey, reloading);

                // Write draw state
                if (m_OnSelectCoroutine != null)
                    writer.WriteValue(k_RaiseTimerKey, m_RaiseTimer);

                // Write aim state
                aimToggleHold.WriteProperties(writer, k_AimKey, false);

                // Write pose
                m_PoseHandler.WriteProperties(writer);
            }
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            // Read accuracy
            float floatResult = 0f;
            if (reader.TryReadValue(k_MoveAccuracyKey, out floatResult, 1f))
                moveAccuracyModifier = floatResult;
            if (reader.TryReadValue(k_CurrentAccuracyKey, out floatResult, 1f))
                currentAccuracy = floatResult;

            // Check if reloading
            bool boolResult = false;
            if (reader.TryReadValue(k_ReloadingKey, out boolResult, false) && boolResult)
            {
                StartCoroutine(ReloadCoroutine());
            }

            // Read aim state
            aimToggleHold.ReadProperties(reader, k_AimKey);

            // Read draw state
            if (reader.TryReadValue(k_RaiseTimerKey, out floatResult, 0f))
                m_OnSelectCoroutine = StartCoroutine(OnSelectCoroutine(floatResult));

            // Read pose
            m_PoseHandler.ReadProperties(reader);
        }

        #endregion
    }
}