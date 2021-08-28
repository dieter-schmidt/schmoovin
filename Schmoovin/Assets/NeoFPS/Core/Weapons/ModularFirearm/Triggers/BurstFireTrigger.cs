using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Serialization;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-burstfiretrigger.html")]
	public class BurstFireTrigger : BaseTriggerBehaviour
    {
        [Header("Trigger Settings")]

        [SerializeField, Tooltip("The number of shots in a burst.")]
		private int m_BurstSize = 3;

		[SerializeField, Tooltip("Cooldown between shots in a burst (number of fixed update frames).")]
		private int m_BurstSpacing = 5;

        [SerializeField, Tooltip("The minimum spacing from ending a burst to starting a new one. Prevents rapid clicking firing at a higher fire rate than the burst itself.")]
        private int m_MinRepeatDelay = 8;

        [SerializeField, Tooltip("Does holding the trigger fire a new burst after a set duration.")]
        private bool m_RepeatOnTriggerHold = false;

        [SerializeField, FormerlySerializedAs("m_RepeatDelay"), Tooltip("How many fixed update frames from last shot of a burst before starting another.")]
		private int m_HoldRepeatDelay = 50;

		[SerializeField, Tooltip("Is the burst cancelled when the trigger is released?")]
		private bool m_CancelOnRelease = false;

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Bool, true, true), Tooltip("The bool animator property key to set while the trigger is pressed.")]
        private string m_TriggerHoldAnimKey = string.Empty;

        private bool m_Triggered = false;
        private int m_TriggerHoldHash = -1;
        private int m_Wait = 0;
        private int m_Repeat = 0;
        private int m_Shots = 0;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_BurstSize < 2)
                m_BurstSize = 2;
            if (m_BurstSpacing < 1)
                m_BurstSpacing = 1;
            if (m_MinRepeatDelay < m_BurstSpacing)
                m_MinRepeatDelay = m_BurstSpacing;
            if (m_HoldRepeatDelay < m_MinRepeatDelay)
                m_HoldRepeatDelay = m_MinRepeatDelay;
        }
#endif
        protected override void Awake()
        {
            if (m_TriggerHoldAnimKey != string.Empty)
                m_TriggerHoldHash = Animator.StringToHash(m_TriggerHoldAnimKey);
            base.Awake();
        }

        public override bool pressed
		{
			get { return m_Triggered; }
		}

		public override void Press ()
        {
            base.Press();

            m_Triggered = true;

            // Should this use events instead?
            if (firearm.animator != null && m_TriggerHoldHash != -1)
                firearm.animator.SetBool(m_TriggerHoldHash, true);

			// Start the burst
			if (m_Shots == 0)
				m_Shots = m_BurstSize;
		}

		public override void Release ()
        {
            base.Release();

            m_Triggered = false;

            // Should this use events instead?
            if (firearm.animator != null && m_TriggerHoldHash != -1)
                firearm.animator.SetBool (m_TriggerHoldHash, false);

			// Wipe the repeat delay
			m_Repeat = 0;

			// Cancel the burst (if appropriate)
			if (m_CancelOnRelease)
			{
				m_Shots = 0;
			}
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Triggered = false;
        }

        protected override void OnSetBlocked (bool to)
		{
			base.OnSetBlocked (to);
			if (to)
			{
				m_Triggered = false;
				m_Wait = 0;
				m_Shots = 0;
				m_Repeat = 0;
			}
		}

		protected override void FixedTriggerUpdate ()
		{
			// Decrement cooldown
			if (m_Wait > 0)
				--m_Wait;
			
			// Check triggered and cooldowns
			if (m_Shots > 0 && m_Wait == 0)
            {
                // Decrement shots and wait for burst cooldown or trigger delay
                m_Shots = Mathf.Clamp(m_Shots - 1, 0, m_BurstSize);
                if (m_Shots > 0)
                {
                    // Set wait time
                    m_Wait = m_BurstSpacing;
                }
                else
                {
                    if (m_Triggered && m_RepeatOnTriggerHold)
                        m_Repeat = m_HoldRepeatDelay;
                    m_Wait = m_MinRepeatDelay;
                }

                // Shoot
                Shoot ();
            }
			else
			{
				// Decrement repeat
				if (m_Repeat > 0)
				{
					--m_Repeat;
					// Start a new burst
					if (m_Repeat == 0)
						m_Shots = m_BurstSize;
				}
			}
        }

        private static readonly NeoSerializationKey k_TriggeredKey = new NeoSerializationKey("triggered");
        private static readonly NeoSerializationKey k_WaitKey = new NeoSerializationKey("wait");
        private static readonly NeoSerializationKey k_RepeatKey = new NeoSerializationKey("repeat");
        private static readonly NeoSerializationKey k_ShotsKey = new NeoSerializationKey("shots");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

            if (saveMode == SaveMode.Default)
            {
                writer.WriteValue(k_TriggeredKey, m_Triggered);
                writer.WriteValue(k_WaitKey, m_Wait);
                writer.WriteValue(k_RepeatKey, m_Repeat);
                writer.WriteValue(k_ShotsKey, m_Shots);
            }
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            reader.TryReadValue(k_WaitKey, out m_Wait, m_Wait);
            reader.TryReadValue(k_RepeatKey, out m_Repeat, m_Repeat);
            reader.TryReadValue(k_ShotsKey, out m_Shots, m_Shots);

            reader.TryReadValue(k_TriggeredKey, out m_Triggered, m_Triggered);
            if (m_Triggered)
                Release();
        }
    }
}