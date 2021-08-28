using NeoSaveGames;
using NeoSaveGames.Serialization;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-chargedtrigger.html")]
	public class ChargedTrigger : BaseTriggerBehaviour
    {
        [Header("Trigger Settings")]

        [SerializeField, Tooltip("How long does it take to charge the trigger.")]
		private float m_ChargeDuration = 0.5f;

		[SerializeField, Tooltip("How long does it take to uncharge the trigger, assuming it hasn't gone off.")]
		private float m_UnchargeDuration = 0.5f;
		
		[SerializeField, Tooltip("Once the shot is fired, start charging the next shot.")]
		private bool m_Repeat = false;

        [SerializeField, Tooltip("The time between a shot firing and starting charging the next shot")]
        private float m_RepeatDelay = 0.5f;

        [Header ("Audio")]

        [SerializeField, Tooltip("The source to play the audio from (needs its own as it must be interrupted and seeked).")]
        private AudioSource m_AudioSource = null;

        [SerializeField, Tooltip("The audio clip to play on charge.")]
        private AudioClip m_TriggerAudioCharge = null;

        [SerializeField, Tooltip("The audio clip to play on release.")]
        private AudioClip m_TriggerAudioRelease = null;

        [Header("Animation")]

        [SerializeField, Tooltip("How should the charge be animated. LayerWeight blends in a layer in the weapon's animator as charge increases. FloatParameter sets a float parameter value on the animator. Events only fires an event when the charge changes, but does not affect the attached animator.")]
        private ChargeAnimation m_ChargeAnimation = ChargeAnimation.LayerWeight;

        [SerializeField, Tooltip("The animator layer index to blend in for the charge effect.")]
        private int m_LayerIndex = 1;

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Float, true, true), Tooltip("The float animator parameter key to set when the trigger charge changes.")]
        private string m_ChargeAnimKey = string.Empty;

        [Space(2)]

        [SerializeField, Tooltip("An event fired each frame the charge changes.")]
        private FloatEvent m_OnChargeChanged = new FloatEvent();

        public event UnityAction<float> onChargeChanged;

        private bool m_Triggered = false;
        private float m_InverseChargeDuration = 0f;
        private float m_InverseUnchargeDuration = 0f;
        private float m_RepeatTimer = 0f;
        private int m_ChargeAnimHash = 0;

        [Serializable]
        class FloatEvent : UnityEvent<float> { }

        enum ChargeAnimation
        {
            LayerWeight,
            FloatParameter,
            EventsOnly
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_ChargeDuration < 0.1f)
                m_ChargeDuration = 0.1f;
            if (m_UnchargeDuration < 0f)
                m_UnchargeDuration = 0f;
            if (m_RepeatDelay < 0f)
                m_RepeatDelay = 0f;
            if (m_LayerIndex < 1)
                m_LayerIndex = 1;
        }
#endif

        private float m_Charge = 0f;
		public float charge
		{
			get { return m_Charge; }
			private set
			{
                if (m_Charge != value)
                {
                    m_Charge = Mathf.Clamp01(value);

                    // Update animator
                    switch (m_ChargeAnimation)
                    {
                        case ChargeAnimation.LayerWeight:
                            {
                                if (firearm.animator != null && firearm.animator.isActiveAndEnabled && firearm.animator.layerCount >= m_LayerIndex)
                                    firearm.animator.SetLayerWeight(m_LayerIndex, m_Charge);
                            }
                            break;
                        case ChargeAnimation.FloatParameter:
                            {
                                if (firearm.animator != null && m_ChargeAnimHash != 0 && firearm.animator.isActiveAndEnabled)
                                    firearm.animator.SetFloat(m_ChargeAnimHash, m_Charge);
                            }
                            break;
                    }

                    // Fire events
                    if (onChargeChanged != null)
                        onChargeChanged(m_Charge);
                    m_OnChargeChanged.Invoke(m_Charge);
                }
			}
		}

		public override bool pressed
		{
			get { return m_Triggered; }
		}

        protected override void Awake()
        {
            base.Awake();
            if (m_ChargeAnimation == ChargeAnimation.FloatParameter)
                m_ChargeAnimHash = Animator.StringToHash(m_ChargeAnimKey);
        }

        public override void Enable ()
		{
			base.Enable ();
			charge = 0f;
			m_InverseChargeDuration = 1f / m_ChargeDuration;
			m_InverseUnchargeDuration = 1f / m_UnchargeDuration;
		}

		public override void Disable ()
		{
			base.Disable ();
			charge = 0f;
		}

		public override void Press ()
        {
            base.Press();

            if (firearm.reloader != null && firearm.reloader.empty)
				Shoot ();
            else
            {
                m_Triggered = true;
                if (m_AudioSource != null)
                {
                    m_AudioSource.clip = m_TriggerAudioCharge;
                    m_AudioSource.time = 0f;
                    m_AudioSource.Play();
                }
            }
        }

		public override void Release ()
        {
            base.Release();

            if (m_Triggered)
            {
                m_Triggered = false;
                if (m_AudioSource != null)
                {
                    m_AudioSource.Stop();
                    if (m_TriggerAudioRelease != null)
                    {
                        m_AudioSource.clip = m_TriggerAudioRelease;
                        m_AudioSource.time = Mathf.Clamp01(1f - charge) * (m_TriggerAudioRelease.length - 0.01f);
                        m_AudioSource.Play();
                    }
                }
            }
        }

        protected override void FixedTriggerUpdate ()
		{
            if (m_Triggered)
            {
                if (m_RepeatTimer > 0f)
                {
                    m_RepeatTimer -= Time.deltaTime;
                    if (m_RepeatTimer < 0f)
                        m_RepeatTimer = 0f;
                }
                else
                    charge = charge + Time.deltaTime * m_InverseChargeDuration;
            }
            else
            {
                if (charge > 0f)
                    charge = charge - Time.deltaTime * m_InverseUnchargeDuration;
            }

			// Check if fully charged
			if (charge == 1f)
			{
				// Shoot
				Shoot ();

				if (m_Repeat)
                {
                    charge = 0f;
                    m_RepeatTimer = m_RepeatDelay;

                    if (firearm.reloader != null && firearm.reloader.empty)
                    {
                        //m_Triggered = false;
                        Shoot();
                    }
                    else
                    {
                        m_AudioSource.time = 0f;
                        m_AudioSource.Play();
                    }
				}
				else
				{
					// Reset the trigger
					m_Triggered = false;
					charge = 0f;
				}
			}
        }

        protected override void OnSetBlocked(bool to)
        {
            base.OnSetBlocked(to);
            if (to)
                m_RepeatTimer = 0;
        }

        private static readonly NeoSerializationKey k_ChargeKey = new NeoSerializationKey("charge");
        private static readonly NeoSerializationKey k_TriggeredKey = new NeoSerializationKey("triggered");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

            if (saveMode == SaveMode.Default)
            {
                writer.WriteValue(k_ChargeKey, charge);
                writer.WriteValue(k_TriggeredKey, m_Triggered);
            }
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            float floatResult = 0f;
            if (reader.TryReadValue(k_ChargeKey, out floatResult, 0f))
                charge = floatResult;

            reader.TryReadValue(k_TriggeredKey, out m_Triggered, m_Triggered);
            if (m_Triggered)
                Release();
        }
    }
}