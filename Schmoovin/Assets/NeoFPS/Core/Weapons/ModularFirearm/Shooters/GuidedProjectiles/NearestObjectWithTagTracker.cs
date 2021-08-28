using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-nearestobjectwithtagtracker.html")]
    public class NearestObjectWithTagTracker : MonoBehaviour, IGuidedProjectileTargetTracker
    {
        [SerializeField, Tooltip("The object tag to home in on.")]
        private string m_DetectionTag = "AI";

        [SerializeField, Tooltip("The layers to check for colliders on.")]
        private LayerMask m_DetectionLayers = PhysicsFilter.Masks.Characters;

        [SerializeField, Tooltip("The max distance for targets to home in on.")]
        private float m_DetectionRange = 50f;

        [Range(0f, 5f), SerializeField, Tooltip("The time between searching for targets.")]
        private float m_DetectionCounter = 0.5f;

        private static Collider[] s_OverlapColliders = new Collider[32];

        private Transform m_LocalTransform = null;
        private Collider m_Target = null;
        private float m_DetectionTimer = 0f;
        private bool m_CheckTag = false;

        void OnValidate()
        {
            m_DetectionRange = Mathf.Clamp(m_DetectionRange, 1f, 500f);
            m_DetectionCounter = Mathf.Clamp(m_DetectionCounter, 0f, 10f);
        }

        void Awake()
        {
            m_LocalTransform = transform;
            m_CheckTag = !string.IsNullOrWhiteSpace(m_DetectionTag);
        }

        public bool GetTargetPosition(out Vector3 targetPosition)
        {
            // Detect objects in radius
            m_DetectionTimer -= Time.deltaTime;
            if (m_DetectionTimer <= 0f)
            {
                m_DetectionTimer = m_DetectionCounter;

                // Get overlapping targets
                Vector3 localPosition = m_LocalTransform.position;
                int overlaps = Physics.OverlapSphereNonAlloc(localPosition, m_DetectionRange, s_OverlapColliders, m_DetectionLayers);

                m_Target = null;

                // Find closest valid
                float closestSqrDistance = float.MaxValue;
                for (int i = 0; i < overlaps; ++i)
                {
                    // Check tag
                    if (m_CheckTag && !s_OverlapColliders[i].CompareTag(m_DetectionTag))
                        continue;

                    // Check distance
                    float sqrDistance = Vector3.SqrMagnitude(s_OverlapColliders[i].bounds.center - localPosition);
                    if (sqrDistance < closestSqrDistance)
                    {
                        closestSqrDistance = sqrDistance;
                        m_Target = s_OverlapColliders[i];
                    }
                }
            }

            // Send target center or fail
            if (m_Target != null)
            {
                targetPosition = m_Target.bounds.center;
                return true;
            }
            else
            {
                targetPosition = Vector3.zero;
                return false;
            }
        }

        void OnEnable()
        {
            m_DetectionTimer = 0f;
        }

        void OnDisable()
        {
            m_Target = null;
        }
    }
}