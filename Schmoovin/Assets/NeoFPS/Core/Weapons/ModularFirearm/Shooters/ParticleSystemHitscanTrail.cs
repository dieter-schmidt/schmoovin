using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(PooledObject))]
    public class ParticleSystemHitscanTrail : MonoBehaviour, IPooledHitscanTrail
    {
        [SerializeField, Tooltip("The number of particles per meter of trail. Used for the emit count to ensure a predictable density.")]
        private float m_ParticlesPerMeter = 10f;

        private Transform m_LocalTransform = null;
        private PooledObject m_PooledObject = null;
        private ParticleSystem m_ParticleSystem = null;
        private float m_Duration = 0f;
        private float m_Timer = 0f;

        void Awake()
        {
            m_LocalTransform = transform;
            m_PooledObject = GetComponent<PooledObject>();
            m_ParticleSystem = GetComponent<ParticleSystem>();

            var shapeModule = m_ParticleSystem.shape;
            shapeModule.rotation = new Vector3(0f, 90f, 0f);
            m_Duration = m_ParticleSystem.main.duration;
        }

        void Update()
        {
            if (m_Timer >= m_Duration)
                m_PooledObject.ReturnToPool();
            else
                m_Timer += Time.deltaTime;
        }

        public void Show(Vector3 start, Vector3 end, float size, float duration)
        {
            m_Timer = 0f;
            //m_Duration = duration;

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
        }
    }
}