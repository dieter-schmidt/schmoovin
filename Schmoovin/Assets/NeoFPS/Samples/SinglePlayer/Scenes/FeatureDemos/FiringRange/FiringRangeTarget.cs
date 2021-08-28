using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.Samples.SinglePlayer
{
	[HelpURL("https://docs.neofps.com/manual/samplesref-mb-firingrangetarget.html")]
	public class FiringRangeTarget : MonoBehaviour, IDamageHandler, INeoSerializableComponent
    {
		[SerializeField, Tooltip("The damage threshold for the target to drop and register as a hit.")]
		private float m_DamageThreshold = 1f;

		[SerializeField, Tooltip("The duration the target will be visible. If the target is not hit in this time it registers as a miss.")]
		private float m_PopupDuration = 0.5f;

		[SerializeField, Tooltip("The axis to rotate the target around when it pops up.")]
		private Vector3 m_RotationAxis = new Vector3(1f, 0f, 0f);

		[SerializeField, Tooltip("The rotation of the target around the specified axis when it is fully hidden.")]
		private float m_HiddenRotation = 180f;

		private Transform m_RotationTransform = null;
        private FiringRangeSequencer m_Sequencer = null;
        private Coroutine m_SequenceCoroutine = null;
        private float m_Lerp = 0f;
        private float m_Timer = 0f;
        private bool m_Hit = false;
        private TargetState m_State = TargetState.Idle;

        enum TargetState
        {
            Idle,
            Raising,
            Raised,
            Lowering
        }

        protected bool initialised
        {
            get;
            private set;
        }

		public bool hit
		{
			get { return m_Hit; }
		}

		public virtual bool hidden
		{
			get { return m_SequenceCoroutine == null; }
		}

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_DamageThreshold < 1f)
                m_DamageThreshold = 1f;
            if (m_PopupDuration < 0.1f)
                m_PopupDuration = 0.1f;
            m_HiddenRotation = Mathf.Clamp(m_HiddenRotation, -180f, 180f);
        }
#endif

        public virtual void Initialise (FiringRangeSequencer sequencer)
		{
			m_Sequencer = sequencer;
			m_RotationTransform = transform;

            if (!initialised)
            {
                m_RotationTransform.localRotation = Quaternion.AngleAxis(m_HiddenRotation, m_RotationAxis);
                m_Hit = false;
                initialised = true;
            }
        }

		public virtual void Popup (float duration)
		{
			if (m_SequenceCoroutine != null)
				StopCoroutine (m_SequenceCoroutine);
            m_Timer = duration;
            m_SequenceCoroutine = StartCoroutine (SequencePopup (0f));
		}

		public virtual void ResetTarget ()
		{
			if (!hidden && gameObject.activeInHierarchy)
			{
                if (m_SequenceCoroutine != null)
                    StopCoroutine(m_SequenceCoroutine);
				m_SequenceCoroutine = StartCoroutine (SequenceReset(0f));
			}
		}

		private IEnumerator SequencePopup (float lerp)
		{
            m_State = TargetState.Raising;
            float inversePopup = 1f / m_PopupDuration;

            // Pop up
            m_Lerp = lerp;
			while (m_Lerp < 1f)
			{
				yield return null;
                m_Lerp += Time.deltaTime * inversePopup;

				// Flip up
				m_RotationTransform.localRotation = Quaternion.AngleAxis (Mathf.Lerp (m_HiddenRotation, 0f, m_Lerp), m_RotationAxis);
			}

            StartRaisedSequence();
		}

        private IEnumerator SequenceRaised()
        {
            m_State = TargetState.Raised;
            while (!hit && m_Timer > 0f)
            {
                yield return null;
                m_Timer -= Time.deltaTime;
            }

            yield return null;

            StartResetSequence();
        }
		
		private IEnumerator SequenceReset (float lerp)
        {
            m_State = TargetState.Lowering;
            float inversePopup = 1f / m_PopupDuration;

			// Pop down
			m_Lerp = lerp;
			while (m_Lerp < 1f)
			{
				yield return null;
                m_Lerp += Time.deltaTime * inversePopup;

				// Flip down
				m_RotationTransform.localRotation = Quaternion.AngleAxis (Mathf.Lerp (0f, m_HiddenRotation, m_Lerp), m_RotationAxis);
			}

            if (m_Hit == true)
                m_Hit = false;
            else
                m_Sequencer.AddMiss();
			
			yield return null;

            m_State = TargetState.Idle;
            m_SequenceCoroutine = null;
        }

        protected virtual void StartRaisedSequence()
        {
            m_SequenceCoroutine = StartCoroutine(SequenceRaised());
        }

        protected virtual void StartResetSequence()
        {
            m_SequenceCoroutine = StartCoroutine(SequenceReset(0f));
        }

        protected virtual void OnSequenceCompleted()
        {

        }

		#region IDamageHandler Implementation

		private DamageFilter m_InDamageFilter = DamageFilter.AllDamageAllTeams;
		public DamageFilter inDamageFilter 
		{
			get { return m_InDamageFilter; }
			set { m_InDamageFilter = value; }
		}

		public DamageResult AddDamage (float damage)
		{
			return AddDamage (damage, null);
		}

		public DamageResult AddDamage (float damage, IDamageSource source)
		{
			if (damage >= m_DamageThreshold && !hidden && !hit)
			{
                m_Hit = true;
				m_Sequencer.AddHit();
				ResetTarget ();

                // Report damage dealt
                if (damage > 0f && source != null && source.controller != null)
                    source.controller.currentCharacter.ReportTargetHit(false);
            }
			return DamageResult.Standard;
		}

        public DamageResult AddDamage(float damage, RaycastHit hit)
        {
            return AddDamage(damage);
        }

        public DamageResult AddDamage(float damage, RaycastHit hit, IDamageSource source)
        {
            return AddDamage(damage, source);
        }

        #endregion

        private static readonly NeoSerializationKey k_StateKey = new NeoSerializationKey("state");
        private static readonly NeoSerializationKey k_LerpKey = new NeoSerializationKey("lerp");
        private static readonly NeoSerializationKey k_TimerKey = new NeoSerializationKey("timer");
        private static readonly NeoSerializationKey k_HitKey = new NeoSerializationKey("hit");

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_SequenceCoroutine != null)
            {
                writer.WriteValue(k_StateKey, (int)m_State);
                writer.WriteValue(k_LerpKey, m_Lerp);
                writer.WriteValue(k_TimerKey, m_Timer);
                writer.WriteValue(k_HitKey, m_Hit);
            }
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            int state = 0;
            if (reader.TryReadValue(k_StateKey, out state, 0))
            {
                m_State = (TargetState)state;
                reader.TryReadValue(k_LerpKey, out m_Lerp, m_Lerp);
                reader.TryReadValue(k_TimerKey, out m_Timer, m_Timer);
                reader.TryReadValue(k_HitKey, out m_Hit, m_Hit);

                switch(m_State)
                {
                    case TargetState.Raising:
                        m_SequenceCoroutine = StartCoroutine(SequencePopup(m_Lerp));
                        break;
                    case TargetState.Raised:
                        m_SequenceCoroutine = StartCoroutine(SequenceRaised());
                        break;
                    case TargetState.Lowering:
                        m_SequenceCoroutine = StartCoroutine(SequenceReset(m_Lerp));
                        break;
                }
            }

            initialised = true;
        }
    }
}