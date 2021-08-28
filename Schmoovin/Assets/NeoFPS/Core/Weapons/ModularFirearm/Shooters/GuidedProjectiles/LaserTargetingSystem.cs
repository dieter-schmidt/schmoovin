using NeoSaveGames;
using NeoSaveGames.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-lasertargetingsystem.html")]
    public class LaserTargetingSystem : MonoBehaviour, ITargetingSystem, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The laser pointer hit location will be updated every nth fixed update frame.")]
        private int m_TickRate = 10;

        private List<ITargetTracker> m_ActiveTrackers = new List<ITargetTracker>();
        WieldableLaserPointer m_LaserPointer = null;
        private int m_Ticker = 0;

        void OnValidate()
        {
            m_TickRate = Mathf.Clamp(m_TickRate, 1, 100);
        }

        void Awake()
        {
            m_LaserPointer = GetComponentInChildren<WieldableLaserPointer>();
        }

        public void RegisterTracker(ITargetTracker tracker)
        {
            if (m_LaserPointer != null)
            {
                m_ActiveTrackers.Add(tracker);
                tracker.onDestroyed += OnTrackerDestroyed;

                if (m_ActiveTrackers.Count == 1)
                {
                    CheckLaserHitPoint();
                    m_Ticker = m_TickRate + 1;
                }
            }
        }

        private void OnTrackerDestroyed(ITargetTracker tracker)
        {
            tracker.onDestroyed -= OnTrackerDestroyed;
            m_ActiveTrackers.Remove(tracker);
        }

        void FixedUpdate()
        {
            if (m_ActiveTrackers.Count > 0 && --m_Ticker <= 0)
            {
                m_Ticker = m_TickRate;
                CheckLaserHitPoint();
            }  
        }

        void CheckLaserHitPoint()
        {
            Vector3 hitPoint;
            if (m_LaserPointer.CheckDoesHit(out hitPoint))
            {
                for (int i = 0; i < m_ActiveTrackers.Count; ++i)
                    m_ActiveTrackers[i].SetTargetPosition(hitPoint);
            }
            else
            {
                for (int i = 0; i < m_ActiveTrackers.Count; ++i)
                {
                    if (m_ActiveTrackers[i].hasTarget)
                        m_ActiveTrackers[i].ClearTarget();
                }
            }
        }

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_ActiveTrackersKey = new NeoSerializationKey("activeTrackers");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_ActiveTrackers.Count > 0)
            {
                // Push a context (workaround for no read/write obj reference arrays)
                writer.PushContext(SerializationContext.ObjectNeoSerialized, k_ActiveTrackersKey);

                // Write object references
                for (int i = 0; i < m_ActiveTrackers.Count; ++i)
                    writer.WriteComponentReference(i, m_ActiveTrackers[i], nsgo);

                // Pop context
                writer.PopContext(SerializationContext.ObjectNeoSerialized);
            }
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            // Push a context (workaround for no read/write obj reference arrays)
            // If context isn't found, the array was empty
            if (reader.PushContext(SerializationContext.ObjectNeoSerialized, k_ActiveTrackersKey))
            {
                // Read object references
                for (int i = 0; true; ++i)
                {
                    ITargetTracker tracker;
                    if (reader.TryReadComponentReference(i, out tracker, null))
                        m_ActiveTrackers.Add(tracker);
                    else
                        break;
                }

                reader.PopContext(SerializationContext.ObjectNeoSerialized, k_ActiveTrackersKey);
            }
        }

        #endregion
    }
}