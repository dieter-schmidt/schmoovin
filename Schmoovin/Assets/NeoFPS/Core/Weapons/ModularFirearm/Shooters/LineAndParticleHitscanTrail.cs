using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(PooledObject))]
    public class LineAndParticleHitscanTrail : MonoBehaviour, IPooledHitscanTrail
    {
        [SerializeField, Tooltip("The number of particles per meter of trail. Used for the emit count to ensure a predictable density.")]
        private float m_ParticlesPerMeter = 10f;
        [SerializeField, Tooltip("Randomise the line texture's U mapping 0-1. Requires the material to have an \"_OffsetU\" property accessible via property block")]
        private bool m_RandomUOffset = false;

        private Transform m_LocalTransform = null;
        private PooledObject m_PooledObject = null;
        private LineRenderer m_LineRenderer = null;
        private ParticleSystem m_ParticleSystem = null;
        private float m_TotalDuration = 0f;
        private float m_LineDuration = 0f;
        private float m_Timer = 0f;
        private Color m_StartColour = Color.white;
        private Color m_EndColour = Color.white;

        void Awake()
        {
            m_LocalTransform = transform;
            m_PooledObject = GetComponent<PooledObject>();

            m_LineRenderer = GetComponent<LineRenderer>();
            m_LineRenderer.positionCount = 2;
            m_LineRenderer.enabled = false;
            m_StartColour = m_LineRenderer.startColor;
            m_EndColour = m_LineRenderer.endColor;

            m_ParticleSystem = GetComponent<ParticleSystem>();
            var shapeModule = m_ParticleSystem.shape;
            shapeModule.rotation = new Vector3(0f, 90f, 0f);
            m_TotalDuration = m_ParticleSystem.main.duration;

            if (m_RandomUOffset)
            {
                var b = new MaterialPropertyBlock();
                b.SetFloat("_OffsetU", Random.Range(0, 12f));
                m_LineRenderer.SetPropertyBlock(b);
            }
        }

        void Update()
        {
            // Disable Line Renderer
            if (m_Timer >= m_LineDuration && m_LineRenderer.enabled)
                m_LineRenderer.enabled = false;

            if (m_Timer >= m_TotalDuration)
                m_PooledObject.ReturnToPool();
            {
                float alpha = Mathf.Clamp01(1f - (m_Timer / m_LineDuration));

                Color c = m_StartColour;
                c.a *= alpha;
                m_LineRenderer.startColor = c;

                c = m_EndColour;
                c.a *= alpha;
                m_LineRenderer.endColor = c;

                m_Timer += Time.deltaTime;
            }
        }

        public void Show(Vector3 start, Vector3 end, float size, float duration)
        {
            m_Timer = 0f;
            m_LineDuration = duration;

            // Get the required length
            float length = (end - start).magnitude;

            // Position and rotate the object
            m_LocalTransform.position = (start + end) * 0.5f;
            m_LocalTransform.localRotation = Quaternion.FromToRotation(Vector3.forward, end - start);

            // Set the particle system shape length
            var shape = m_ParticleSystem.shape;
            shape.radius = length * 0.5f;

            // Emit based on particles per meter
            m_ParticleSystem.Emit((int)(length * m_ParticlesPerMeter));

            // Setup line renderer
            m_LineRenderer.SetPosition(0, start);
            m_LineRenderer.SetPosition(1, end);
            m_LineRenderer.widthMultiplier = size;
            m_LineRenderer.startColor = Color.white;
            m_LineRenderer.endColor = Color.white;
            m_LineRenderer.enabled = true;
        }
    }
}