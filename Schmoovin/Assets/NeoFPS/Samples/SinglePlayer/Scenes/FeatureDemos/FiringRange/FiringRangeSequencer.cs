using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.Samples.SinglePlayer
{
	[HelpURL("https://docs.neofps.com/manual/samplesref-mb-firingrangesequencer.html")]
	public class FiringRangeSequencer : MonoBehaviour, INeoSerializableComponent
	{
		[SerializeField, Range (1.5f, 10f), Tooltip("The pause in between each wave.")]
		private float m_TimeBetweenWaves = 3f;

        [SerializeField, Tooltip("The targets for each wave.")]
        private TargetGroup[] m_Targets = new TargetGroup[0];

        [SerializeField, Tooltip("An event that is invoked when a target is hit.")]
        private IntEvent m_OnHitsChanged = new IntEvent();

        [SerializeField, Tooltip("An event that is invoked when a target is missed.")]
        private IntEvent m_OnMissesChanged = new IntEvent();

		[SerializeField, Tooltip("The audio source for playing one shot firing range audio clips.")]
		private AudioSource m_AudioSource = null;

        [SerializeField, Tooltip("The audio clip to play when the sequence starts.")]
		private AudioClip m_AudioStart = null;

        [SerializeField, Tooltip("The audio clip to play when the sequence is cancelled.")]
		private AudioClip m_AudioCancel = null;

        [SerializeField, Tooltip("The audio clip to play when a target is hit.")]
		private AudioClip m_AudioHit = null;

        private Coroutine m_SequenceCoroutine = null;
        private int m_Wave = 0;
        private int m_Spawned = 0;
        private int m_TargetCount = 0;
        private float m_Timer = 0f;
        private float m_ButtonCooldown = 0f;
        private SequenceState m_State = SequenceState.Stopped;

        private enum SequenceState
        {
            Stopped,
            WaveStart,
            WavePhase,
            Reset,
            Waiting
        }

        [Serializable]
		public class IntEvent : UnityEvent<int> {}

		[Serializable]
		public class TargetGroup
		{
			[Tooltip("The targets for this wave.")]
			public FiringRangeTarget[] targets;
			[Tooltip("The total number of targets to pop up this wave.")]
			public int total = 5;
			[Tooltip("The number of targets to pop up for each step of the wave.")]
			public int perStep = 1;
			[Tooltip("Should the targets be chosen at random or in sequence.")]
			public bool randomise = false;
			[Tooltip("The duration a target should stay up.")]
			public float duration = 5f;
			[Tooltip("The delay between steps.")]
			public float delay = 6f;

            public void OnValidate ()
            {
                if (total < 1)
                    total = 1;
                if (perStep < 1)
                    perStep = 1;
                if (duration < 0.5f)
                    duration = 0.5f;
                if (delay < 0.5f)
                    delay = 0.5f;
            }
		}

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_Targets != null)
            {
                for (int i = 0; i < m_Targets.Length; ++i)
                    m_Targets[i].OnValidate();
            }
        }
#endif

        private int m_Hits = 0;
		public int hits
		{
			get { return m_Hits; }
			private set
			{
				m_Hits = value;
				m_OnHitsChanged.Invoke(m_Hits);
			}
		}

        private int m_Misses = 0;
		public int misses
		{
			get { return m_Misses; }
			private set
			{
				m_Misses = value;
				m_OnMissesChanged.Invoke(m_Misses);
			}
		}

		public bool interactable
		{
			get { return m_SequenceCoroutine == null && m_ButtonCooldown <= 0f; }
		}

		void Start()
		{
			FiringRangeTarget[] collected = GetComponentsInChildren<FiringRangeTarget>(true);
            for (int i = 0; i < collected.Length; ++i)
                collected[i].Initialise(this);

            if (!m_Initialised)
            {
                hits = 0;
                misses = 0;
            }
            else
            {
                m_OnHitsChanged.Invoke(m_Hits);
                m_OnMissesChanged.Invoke(m_Misses);
            }
        }

        void Update()
        {
            if (m_ButtonCooldown > 0f)
            {
                m_ButtonCooldown -= Time.deltaTime;
                if (m_ButtonCooldown < 0f)
                    m_ButtonCooldown = 0f;
            }
        }

		public void AddHit ()
		{
			++hits;
            m_AudioSource.PlayOneShot(m_AudioHit);
        }

		public void AddMiss ()
		{
			++misses;
		}

		public void OnButtonPush ()
		{
            if (m_ButtonCooldown <= 0f)
            {
                // If ongoing, stop
                if (m_SequenceCoroutine != null)
                {
                    if (m_State != SequenceState.Reset)
                    {
                        StopAllCoroutines();
                        m_SequenceCoroutine = StartCoroutine(ResetTargets());
                        m_AudioSource.PlayOneShot(m_AudioCancel, 0.25f);
                    }
                }
                else
                {
                    // Else start
                    m_Wave = 0;
                    hits = 0;
                    misses = 0;
                    m_SequenceCoroutine = StartCoroutine(WaveStart(m_TimeBetweenWaves));
                    m_AudioSource.PlayOneShot(m_AudioStart, 0.25f);
                }

                m_ButtonCooldown = 3f;
            }
        }
        
        private IEnumerator WaveStart(float timer)
        {
            m_State = SequenceState.WaveStart;
            m_Timer = timer;

            yield return null;

            // Play wave start audio 1 sec before timer ends
            if (m_Timer > 1f)
            {
                while (m_Timer > 1f)
                {
                    yield return null;
                    m_Timer -= Time.deltaTime;
                }
                m_AudioSource.PlayOneShot(m_AudioStart, 0.25f);
            }

            // Wait for timer
            while (m_Timer > 0f)
            {
                yield return null;
                m_Timer -= Time.deltaTime;
            }

            m_Spawned = 0;
            m_TargetCount = m_Targets[m_Wave].perStep;
            m_SequenceCoroutine = StartCoroutine(WavePhase(0f));
        }

        private IEnumerator WavePhase(float timer)
        {
            m_State = SequenceState.WavePhase;
            var group = m_Targets[m_Wave];

            yield return null;

            // Wait for step timer
            m_Timer = timer;
            while (m_Timer > 0f)
            {
                yield return null;
                m_Timer -= Time.deltaTime;
            }            

            // Spawn targets
            if (group.randomise)
            {
                // Pick at random
                while (m_Spawned < m_TargetCount)
                {
                    int i = UnityEngine.Random.Range(0, group.targets.Length);
                    if (group.targets[i] != null && group.targets[i].hidden)
                    {
                        // Trigger
                        group.targets[i].Popup(group.duration);
                        ++m_Spawned;
                    }
                    else
                        yield return null; // Yield to prevent endless loop
                }
            }
            else
            {
                // Trigger sequentially
                while (m_Spawned < m_TargetCount)
                {
                    int index = m_Spawned;
                    while (index >= group.targets.Length)
                        index -= group.targets.Length;
                    if (group.targets[index] != null)
                        group.targets[index].Popup(group.duration);
                    ++m_Spawned;
                }
            }

            if (m_Spawned < group.total)
            {
                m_TargetCount += group.perStep;
                m_SequenceCoroutine = StartCoroutine(WavePhase(group.delay));
            }
            else
            {
                m_SequenceCoroutine = StartCoroutine(WaitForReset(m_Wave + 1 >= m_Targets.Length));
            }
        }

		private IEnumerator ResetTargets ()
		{
            for (int i = 0; i < m_Targets[m_Wave].targets.Length; ++i)
			{
				FiringRangeTarget t = m_Targets[m_Wave].targets[i];
				if (t != null && !t.hidden)
					t.ResetTarget();
			}

            return WaitForReset(true);
		}

        private IEnumerator WaitForReset(bool completed)
        {
            if (completed)
                m_State = SequenceState.Reset;
            else
                m_State = SequenceState.Waiting;

            yield return null;

            bool allTargetsHidden = false;
            while (!allTargetsHidden)
            {
                yield return null;

                allTargetsHidden = true;
                for (int i = 0; i < m_Targets[m_Wave].targets.Length; ++i)
                {
                    FiringRangeTarget t = m_Targets[m_Wave].targets[i];
                    if (t != null && !t.hidden)
                    {
                        allTargetsHidden = false;
                        break;
                    }
                }
            }

            // Reset the sequence if completed or start next wave
            if (completed)
            {
                m_Wave = 0;
                m_State = SequenceState.Stopped;
                m_SequenceCoroutine = null;
            }
            else
            {
                ++m_Wave;
                m_SequenceCoroutine = StartCoroutine(WaveStart(m_TimeBetweenWaves));
            }
        }

        private static readonly NeoSerializationKey k_HitsKey = new NeoSerializationKey("hits");
        private static readonly NeoSerializationKey k_MissesKey = new NeoSerializationKey("misses");
        private static readonly NeoSerializationKey k_StateKey = new NeoSerializationKey("state");
        private static readonly NeoSerializationKey k_TimerKey = new NeoSerializationKey("timer");
        private static readonly NeoSerializationKey k_WaveKey = new NeoSerializationKey("wave");
        private static readonly NeoSerializationKey k_SpawnedKey = new NeoSerializationKey("spawned");
        private static readonly NeoSerializationKey k_TargetCountKey = new NeoSerializationKey("targetCount");

        private bool m_Initialised = false;

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            // Write properties
            writer.WriteValue(k_HitsKey, m_Hits);
            writer.WriteValue(k_MissesKey, m_Misses);

            // Write coroutines
            if (m_SequenceCoroutine != null)
            {
                writer.WriteValue(k_StateKey, (int)m_State);
                writer.WriteValue(k_TimerKey, m_Timer);
                writer.WriteValue(k_WaveKey, m_Wave);
                writer.WriteValue(k_SpawnedKey, m_Spawned);
                writer.WriteValue(k_TargetCountKey, m_TargetCount);
            }
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_HitsKey, out m_Hits, m_Hits);
            reader.TryReadValue(k_MissesKey, out m_Misses, m_Misses);

            int state = 0;
            if (reader.TryReadValue(k_StateKey, out state, 0))
            {
                m_State = (SequenceState)state;
                reader.TryReadValue(k_TimerKey, out m_Timer, m_Timer);
                reader.TryReadValue(k_WaveKey, out m_Wave, m_Wave);
                reader.TryReadValue(k_SpawnedKey, out m_Spawned, m_Spawned);
                reader.TryReadValue(k_TargetCountKey, out m_TargetCount, m_TargetCount);
                
                switch (m_State)
                {
                    case SequenceState.WaveStart:
                        m_SequenceCoroutine = StartCoroutine(WaveStart(m_Timer));
                        break;
                    case SequenceState.WavePhase:
                        m_SequenceCoroutine = StartCoroutine(WavePhase(m_Timer));
                        break;
                    case SequenceState.Waiting:
                        m_SequenceCoroutine = StartCoroutine(WaitForReset(false));
                        break;
                    case SequenceState.Reset:
                        m_SequenceCoroutine = StartCoroutine(WaitForReset(true));
                        break;
                }
            }

            m_Initialised = true;
        }
    }
}