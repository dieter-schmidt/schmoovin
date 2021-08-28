using NeoFPS.CharacterMotion;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(BoxCollider))]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mb-basicwaterzone.html")]
    public class BasicWaterZone : MonoBehaviour, IWaterZone
    {
        [SerializeField, Tooltip("The key to the transform parameter on a character's motion graph to assign this waterzone to.")]
        private string m_ParameterKey = "waterZone";
        [SerializeField, Tooltip("The flow in meters per second of this water zone (local axes)")]
        private Vector3 m_Flow = Vector3.zero;

        private Transform m_Transform = null;
        private BoxCollider m_Collider = null;
        private List<Collider> m_Swimmers = new List<Collider>();
        private int m_ParameterHash = -1;

        void Awake()
        {
            m_Transform = transform;
            m_ParameterHash = Animator.StringToHash(m_ParameterKey);
            m_Collider = GetComponent<BoxCollider>();
            m_Collider.isTrigger = true;
        }

        public Vector3 FlowAtPosition(Vector3 position)
        {
            return m_Transform.rotation * m_Flow;
        }

        public WaterSurfaceInfo SurfaceInfoAtPosition(Vector3 position)
        {
            return new WaterSurfaceInfo(Vector3.up, m_Transform.position.y + m_Collider.center.y + m_Collider.size.y * 0.5f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_ParameterHash == -1)
                return;

            // Version 2 - Add bouyant objects and a central tracker for objects moving between zones

            // Get motion controller
            if (other.gameObject.layer == PhysicsFilter.LayerIndex.CharacterControllers)
            {
                var controller = other.GetComponent<MotionController>();
                if (controller != null)
                {
                    // Get water zone transform parameter for controller's graph
                    var prop = controller.motionGraph.GetTransformProperty(m_ParameterHash);
                    if (prop != null)
                    {
                        // Track collider
                        m_Swimmers.Add(other);

                        // Assign transform
                        prop.value = m_Transform;
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Get motion controller
            if (other.gameObject.layer == PhysicsFilter.LayerIndex.CharacterControllers)
            {
                int index = m_Swimmers.IndexOf(other);
                if (index != -1)
                {
                    // Remove reference
                    m_Swimmers.RemoveAt(index);

                    // Remove this from motion controller water zone parameter
                    var controller = other.GetComponent<MotionController>();
                    var prop = controller.motionGraph.GetTransformProperty(m_ParameterHash);
                    if (prop.value == m_Transform)
                        prop.value = null;
                }
            }
        }
    }
}