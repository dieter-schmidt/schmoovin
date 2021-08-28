using NeoSaveGames;
using NeoSaveGames.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-raycasttargetingsystem.html")]
    public class RaycastTargetingSystem : MonoBehaviour, ITargetingSystem, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The layers to cast against.")]
        private LayerMask m_RaycastLayers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Tooltip("The max distance for targets to home in on.")]
        private float m_MaxDistance = 500f;

        [SerializeField, Tooltip("Should the targetying system update the projectiles each tick.")]
        private bool m_Continuous = false;

        [SerializeField, Tooltip("The targeting system will only check for new targets after the min interval has elapsed since the last time, passing the last target to any new trackers before that. This is useful for burst launchers so that each missile tracks the same target.")]
        private float m_MinInterval = 1f;

        [SerializeField, Tooltip("When set to use camera aim, the targeting system casts forward from the FirstPersonCamera's aim transform. If not then it casts from the transform the targeting system is attached to.")]
        private UseCameraAim m_UseCameraAim = UseCameraAim.HipAndAimDownSights;

        private Coroutine m_CastCoroutine = null;
        private WaitForFixedUpdate m_WaitForFixedUpdate = new WaitForFixedUpdate();
        private List<ITargetTracker> m_Trackers = null;
        private IModularFirearm m_Firearm = null;
        private RaycastHit m_Hit = new RaycastHit();
        private float m_NextCast = 0f;
        private bool m_DidHit = false;

        void OnValidate()
        {
            m_MaxDistance = Mathf.Clamp(m_MaxDistance, 5f, 500f);
            m_MinInterval = Mathf.Clamp(m_MinInterval, 0f, 10f);
        }

        void Awake()
        {
            m_Firearm = GetComponentInParent<ModularFirearm>();
            if (m_Continuous)
                m_Trackers = new List<ITargetTracker>();
        }

        void OnDisable()
        {
            if (m_Trackers != null)
            {
                // Stop the casting coroutine
                if (m_CastCoroutine != null)
                    StopCoroutine(m_CastCoroutine);

                // Forget the existing trackers
                for (int i = 0; i < m_Trackers.Count; ++i)
                    m_Trackers[i].onDestroyed -= OnTrackerDestroyed;
                m_Trackers.Clear();
            }
        }

        public void RegisterTracker(ITargetTracker tracker)
        {
            // Check if a new cast is required
            if (Time.timeSinceLevelLoad > m_NextCast)
            {
                // Set next cast time
                m_NextCast = Time.timeSinceLevelLoad + m_MinInterval;

                // perform a cast
                Raycast();
            }

            if (m_Continuous)
            {
                m_Trackers.Add(tracker);
                tracker.onDestroyed += OnTrackerDestroyed;

                // Start casting coroutine
                if (m_CastCoroutine == null)
                    m_CastCoroutine = StartCoroutine(CastCoroutine());
            }

            // Send the target hit to the tracker
            if (m_DidHit)
                tracker.SetTargetPosition(m_Hit.point);
            else
                tracker.ClearTarget();
        }

        private void OnTrackerDestroyed(ITargetTracker tracker)
        {
            m_Trackers.Remove(tracker);
            tracker.onDestroyed -= OnTrackerDestroyed;
        }

        IEnumerator CastCoroutine()
        {
            while (m_Trackers.Count > 0)
            {
                yield return m_WaitForFixedUpdate;

                // Check if a new cast is required
                if (Time.timeSinceLevelLoad > m_NextCast)
                {
                    // Set next cast time
                    m_NextCast = Time.timeSinceLevelLoad + m_MinInterval;

                    // perform a cast
                    Raycast();

                    // Apply new hit position
                    if (m_DidHit)
                    {
                        for (int i = 0; i < m_Trackers.Count; ++i)
                            m_Trackers[i].SetTargetPosition(m_Hit.point);
                    }
                }
            }

            m_CastCoroutine = null;
        }

        void Raycast()
        {
            bool useCamera = false;
            if (m_Firearm != null && m_Firearm.wielder != null)
            {
                switch (m_UseCameraAim)
                {
                    case UseCameraAim.HipAndAimDownSights:
                        useCamera = true;
                        break;
                    case UseCameraAim.AimDownSightsOnly:
                        if (m_Firearm.aimer != null)
                            useCamera = m_Firearm.aimer.isAiming;
                        break;
                    case UseCameraAim.HipFireOnly:
                        if (m_Firearm.aimer != null)
                            useCamera = !m_Firearm.aimer.isAiming;
                        else
                            useCamera = true;
                        break;
                }
            }

            if (useCamera)
            {
                var camTransform = m_Firearm.wielder.fpCamera.transform;
                Ray ray = new Ray(camTransform.position, camTransform.forward);
                m_DidHit = PhysicsExtensions.RaycastNonAllocSingle(ray, out m_Hit, m_MaxDistance, m_RaycastLayers, camTransform.root, QueryTriggerInteraction.Ignore);
            }
            else
            {
                var t = transform;
                Ray ray = new Ray(t.position, t.forward);
                m_DidHit = PhysicsExtensions.RaycastNonAllocSingle(ray, out m_Hit, m_MaxDistance, m_RaycastLayers, t.root, QueryTriggerInteraction.Ignore);
            }
        }

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_ActiveTrackersKey = new NeoSerializationKey("activeTrackers");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_Trackers.Count > 0)
            {
                // Push a context (workaround for no read/write obj reference arrays)
                writer.PushContext(SerializationContext.ObjectNeoSerialized, k_ActiveTrackersKey);

                // Write object references
                for (int i = 0; i < m_Trackers.Count; ++i)
                    writer.WriteComponentReference(i, m_Trackers[i], nsgo);

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
                        m_Trackers.Add(tracker);
                    else
                        break;
                }

                reader.PopContext(SerializationContext.ObjectNeoSerialized, k_ActiveTrackersKey);

                if (m_Trackers.Count > 0 && m_CastCoroutine == null)
                    m_CastCoroutine = StartCoroutine(CastCoroutine());
            }
        }

        #endregion
    }
}