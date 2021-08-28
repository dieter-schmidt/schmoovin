using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(PooledObject))]
    public class NoisyLineHitscanTrail : MonoBehaviour, IPooledHitscanTrail
    {
        [SerializeField, Tooltip("The number of particles per meter of trail. Used for the emit count to ensure a predictable density.")]
        private float m_PointsPerMeter = 4f;
        [SerializeField, Tooltip("The maximum number of points for the line.")]
        private int m_MaxPoints = 500;
        [SerializeField, Tooltip("The line width once the trail dies.")]
        private float m_EndSizeMultiplier = 3f;
        [SerializeField, Tooltip("Randomise the texture's U mapping 0-1. Requires the material to have an \"_OffsetU\" property accessible via property block")]
        private bool m_RandomUOffset = false;

        private const int k_MaxPoints = 2048;

        private static ParticleSystem.Particle[] s_Particles = new ParticleSystem.Particle[k_MaxPoints];

        private Transform m_LocalTransform = null;
        private PooledObject m_PooledObject = null;
        private LineRenderer m_LineRenderer = null;
        private ParticleSystem m_ParticleSystem = null;
        private int m_PointCount = 0;
        private float m_Duration = 0f;
        private float m_Timer = 0f;
        private float m_WidthMultiplier = 1f;
        private Color m_StartColour = Color.white;
        private Color m_EndColour = Color.white;

        void OnValidate()
        {
            m_MaxPoints = Mathf.Clamp(m_MaxPoints, 10, k_MaxPoints);
        }

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
            shapeModule.rotation = new Vector3(0f, -90f, 0f);
            m_Duration = m_ParticleSystem.main.duration;

            if (m_RandomUOffset)
            {
                var b = new MaterialPropertyBlock();
                b.SetFloat("_OffsetU", Random.Range(0, 12f));
                m_LineRenderer.SetPropertyBlock(b);
            }
        }

        void Update()
        {
            if (m_Timer >= m_Duration)
            {
                m_LineRenderer.enabled = false;
                m_PooledObject.ReturnToPool();
            }
            else
            {
                float normalised = Mathf.Clamp01(m_Timer / m_Duration);

                // Set the line colour
                float alpha = 1f - normalised;
                Color c = m_StartColour;
                c.a *= alpha;
                m_LineRenderer.startColor = c;

                c = m_EndColour;
                c.a *= alpha;
                m_LineRenderer.endColor = c;

                // Set the line thickness
                m_LineRenderer.widthMultiplier = Mathf.Lerp(m_WidthMultiplier, m_WidthMultiplier * m_EndSizeMultiplier, normalised);

                // Set the line points
                UpdateLinePoints();

                // Increment the timer
                m_Timer += Time.deltaTime;
            }
        }

        public void Show(Vector3 start, Vector3 end, float size, float duration)
        {
            m_Timer = 0f;
            m_Duration = duration;
            
            // Get the required length
            float length = (end - start).magnitude;

            // Position and rotate the object
            m_LocalTransform.position = (start + end) * 0.5f;
            m_LocalTransform.localRotation = Quaternion.FromToRotation(Vector3.forward, end - start);

            // Set the particle lifetime
            var main = m_ParticleSystem.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(duration);

            // Set the particle system shape length
            var shape = m_ParticleSystem.shape;
            shape.radius = length * 0.5f;

            // Emit based on particles per meter
            int count = Mathf.Min((int)(length * m_PointsPerMeter), m_MaxPoints);
            m_ParticleSystem.Emit(count);

            // Setup line renderer
            m_PointCount = count;
            m_WidthMultiplier = size;
            m_LineRenderer.positionCount = count;
            m_LineRenderer.widthMultiplier = size;
            m_LineRenderer.startColor = Color.white;
            m_LineRenderer.endColor = Color.white;
            UpdateLinePoints();
            m_LineRenderer.enabled = true;
        }

        void UpdateLinePoints()
        {
            int numPoints = m_ParticleSystem.GetParticles(s_Particles, m_PointCount);

            // Check line renderer length is the same
            if (m_LineRenderer.positionCount != numPoints)
                m_LineRenderer.positionCount = numPoints;

            for (int i = 0; i < numPoints; ++i)
                m_LineRenderer.SetPosition(i, s_Particles[i].position);

            //m_LineRenderer.SetPosition(0, start);
            //m_LineRenderer.SetPosition(1, end);
        }
    }
}