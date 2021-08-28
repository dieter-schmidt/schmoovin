using System;
using System.Collections.Generic;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    public class WaypointMovingPlatform : BaseMovingPlatform
    {
        [SerializeField]
        private Waypoint[] m_Waypoints = new Waypoint[0];

        [SerializeField]
        private float[] m_JourneyTimes = new float[0];

        [SerializeField, Tooltip("The waypoint the platform starts at (will be repositioned on start).")]
        private int m_StartingWaypoint = 0;

        [SerializeField, Tooltip("An animation curve to apply easing to movement between waypoints.")]
        private AnimationCurve m_SpeedCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f));

        [SerializeField, Tooltip("The delay between waypoints when moving through a sequence. The platform will stop at a waypoint for this duration.")]
        private float m_Delay = 0f;

        [SerializeField, Tooltip("If the waypoints are circular then there is a direct route from the first to last waypoints without going through the others.")]
        private bool m_Circular = true;

        [SerializeField, Tooltip("What to do on start. If the waypoints are not circular then looping will ping-pong from the first to last waypoints via the intermediates.")]
        private StartingBehaviour m_OnStart = StartingBehaviour.Nothing;

        [SerializeField, Tooltip("An event fired once the platform starts moving.")]
        private UnityEvent m_OnStartMoving = null;

        [SerializeField, Tooltip("An event fired once the platform has reached its destination.")]
        private UnityEvent m_OnDestinationReached = null;

        private static readonly NeoSerializationKey k_MoveIndicesKey = new NeoSerializationKey("moveIndices");
        private static readonly NeoSerializationKey k_MoveDurationsKey = new NeoSerializationKey("moveDurations");
        private static readonly NeoSerializationKey k_SourceIndexKey = new NeoSerializationKey("sourceIndex");
        private static readonly NeoSerializationKey k_ProgressKey = new NeoSerializationKey("progress");
        private static readonly NeoSerializationKey k_TimeoutKey = new NeoSerializationKey("timeout");
        private static readonly NeoSerializationKey k_LoopDirKey = new NeoSerializationKey("loopDir");

        [Serializable]
        public struct Waypoint
        {
            public Vector3 position;
            public Vector3 rotation;
        }

        private struct MoveSegment
        {
            public int index;
            public float duration;
        }

        public enum StartingBehaviour
        {
            Nothing,
            LoopForwards,
            LoopBackwards
        }

        public event Action<int> onReachedWaypoint;

        private int m_SourceIndex = 0;
        private float m_Progress = 0f;
        private float m_Timeout = 0f;
        private MoveSegment[] m_MoveSegments = null;
        private int m_HeadSegment = 0;
        private int m_SegmentCount = 0;
        private int m_LoopDirection = 0;

        public Waypoint[] waypoints
        {
            get { return m_Waypoints; }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // Check waypoints have been initialised
            if (m_Waypoints == null || m_Waypoints.Length == 0)
            {
                m_Waypoints = new Waypoint[1];
                m_Waypoints[0].position = transform.position;
                m_Waypoints[0].rotation = transform.rotation.eulerAngles;
            }

            // Clamp the starting waypoint
            m_StartingWaypoint = Mathf.Clamp(m_StartingWaypoint, 0, m_Waypoints.Length - 1);

            // Set correct number of journey times for the number of waypoints
            int numJourneys = m_Waypoints.Length;
            if (!m_Circular)
                --numJourneys;
            if (m_JourneyTimes.Length != numJourneys)
            {
                float[] replacement = new float[numJourneys];
                int i = 0;
                for (; i < numJourneys && i < m_JourneyTimes.Length; ++i)
                    replacement[i] = m_JourneyTimes[i];
                for (; i < numJourneys; ++i)
                    replacement[i] = 5f;
                m_JourneyTimes = replacement;
            }

            // Clamp travel times
            for (int i = 0; i < m_JourneyTimes.Length; ++i)
                m_JourneyTimes[i] = Mathf.Clamp(m_JourneyTimes[i], 1f, 60f);
        }
#endif

        void AppendSegment (int index, float duration)
        {
            // Get new index
            int i = m_HeadSegment + m_SegmentCount;
            if (i >= m_MoveSegments.Length)
                i -= m_MoveSegments.Length;

            // Assign properties
            m_MoveSegments[i].index = index;
            m_MoveSegments[i].duration = duration;

            ++m_SegmentCount;
        }

        void PopHeadSegment ()
        {
            // Get source properties
            m_SourceIndex = m_MoveSegments[m_HeadSegment].index;

            ++m_HeadSegment;
            if (m_HeadSegment >= m_MoveSegments.Length)
                m_HeadSegment -= m_MoveSegments.Length;
            --m_SegmentCount;

            if (m_SegmentCount > 0)
                m_Timeout = m_Delay;
            else
                m_OnDestinationReached.Invoke();
        }

        protected override void Awake()
        {
            base.Awake();
            m_MoveSegments = new MoveSegment[m_Waypoints.Length];
        }

        protected override void Start()
        {
            base.Start();

            // Loop on start if set
            if (m_OnStart != StartingBehaviour.Nothing)
                LoopWaypoints(m_OnStart == StartingBehaviour.LoopForwards);
        }

        protected override Vector3 GetStartingPosition()
        {
            m_SourceIndex = Mathf.Clamp(m_StartingWaypoint, 0, m_Waypoints.Length - 1);
            return m_Waypoints[m_SourceIndex].position;
        }

        protected override Quaternion GetStartingRotation()
        {
            m_SourceIndex = Mathf.Clamp(m_StartingWaypoint, 0, m_Waypoints.Length - 1);
            return Quaternion.Euler(m_Waypoints[m_SourceIndex].rotation);
        }

        protected override Vector3 GetNextPosition()
        {
            if (m_SegmentCount == 0)
                return fixedPosition;

            // Timeout if required
            if (m_Timeout > 0f)
            {
                m_Timeout -= Time.deltaTime;
                if (m_Timeout < 0f)
                    m_Timeout = 0f;
                else
                    return fixedPosition;
            }

            // Increment progress
            m_Progress += Time.deltaTime / m_MoveSegments[m_HeadSegment].duration;
            if (m_Progress >= 1f)
            {
                m_Progress = 1f;
                return m_Waypoints[m_MoveSegments[m_HeadSegment].index].position;
            }

            return Vector3.LerpUnclamped(
                m_Waypoints[m_SourceIndex].position,
                m_Waypoints[m_MoveSegments[m_HeadSegment].index].position,
                m_SpeedCurve.Evaluate(m_Progress)
                );
        }

        protected override Quaternion GetNextRotation()
        {
            if (m_SegmentCount == 0)
                return fixedRotation;

            if (m_Timeout > 0f)
                return fixedRotation;

            if (m_Progress == 1f)
            {
                // Reset progress
                m_Progress = 0f;

                // Pop head segment & return position
                int index = m_MoveSegments[m_HeadSegment].index;
                PopHeadSegment();

                // React to reaching waypoint
                OnReachedWaypoint(index);

                return Quaternion.Euler(m_Waypoints[index].rotation);
            }

            // Slerp between source and destination
            return Quaternion.SlerpUnclamped(
                Quaternion.Euler(m_Waypoints[m_SourceIndex].rotation),
                Quaternion.Euler(m_Waypoints[m_MoveSegments[m_HeadSegment].index].rotation),
                m_SpeedCurve.Evaluate(m_Progress)
                );
        }

        protected virtual void OnReachedWaypoint(int wp)
        {
            // Loop if set
            if (m_LoopDirection != 0)
            {
                m_Timeout = m_Delay;
                GetNextLoopWaypoint();
            }

            // Fire waypoint reached event
            if (onReachedWaypoint != null)
                onReachedWaypoint(wp);
        }

        public void GoToWaypoint(int index, bool direct)
        {
            // Check if valid
            if (index < 0 || index >= m_Waypoints.Length)
                return;

            int source = m_SourceIndex;
            m_LoopDirection = 0;

            // Prevent delay
            if (m_Timeout > 0f)
            {
                m_Timeout = 0f;
                // Clear pending moves
                m_SegmentCount = 0;
            }
            else
            {
                // Trim all but current move if one is in progress
                if (m_SegmentCount > 1)
                {
                    // Source is current move (when complete)
                    m_SegmentCount = 1;
                    source = m_MoveSegments[0].index;
                }
            }

            // If source is target then complete
            if (source == index)
                return;
            
            bool forwards;
            if (m_Circular)
            {
                int diff = index - source;
                int halfLength = m_Waypoints.Length / 2;
                forwards = (diff > 0 && diff <= halfLength) || (diff < 0 && diff < -halfLength);
            }
            else
            {
                forwards = index > source;
            }

            int itr = source;
            float duration = 0f;
            if (forwards)
            {
                // Walk forwards through waypoints
                while (itr != index)
                {
                    // Record source waypoint
                    int original = itr;

                    // Iterate through waypoints
                    ++itr;
                    if (itr >= m_Waypoints.Length)
                        itr -= m_Waypoints.Length;

                    if (direct)
                    {
                        // Accumulate duration
                        duration += m_JourneyTimes[original];
                        // Set position at the end
                        if (itr == index)
                            AppendSegment(itr, duration);
                    }
                    else
                        AppendSegment(itr, m_JourneyTimes[original]);
                }
            }
            else
            {
                // Walk backwards through waypoints
                while (itr != index)
                {
                    // Iterate through waypoints
                    --itr;
                    if (itr < 0)
                        itr += m_Waypoints.Length;

                    if (direct)
                    {
                        // Accumulate duration
                        duration += m_JourneyTimes[itr];
                        // Set position at the end
                        if (itr == index)
                            AppendSegment(itr, duration);
                    }
                    else
                        AppendSegment(itr, m_JourneyTimes[itr]);
                }
            }

            m_OnStartMoving.Invoke();
        }

        public void GoToWaypoint(int index)
        {
            GoToWaypoint(index, false);
        }

        public void LoopWaypoints (bool forward)
        {
            if (m_Waypoints.Length == 1)
                return;

            m_LoopDirection = (forward) ? 1 : -1;

            // If waiting, stop timer and clear moves
            if (m_Timeout > 0f)
            {
                m_Timeout = 0f;
                m_SegmentCount = 0;
            }
            else
            {
                // If move in progress, drop subsequent moves
                if (m_SegmentCount > 1)
                {
                    m_SegmentCount = 1;
                    return; // Looping handled on reached waypoint
                }
            }

            GetNextLoopWaypoint();
        }

        void GetNextLoopWaypoint ()
        {
            // If waypoints aren't circular, ping pong
            if (!m_Circular)
            {
                if (m_SourceIndex == 0)
                    m_LoopDirection = 1;
                if (m_SourceIndex == m_Waypoints.Length - 1)
                    m_LoopDirection = -1;
            }

            // Get wrapped index
            int index = m_SourceIndex + m_LoopDirection;
            if (index < 0)
                index += m_Waypoints.Length;
            if (index >= m_Waypoints.Length)
                index -= m_Waypoints.Length;

            // Append segment
            if (m_LoopDirection > 0)
                AppendSegment(index, m_JourneyTimes[m_SourceIndex]);
            else
                AppendSegment(index, m_JourneyTimes[index]);
        }

        public void Stop()
        {
            m_LoopDirection = 0;

            // If waiting, stop timer and clear moves
            if (m_Timeout > 0f)
            {
                m_Timeout = 0f;
                m_SegmentCount = 0;
            }
            else
            {
                // If move in progress, drop subsequent moves
                if (m_SegmentCount > 1)
                    m_SegmentCount = 1;
            }
        }

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

            if (m_SegmentCount > 0)
            {
                // Gather move segments
                int[] indices = new int[m_SegmentCount];
                float[] durations = new float[m_SegmentCount];
                for (int i = 0; i < m_SegmentCount; ++i)
                {
                    int index = m_HeadSegment + i;
                    if (index > m_MoveSegments.Length)
                        index -= m_MoveSegments.Length;

                    indices[i] = m_MoveSegments[i].index;
                    durations[i] = m_MoveSegments[i].duration;
                }

                writer.WriteValues(k_MoveIndicesKey, indices);
                writer.WriteValues(k_MoveDurationsKey, durations);
            }

            writer.WriteValue(k_SourceIndexKey, m_SourceIndex);
            writer.WriteValue(k_ProgressKey, m_Progress);
            writer.WriteValue(k_TimeoutKey, m_Timeout);
            writer.WriteValue(k_LoopDirKey, m_LoopDirection);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            reader.TryReadValue(k_SourceIndexKey, out m_SourceIndex, m_SourceIndex);
            reader.TryReadValue(k_ProgressKey, out m_Progress, m_Progress);
            reader.TryReadValue(k_TimeoutKey, out m_Timeout, m_Timeout);
            reader.TryReadValue(k_LoopDirKey, out m_LoopDirection, m_LoopDirection);

            int[] indices = null;
            reader.TryReadValues(k_MoveIndicesKey, out indices, new int[0]);
            float[] durations = null;
            reader.TryReadValues(k_MoveDurationsKey, out durations, new float[0]);

            for (int i = 0; i < indices.Length; ++i)
            {
                m_MoveSegments[i].index = indices[i];
                m_MoveSegments[i].duration = durations[i];
            }
            m_HeadSegment = 0;
            m_SegmentCount = indices.Length;
        }
    }
}