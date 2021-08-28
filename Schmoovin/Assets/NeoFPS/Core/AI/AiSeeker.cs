using System.Collections;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
	public abstract class AiSeeker : MonoBehaviour, INeoSerializableComponent
    {
        private static readonly NeoSerializationKey k_StateKey = new NeoSerializationKey("state");

        public enum State
		{
			Idle,
			Suspicious,
			Engaged,
			Hunting,
			Dead
		}

		public delegate void StateChangeDelegate (State from, State to);
		public event StateChangeDelegate onStateChanged;

		private State m_State = State.Idle;
		public State state
		{
			get { return m_State; }
			set
			{
				if (m_State != value)
				{
					State from = m_State;

					m_State = value;

                    if (isActiveAndEnabled)
                    {
                        // Stop old state coroutine
                        if (m_CurrentStateCoroutine != null)
                            StopCoroutine(m_CurrentStateCoroutine);
                        // Start new state coroutine
                        m_CurrentStateCoroutine = EnterState(m_State);
                        // React to change
                        OnStateChanged(from, m_State);
                    }
				}
			}
		}

		private Coroutine m_CurrentStateCoroutine = null;

		protected abstract IEnumerator IdleCoroutine ();
		protected abstract IEnumerator SuspiciousCoroutine ();
		protected abstract IEnumerator EngagedCoroutine ();
		protected abstract IEnumerator HuntingCoroutine ();
		protected abstract IEnumerator DeadCoroutine ();

		private Coroutine EnterState (State s)
		{
			switch (s)
			{
				case State.Idle:
                    return StartCoroutine (IdleCoroutine ());
				case State.Suspicious:
                    return StartCoroutine (SuspiciousCoroutine ());
				case State.Engaged:
                    return StartCoroutine (EngagedCoroutine ());
				case State.Hunting:
                    return StartCoroutine (HuntingCoroutine ());
				default:
                    return StartCoroutine (DeadCoroutine ());
			}
		}

		protected virtual void Start ()
		{
			m_CurrentStateCoroutine = EnterState (m_State);
		}

        protected virtual void OnStateChanged(State from, State to)
        {
            if (onStateChanged != null)
                onStateChanged(from, to);
        }

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_StateKey, (int)m_State);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            int s = 0;
            if (reader.TryReadValue(k_StateKey, out s, 0))
            {
                m_State = (State)s;
                if (isActiveAndEnabled)
                {
                    // Stop old state coroutine
                    if (m_CurrentStateCoroutine != null)
                        StopCoroutine(m_CurrentStateCoroutine);
                    // Start new state coroutine
                    m_CurrentStateCoroutine = EnterState(m_State);
                }
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate ()
		{
		}
#endif
    }
}