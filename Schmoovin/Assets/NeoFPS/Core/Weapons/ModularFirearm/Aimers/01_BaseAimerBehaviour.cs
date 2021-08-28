using UnityEngine;
using UnityEngine.Events;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
    public abstract class BaseAimerBehaviour : BaseFirearmModuleBehaviour, IAimer, IFirearmModuleValidity, INeoSerializableComponent
    {
        [Header ("Aimer Settings")]

        [SerializeField, Tooltip("An audio clip to play when the weapon is raised.")]
        private AudioClip m_AimUpAudio = null;

        [SerializeField, Tooltip("An audio clip to play when the weapon is lowered.")]
        private AudioClip m_AimDownAudio = null;

        [SerializeField, Range(0f, 1f), Tooltip("The highest accuracy possible when firing from the hip.")]
        private float m_HipAccuracyCap = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("The highest accuracy possible when aiming down sights.")]
        private float m_AimedAccuracyCap = 1f;

        [SerializeField, Tooltip("Should the weapon be lowered when reloading or can it stay aimed.")]
        private bool m_CanAimWhileReloading = false;

        [SerializeField, Tooltip("An event called when the weapon is fully raised.")]
        private UnityEvent m_OnAimUp = new UnityEvent();

        [SerializeField, Tooltip("An event called when the weapon is fully lowered.")]
        private UnityEvent m_OnAimDown = new UnityEvent();

        public event UnityAction<IModularFirearm, FirearmAimState> onAimStateChanged;
        public event UnityAction<FpsCrosshair> onCrosshairChange;

        private FpsCrosshair m_Crosshair = FpsCrosshair.Default;
        private FirearmAimState m_AimState = FirearmAimState.HipFire;

        public FpsCrosshair crosshair
        {
            get { return m_Crosshair; }
            protected set
            {
                if (m_Crosshair != value)
                {
                    m_Crosshair = value;
                    if (onCrosshairChange != null)
                        onCrosshairChange(m_Crosshair);
                }
            }
        }

        public virtual bool isAiming
        {
            get { return m_AimState != FirearmAimState.HipFire; }
        }

        public float hipAccuracyCap
        {
            get { return m_HipAccuracyCap; }
        }

        public float aimedAccuracyCap
        {
            get { return m_AimedAccuracyCap; }
        }

        public bool canAimWhileReloading
        {
            get { return m_CanAimWhileReloading; }
        }
        
        public abstract float aimUpDuration { get; }
        public abstract float aimDownDuration { get; }
        
		protected virtual void OnEnable ()
		{
			firearm.SetAimer (this);
		}
		protected virtual void OnDisable ()
		{ }

		public abstract float fovMultiplier { get; }

		public void Aim ()
        {
			if (m_AimState != FirearmAimState.Aiming && m_AimState != FirearmAimState.EnteringAim)
				AimInternal ();
		}

		public void StopAim ()
        {
            if (m_AimState != FirearmAimState.HipFire && m_AimState != FirearmAimState.ExitingAim)
                StopAimInternal(false);
		}

        public void StopAimInstant()
        {
            if (m_AimState != FirearmAimState.HipFire)
                StopAimInternal(true);
        }

		protected abstract void AimInternal ();
		protected abstract void StopAimInternal (bool instant);

        protected void SetAimState(FirearmAimState s)
        {
            if (m_AimState != s)
            {
                m_AimState = s;

                switch (m_AimState)
                {
                    case FirearmAimState.EnteringAim:
                        if (m_AimUpAudio != null)
                            firearm.PlaySound(m_AimUpAudio);
                        break;
                    case FirearmAimState.Aiming:
                        m_OnAimUp.Invoke();
                        break;
                    case FirearmAimState.ExitingAim:
                        if (m_AimDownAudio != null)
                            firearm.PlaySound(m_AimDownAudio);
                        break;
                    case FirearmAimState.HipFire:
                        m_OnAimDown.Invoke();
                        break;
                }

                if (onAimStateChanged != null)
                    onAimStateChanged(firearm, m_AimState);
            }
        }

        public virtual bool isModuleValid
        {
            get { return true; }
        }
        
        private static readonly NeoSerializationKey k_AimingKey = new NeoSerializationKey("aiming");
        private static readonly NeoSerializationKey k_CrosshairKey = new NeoSerializationKey("crosshair");

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (saveMode == SaveMode.Default)
            {
                // Write properties
                writer.WriteValue(k_AimingKey, (int)m_AimState);
                writer.WriteValue(k_CrosshairKey, (int)m_Crosshair);
            }
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            // Read properties
            int intResult = 0;
            if (reader.TryReadValue(k_AimingKey, out intResult, 0))
                m_AimState = (FirearmAimState)intResult;
            if (reader.TryReadValue(k_CrosshairKey, out intResult, 0))
                crosshair = (FpsCrosshair)intResult;
        }
    }
}