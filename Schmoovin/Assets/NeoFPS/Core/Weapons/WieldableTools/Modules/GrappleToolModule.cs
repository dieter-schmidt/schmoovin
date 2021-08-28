using NeoFPS.CharacterMotion.Parameters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.WieldableTools
{
    [RequireComponent (typeof (LineRenderer))]
    public class GrappleToolModule : BaseWieldableToolModule
    {
        [SerializeField, Tooltip("The name of the vector parameter on the character's motion graph that you want to set the grapple point to.")]
        private string m_GrapplePointKey = "grapplePoint";
        [SerializeField, Tooltip("The physics layers the tool can grapple onto.")]
        private LayerMask m_GrappleLayers = PhysicsFilter.Masks.BulletBlockers;
        [SerializeField, Tooltip("The maximum distance that a grapple can connect.")]
        private float m_MaxGrappleDistance = 50f;
        [SerializeField, Tooltip("The transform to use as the start point of the tether line renderer.")]
        private Transform m_TetherStart = null;

        private LineRenderer m_LineRenderer = null;
        private VectorParameter m_GrapplePointParameter = null;
        private bool m_IsGrappling = false;

        public override WieldableToolActionTiming timing
        {
            get { return k_TimingsAll; }
        }

        public override bool isValid
        {
            get { return m_GrappleLayers != 0; }
        }

        void OnValidate()
        {
            m_MaxGrappleDistance = Mathf.Clamp(m_MaxGrappleDistance, 1f, 1000f);
            if (m_TetherStart == null)
                m_TetherStart = transform;
        }

        public override void Initialise(IWieldableTool t)
        {
            base.Initialise(t);

            if (t.wielder != null)
                m_GrapplePointParameter = t.wielder.motionController.motionGraph.GetVectorProperty(m_GrapplePointKey);
            if (m_GrapplePointParameter == null)
                Debug.LogError("Failed to get grapple point parameter from the wielder's motion graph.");

            m_LineRenderer = GetComponent<LineRenderer>();
            m_LineRenderer.useWorldSpace = true;
            m_LineRenderer.positionCount = 2;
            m_LineRenderer.enabled = false;
        }

        public override void FireStart()
        {
            if (m_GrapplePointParameter != null)
            {
                // Check for hit
                RaycastHit hit;
                if (PhysicsExtensions.RaycastNonAllocSingle(tool.wielder.fpCamera.GetAimRay(), out hit, m_MaxGrappleDistance, m_GrappleLayers, tool.wielder.transform, QueryTriggerInteraction.Ignore))
                {
                    m_GrapplePointParameter.value = hit.point;
                    m_IsGrappling = true;
                    m_LineRenderer.enabled = true;
                    m_LineRenderer.SetPosition(1, hit.point);
                }
                else
                    tool.Interrupt();
            }
        }

        public override void FireEnd(bool success)
        {
            // Disconnect line & set to zero
            if (m_GrapplePointParameter != null && m_GrapplePointParameter.value != Vector3.zero)
                m_GrapplePointParameter.value = Vector3.zero;
            m_IsGrappling = false;
            m_LineRenderer.enabled = false;
        }

        public override bool TickContinuous()
        {
            // Check if zero and interrupt
            if (m_IsGrappling)
            {
                // Interrupt if grapple point has been reset
                if (m_GrapplePointParameter.value == Vector3.zero)
                {
                    tool.Interrupt();
                    m_IsGrappling = false;
                    m_LineRenderer.enabled = false;
                }
            }

            return true;
        }

        void LateUpdate()
        {
            if (m_IsGrappling)
            {
                // Rotate and draw line
                m_LineRenderer.SetPosition(0, m_TetherStart.position);
                //m_LineRenderer.SetPosition(1, m_GrapplePointParameter.value);
            }
        }
    }
}