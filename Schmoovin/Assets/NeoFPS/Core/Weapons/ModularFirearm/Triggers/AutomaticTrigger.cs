using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-automatictrigger.html")]
	public class AutomaticTrigger : BaseTriggerBehaviour
    {
        [Header("Trigger Settings")]

        [SerializeField, Tooltip("How many fixed update frames between shots.")]
		private int m_ShotSpacing = 5;

		[SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Bool, true, true), Tooltip("The bool animator property key to set while the trigger is pressed.")]
		private string m_TriggerHoldAnimKey = string.Empty;

        private bool m_Triggered = false;
        private int m_Wait = 0;
		private int m_TriggerHoldHash = -1;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_ShotSpacing < 1)
                m_ShotSpacing = 1;
        }
#endif

        public override bool pressed
		{
			get { return m_Triggered; }
		}

		protected override void Awake ()
		{
			if (m_TriggerHoldAnimKey != string.Empty)
				m_TriggerHoldHash = Animator.StringToHash (m_TriggerHoldAnimKey);
			base.Awake ();
		}

		public override void Press ()
        {
            base.Press();

            m_Triggered = true;

			// Should this use events instead?
			if (firearm.animator != null && m_TriggerHoldHash != -1)
				firearm.animator.SetBool (m_TriggerHoldHash, true);
		}

		public override void Release ()
        {
            base.Release();

            m_Triggered = false;

			// Should this use events instead?
			if (firearm.animator != null && m_TriggerHoldHash != -1)
				firearm.animator.SetBool (m_TriggerHoldHash, false);
		}

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Triggered = false;
        }

        protected override void FixedTriggerUpdate ()
		{
			// Decrement cooldowns
			if (m_Wait > 0)
				--m_Wait;

			// Check triggered and cooldowns
			if (m_Triggered && m_Wait == 0)
            {
                // Set cooldowns
                m_Wait = m_ShotSpacing;

                // Shoot
                Shoot ();
			}
        }

        protected override void OnSetBlocked(bool to)
        {
            base.OnSetBlocked(to);
            if (to)
                m_Wait = 0;
        }

        private static readonly NeoSerializationKey k_TriggeredKey = new NeoSerializationKey("triggered");
        private static readonly NeoSerializationKey k_WaitKey = new NeoSerializationKey("wait");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

            if (saveMode == SaveMode.Default)
            {
                writer.WriteValue(k_TriggeredKey, m_Triggered);
                writer.WriteValue(k_WaitKey, m_Wait);
            }
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);
            
            reader.TryReadValue(k_WaitKey, out m_Wait, m_Wait);

            reader.TryReadValue(k_TriggeredKey, out m_Triggered, m_Triggered);
            if (m_Triggered)
                Release();
        }
    }
}