using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(PooledObject))]
    public class LineRendererHitscanTrail : MonoBehaviour, IPooledHitscanTrail
    {
        [SerializeField, Tooltip("Randomise the texture's U mapping 0-1. Requires the material to have an \"_OffsetU\" property accessible via property block")]
        private bool m_RandomUOffset = false;

        [SerializeField, Tooltip("The maximum length of the trail.")]
        private float m_MaxLength = 100f;

        private PooledObject m_PooledObject = null;
        private LineRenderer m_LineRenderer = null;
        private float m_Duration = 0f;
        private float m_Timer = 0f;
        private Color m_StartColour = Color.white;
        private Color m_EndColour = Color.white;

        void Awake()
        {
            m_PooledObject = GetComponent<PooledObject>();
            m_LineRenderer = GetComponent<LineRenderer>();
            m_LineRenderer.positionCount = 2;
            m_LineRenderer.enabled = false;
            m_StartColour = m_LineRenderer.startColor;
            m_EndColour = m_LineRenderer.endColor;

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
                float alpha = Mathf.Clamp01(1f - (m_Timer / m_Duration));

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
            m_Duration = duration;
            m_LineRenderer.SetPosition(0, start);

            // Get the end of the trail
            end -= start;
            end = Vector3.ClampMagnitude(end, m_MaxLength);
            m_LineRenderer.SetPosition(1, start + end);

            m_LineRenderer.widthMultiplier = size;
            m_LineRenderer.startColor = Color.white;
            m_LineRenderer.endColor = Color.white;
            m_LineRenderer.enabled = true;
        }
    }
}