using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ShockwaveEffect : MonoBehaviour
    {
        [SerializeField, Tooltip("")]
        AnimationCurve m_AlphaCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 1f), new Keyframe(0.75f, 0.75f, 0f, 0f), new Keyframe(1f, 0f) });
        [SerializeField, Tooltip("")]
        private float m_Duration = 1f;
        [SerializeField, Tooltip("")]
        private float m_MaxScale = 5f;
        [SerializeField, Tooltip("")]
        private int m_MaterialIndex = 0;

        Transform m_LocalTransform = null;
        MeshRenderer m_Renderer = null;
        MaterialPropertyBlock m_PropertyBlock = null;
        private int m_NameID = -1;
        private float m_Progress = 0f;
        
        void Awake()
        {
            Initialise(true);
        }

        void SetProgress(float p)
        {
            p = Mathf.Clamp01(p);

            m_PropertyBlock.SetFloat(m_NameID, m_AlphaCurve.Evaluate(p));
            m_Renderer.SetPropertyBlock(m_PropertyBlock, m_MaterialIndex);

            float oneMinusP = 1f - p;
            float scale = (1f - (oneMinusP * oneMinusP)) * m_MaxScale;
            m_LocalTransform.localScale = new Vector3(scale, scale, scale);
        }

        void Initialise(bool setStart)
        {
            if (m_LocalTransform == null)
            {
                m_LocalTransform = transform;
                m_Renderer = GetComponent<MeshRenderer>();
                m_PropertyBlock = new MaterialPropertyBlock();
                m_NameID = Shader.PropertyToID("_Distortion");

                // Set the starting glow
                if (setStart)
                {
                    m_Progress = 0f;
                    SetProgress(0f);
                }
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
                Show();

            m_Progress += Time.deltaTime / m_Duration;
            if (m_Progress < 1f)
                SetProgress(m_Progress);
            else
                SetProgress(1f);
        }

        void Show()
        {
            m_Progress = 0f;
            SetProgress(0f);
        }
    }
}