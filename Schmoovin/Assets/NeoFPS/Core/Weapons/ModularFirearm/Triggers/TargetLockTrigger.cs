using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-targetlocktrigger.html")]
    public class TargetLockTrigger : BaseTriggerBehaviour, ITargetingSystem, ITargetLock
    {
        [Header("Target Lock")]

        [SerializeField, Tooltip("The object tag to home in on.")]
        private string m_DetectionTag = "AI";

        [SerializeField, Tooltip("The layers to check for colliders on.")]
        private LayerMask m_DetectionLayers = PhysicsFilter.Masks.Characters;

        [SerializeField, Tooltip("The time it takes to achieve a full lock and be able to fire.")]
        private float m_LockOnTime = 2f;

        [SerializeField, Tooltip("The angle of the cone within which objects can be detected.")]
        private float m_DetectionConeAngle = 10f;

        [SerializeField, Tooltip("The maximum distance that objects can be detected from the wielder.")]
        private float m_DetectionRange = 200f;

        [SerializeField, Tooltip("Does a target lock require an unobstructed line of sight to the target.")]
        private bool m_RequireLineOfSight = true;

        [SerializeField, Tooltip("The layers that can obstruct line of sight to the target.")]
        private LayerMask m_BlockingLayers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Tooltip("How long after the trigger fires does the trigger remember the target. This can be useful for shooters that shoot multiple projectiles.")]
        private float m_Memory = 0.1f;

        public event UnityAction<Collider, bool> onTargetLock;
        public event UnityAction<Collider> onTargetLockBroken;
        public event UnityAction<Collider, float> onTargetLockStrengthChanged;

        private static Collider[] s_OverlapColliders = new Collider[256];

        private bool m_Pressed = false;
        private bool m_LockedOn = false;
        private bool m_CheckTag = false;
        private bool m_Shoot = false;
        private Transform m_CameraTransform = null;
        private Collider m_CurrentTarget = null;
        private float m_LockStrength = 0f;
        private float m_EndTimer = 0f;

        private RaycastHit m_Hit = new RaycastHit();

        enum State
        {
            Idle,
            PendingLock,
            LockedOn
        }

        public Collider currentTarget
        {
            get { return m_CurrentTarget; }
            private set
            {
                if (m_CurrentTarget != value)
                {
                    if (m_CurrentTarget != null && onTargetLockBroken != null)
                        onTargetLockBroken(m_CurrentTarget);

                    m_CurrentTarget = value;

                    if (m_CurrentTarget != null)
                    {
                        if (onTargetLock != null)
                            onTargetLock(m_CurrentTarget, true);
                    }
                }
            }
        }

        public float lockStrength
        {
            get { return m_LockStrength; }
            set
            {
                value = Mathf.Clamp01(value);
                if (m_LockStrength != value)
                {
                    m_LockStrength = value;
                    if (onTargetLockStrengthChanged != null && currentTarget != null)
                        onTargetLockStrengthChanged(currentTarget, lockStrength);
                }
            }
        }

        public override bool cancelOnReload
        {
            get { return (m_CurrentTarget != null); }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            m_LockOnTime = Mathf.Clamp(m_LockOnTime, 0.5f, 100f);
            m_DetectionConeAngle = Mathf.Clamp(m_DetectionConeAngle, 0.01f, 90f);
            m_DetectionRange = Mathf.Clamp(m_DetectionRange, 5f, 500f);
        }
#endif

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
            //float closestSqrDistance = float.MaxValue;
            float smallestAngle = m_DetectionConeAngle * 0.5f;
            for (int i = 0; i < overlaps; ++i)
            {
                // Check tag
                if (m_CheckTag && !s_OverlapColliders[i].CompareTag(m_DetectionTag))
                    continue;

                // Check distance && angle
                var center = s_OverlapColliders[i].bounds.center;
                var offset = center - localPosition;
                var angle = Vector3.Angle(offset, camForward);
                //var sqrDistance = Vector3.SqrMagnitude(offset);
                if (angle < smallestAngle && angle < m_DetectionConeAngle * 0.5f)
                {
                    // Check line of sight
                    if (m_RequireLineOfSight && PhysicsExtensions.RaycastNonAllocSingle(
                        new Ray(localPosition, offset.normalized), out m_Hit, m_DetectionRange,
                        m_BlockingLayers, ignoreRoot) && !CheckIfHit(s_OverlapColliders[i]))
                    {
                        continue;
                    }

                    smallestAngle = angle;
                    target = s_OverlapColliders[i];
                }
                //if (sqrDistance < closestSqrDistance && angle < m_DetectionConeAngle * 0.5f)
                //{
                //    closestSqrDistance = sqrDistance;
                //    target = s_OverlapColliders[i];
                //}
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

        bool IsTargetInsideDetectionCone()
        {
            // Check distance and cone
            var localPosition = m_CameraTransform.position;
            var camForward = m_CameraTransform.forward;
            var center = currentTarget.bounds.center;
            var offset = center - localPosition;

            // Check if out of range (lose lock if so)
            if (Vector3.SqrMagnitude(offset) > (m_DetectionRange * m_DetectionRange))
            {
                currentTarget = null;
                return false;
            }

            // Check if within detection cone
            return (Vector3.Angle(offset, camForward) < m_DetectionConeAngle * 0.5f);
        }

        public override bool pressed
        {
            get { return m_Pressed; }
        }

        public override void Press()
        {
            base.Press();

            // Record state
            m_Pressed = true;

            // Get best target
            currentTarget = GetBestAvailableTarget();

            // Set starting state
            m_LockedOn = false;
            lockStrength = 0f;
        }

        public override void Release()
        {
            base.Release();

            if (m_Pressed)
            {
                // Record state
                m_Pressed = false;

                // Check if should shoot or cancel
                if (m_LockedOn)
                {
                    m_Shoot = true;
                    m_EndTimer = m_Memory;
                }
                else
                {
                    currentTarget = null;
                    lockStrength = 0f;
                }
            }
        }

        public override void Cancel()
        {
            m_Pressed = false;
            m_LockedOn = false;
            currentTarget = null;
        }

        private void OnDisable()
        {
            m_Pressed = false;
            m_LockedOn = false;
            currentTarget = null;
        }

        protected override void FixedTriggerUpdate()
        {
            if (m_Shoot)
            {
                Shoot();
                m_Shoot = false;
                m_LockedOn = false;
                currentTarget = null;
            }
            else
            {
                if (m_EndTimer > 0f)
                {
                    m_EndTimer -= Time.deltaTime;

                    //// Reset after short delay
                    //if (m_EndTimer <= 0f)
                    //{
                    //    m_EndTimer = 0f;
                    //    m_LockedOn = false;
                    //    currentTarget = null;
                    //}
                }
                else
                {
                    if (currentTarget != null && !m_LockedOn)
                    {
                        // Check if should continue or reset lock timer
                        if (!IsTargetInsideDetectionCone())
                        {
                            lockStrength = 0f;
                            if (onTargetLockStrengthChanged != null)
                                onTargetLockStrengthChanged(currentTarget, 0f);
                        }
                        else
                        {
                            lockStrength += Time.deltaTime / m_LockOnTime;
                            if (lockStrength == 1f)
                                m_LockedOn = true;
                        }
                    }
                }
            }
        }

        protected override void OnSetBlocked(bool to)
        {
            base.OnSetBlocked(to);
            if (to)
            {
                m_LockedOn = false;
                currentTarget = null;
                lockStrength = 0f;
            }
        }

        public void RegisterTracker(ITargetTracker tracker)
        {
            if (currentTarget != null && m_LockedOn)
            {
                tracker.SetTargetCollider(currentTarget);
                //currentTarget = null;
                //m_LockedOn = false;
            }
        }
    }
}