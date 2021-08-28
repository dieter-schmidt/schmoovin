using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Events;
using System.Collections.Generic;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-multitargetlocktrigger.html")]
    public class MultiTargetLockTrigger : BaseTriggerBehaviour, ITargetingSystem, ITargetLock
    {
        [Header ("Target Lock")]

        [SerializeField, Tooltip("The object tag to home in on.")]
        private string m_DetectionTag = "AI";

        [SerializeField, Tooltip("The layers to check for colliders on.")]
        private LayerMask m_DetectionLayers = PhysicsFilter.Masks.Characters;

        [SerializeField, Tooltip("The angle of the cone within which objects can be detected.")]
        private float m_DetectionConeAngle = 75f;

        [SerializeField, Tooltip("The maximum distance that objects can be detected from the wielder.")]
        private float m_DetectionRange = 200f;

        [SerializeField, Tooltip("The time between queueing target locks.")]
        private int m_LockSpacing = 50;

        [SerializeField, Tooltip("The time between failing to find a target and trying again.")]
        private int m_LockRetrySpacing = 25;

        [SerializeField, Tooltip("Does a target lock require an unobstructed line of sight to the target.")]
        private bool m_RequireLineOfSight = true;

        [SerializeField, Tooltip("The layers that can obstruct line of sight to the target.")]
        private LayerMask m_BlockingLayers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Tooltip("Should the first target lock be instant or wait for the same time as subsequent locks")]
        private bool m_DelayFirst = false;

        [Header ("Burst Fire")]

        [SerializeField, Tooltip("The maximum number of shots that can be enqueued.")]
        private int m_MaxQueueSize = 6;

        [SerializeField, Tooltip("Cooldown between shots fired (number of fixed update frames).")]
        private int m_BurstSpacing = 5;

        [SerializeField, Tooltip("The minimum amount of time (fixed update frames) after firing before you can queue more shots again.")]
        private int m_RepeatDelay = 25;

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Bool, true, true), Tooltip("The bool animator property key to set while the trigger is pressed.")]
        private string m_TriggerHoldAnimKey = string.Empty;

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Int, true, true), Tooltip("")]
        private string m_QueueCountAnimKey = string.Empty;

        public event UnityAction<Collider, bool> onTargetLock;
        public event UnityAction<Collider> onTargetLockBroken;
        public event UnityAction<int> onQueueCountChanged;

#pragma warning disable CS0067
        public event UnityAction<Collider, float> onTargetLockStrengthChanged;
#pragma warning restore CS0067

        private static Collider[] s_OverlapColliders = new Collider[256];

        private List<Collider> m_CurrentLocks = new List<Collider>();
        private int m_TriggerHoldHash = -1;
        private int m_QueueCountHash = -1;
        private bool m_Triggered = false;
        private bool m_CheckTag = false;
        private bool m_Shooting = false;
        private Transform m_CameraTransform = null;
        private int m_Cooldown = 0;
        private int m_QueueTicker = 0;

        private RaycastHit m_Hit = new RaycastHit();

        enum State
        {
            Idle,
            PendingLock,
            LockedOn
        }

        public int currentQueueCount
        {
            get { return m_CurrentLocks.Count; }
        }

        public override bool cancelOnReload
        {
            get { return (m_CurrentLocks.Count > 0); }
        }

        void AddTarget(Collider target)
        {
            // Add the target
            m_CurrentLocks.Add(target);
            var count = m_CurrentLocks.Count;

            // Fire target lock event
            if (onTargetLock != null)
                onTargetLock(target, false);

            // Fire queue size changed event
            if (onQueueCountChanged != null)
                onQueueCountChanged(count);

            // Update animator
            if (firearm.animator != null && m_QueueCountHash != -1)
                firearm.animator.SetInteger(m_QueueCountHash, count);
        }

        void RemoveFirstTarget()
        {
            if (m_CurrentLocks.Count > 0)
            {
                // Fire target lost event
                if (onTargetLockBroken != null)
                    onTargetLockBroken(m_CurrentLocks[0]);

                // Remove the target
                m_CurrentLocks.RemoveAt(0);
                var count = m_CurrentLocks.Count;

                // Fire queue size changed event
                if (onQueueCountChanged != null)
                    onQueueCountChanged(count);

                // Update animator
                if (firearm.animator != null && m_QueueCountHash != -1)
                    firearm.animator.SetInteger(m_QueueCountHash, count);
            }
        }

        void ClearTargets()
        {
            if (m_CurrentLocks.Count > 0)
            {
                // Fire target lost events
                if (onTargetLockBroken != null)
                {
                    for (int i = 0; i < m_CurrentLocks.Count; ++i)
                        onTargetLockBroken(m_CurrentLocks[i]);
                }

                // Add the target
                m_CurrentLocks.Clear();

                // Fire queue size changed event
                if (onQueueCountChanged != null)
                    onQueueCountChanged(0);

                // Update animator
                if (firearm.animator != null && m_QueueCountHash != -1)
                    firearm.animator.SetInteger(m_QueueCountHash, 0);
            }
        }

        void CheckTargets()
        {
            // Record old count
            int oldCount = m_CurrentLocks.Count;

            // Check for null entries (the collider was destroyed)
            for (int i = m_CurrentLocks.Count - 1; i >= 0; --i)
            {
                if (m_CurrentLocks[i] == null)
                    m_CurrentLocks.RemoveAt(i);
            }

            // Send events if required
            int newCount = m_CurrentLocks.Count;
            if (oldCount != newCount)
            {
                // Fire queue size changed event
                if (onQueueCountChanged != null)
                    onQueueCountChanged(newCount);

                // Update animator
                if (firearm.animator != null && m_QueueCountHash != -1)
                    firearm.animator.SetInteger(m_QueueCountHash, newCount);
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_MaxQueueSize < 2)
                m_MaxQueueSize = 2;
            if (m_LockSpacing < 1)
                m_LockSpacing = 1;
            if (m_LockRetrySpacing < 1)
                m_LockRetrySpacing = 1;
            if (m_BurstSpacing < 1)
                m_BurstSpacing = 1;
            if (m_RepeatDelay < 1)
                m_RepeatDelay = 1;

            m_DetectionConeAngle = Mathf.Clamp(m_DetectionConeAngle, 0.01f, 90f);
            m_DetectionRange = Mathf.Clamp(m_DetectionRange, 5f, 500f);
        }
#endif

        protected override void Awake()
        {
            base.Awake();

            if (m_TriggerHoldAnimKey != string.Empty)
                m_TriggerHoldHash = Animator.StringToHash(m_TriggerHoldAnimKey);
            if (m_QueueCountAnimKey != string.Empty)
                m_QueueCountHash = Animator.StringToHash(m_QueueCountAnimKey);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (firearm != null && firearm.wielder != null)
                m_CameraTransform = firearm.wielder.fpCamera.transform;
            else
                m_CameraTransform = null;

            m_CheckTag = !string.IsNullOrWhiteSpace(m_DetectionTag);
        }

        Collider GetBestAvailableTarget()
        {
            if (m_CameraTransform == null)
                return null;

            // Get overlapping targets
            var localPosition = m_CameraTransform.position;
            var camForward = m_CameraTransform.forward;
            int overlaps = Physics.OverlapSphereNonAlloc(localPosition, m_DetectionRange, s_OverlapColliders, m_DetectionLayers);
            Transform ignoreRoot = m_CameraTransform.root;

            Collider target = null;

            // Find closest valid
            float smallestAngle = m_DetectionConeAngle * 0.5f;
            for (int i = 0; i < overlaps; ++i)
            {
                // Check tag
                if (m_CheckTag && !s_OverlapColliders[i].CompareTag(m_DetectionTag))
                    continue;

                // Check if already locked on
                if (m_CurrentLocks.Contains(s_OverlapColliders[i]))
                    continue;

                // Check distance && angle
                var center = s_OverlapColliders[i].bounds.center;
                var offset = center - localPosition;
                var angle = Vector3.Angle(offset, camForward);
                if (angle < smallestAngle)
                {
                    // Check line of sight
                    if (m_RequireLineOfSight && PhysicsExtensions.RaycastNonAllocSingle(
                        new Ray(localPosition, offset.normalized), out m_Hit, m_DetectionRange,
                        m_BlockingLayers, ignoreRoot) && !CheckIfHit(s_OverlapColliders[i]))
                    {
                        //Debug.Log("Failing line of sight");
                        //Debug.DrawLine(localPosition, localPosition + offset.normalized * m_Hit.distance, Color.red, 2f);
                        continue;
                    }

                    smallestAngle = angle;
                    target = s_OverlapColliders[i];
                }
            }

            return target;
        }

        bool CheckIfHit(Collider c)
        {
            // Check if it's the same collider
            if (m_Hit.collider == c)
                return true;

            // Check if it's the same rigidbody
            if (m_Hit.rigidbody != null)
                return c.attachedRigidbody == m_Hit.rigidbody;

            // Check if it's the same root transform
            if (m_Hit.transform.root == c.transform.root)
                return true;

            return false;
        }

        public override bool pressed
        {
            get { return m_Triggered; }
        }

        public override void Press()
        {
            base.Press();

            m_Triggered = true;

            // Should this use events instead?
            if (firearm.animator != null && m_TriggerHoldHash != -1)
                firearm.animator.SetBool(m_TriggerHoldHash, true);

            // Set queue ticker
            if (m_DelayFirst)
                m_QueueTicker = m_LockSpacing;
            else
                m_QueueTicker = 0;
        }

        public override void Release()
        {
            base.Release();

            m_Triggered = false;

            // Should this use events instead?
            if (firearm.animator != null && m_TriggerHoldHash != -1)
                firearm.animator.SetBool(m_TriggerHoldHash, false);

            if (!m_Shooting)
            {
                // Shoot if shots are queued
                if (currentQueueCount > 0)
                {
                    m_Shooting = true;
                    m_Cooldown = 0;
                }
            }
        }

        public override void Cancel()
        {
            m_Triggered = false;
            if (!m_Shooting)
                ClearTargets();
        }

        private void OnDisable()
        {
            m_Triggered = false;
            ClearTargets();
        }

        protected override void OnSetBlocked(bool to)
        {
            base.OnSetBlocked(to);
            if (to)
            {
                if (!m_Shooting)
                    ClearTargets();
            }
        }

        protected override void FixedTriggerUpdate()
        {
            CheckTargets();

            if (m_Cooldown > 0)
                --m_Cooldown;
            else
            {
                if (m_Shooting)
                {
                    Shoot();
                    RemoveFirstTarget();
                    if (m_CurrentLocks.Count == 0)
                    {
                        m_Shooting = false;
                        m_Cooldown = m_RepeatDelay;
                    }
                    else
                    {
                        m_Cooldown = m_BurstSpacing;
                    }
                }
                else
                {
                    var reloader = firearm.reloader;
                    if (m_Triggered && reloader != null && reloader.empty)
                    {
                        Shoot(); // Effectively reload
                        m_Triggered = false;
                    }

                    int maxQueue = m_MaxQueueSize;

                    if (reloader != null && reloader.currentMagazine < maxQueue)
                        maxQueue = firearm.reloader.currentMagazine;

                    if (m_Triggered && !blocked && currentQueueCount < maxQueue)
                    {
                        if (m_QueueTicker <= 0)
                        {
                            var bestTarget = GetBestAvailableTarget();
                            if (bestTarget != null)
                            {
                                AddTarget(bestTarget);
                                m_QueueTicker = m_LockSpacing;
                            }
                            else
                                m_QueueTicker = m_LockRetrySpacing;
                        }
                        else
                            --m_QueueTicker;
                    }
                }
            }
        }

        public void RegisterTracker(ITargetTracker tracker)
        {
            if (m_CurrentLocks.Count > 0)
            {
                tracker.SetTargetCollider(m_CurrentLocks[0]);
            }
        }
    }
}