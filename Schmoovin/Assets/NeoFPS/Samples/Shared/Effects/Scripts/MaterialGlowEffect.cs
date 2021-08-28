using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(MeshRenderer))]
    public class MaterialGlowEffect : MonoBehaviour
    {
        [SerializeField, Tooltip("")]
        private int m_MaterialIndex = 0;
        [SerializeField, Tooltip("")]
        private float m_StartingGlow = 0f;

        MeshRenderer m_Renderer = null;
        MaterialPropertyBlock m_PropertyBlock = null;
        private int m_NameID = -1;
        private float m_Glow = 0f;

        private void Awake()
        {
            Initialise(true);
        }

        public float glow
        {
            get { return m_Glow; }
            set
            {
                m_Glow = Mathf.Clamp01(value);
                m_PropertyBlock.SetFloat(m_NameID, m_Glow);
                m_Renderer.SetPropertyBlock(m_PropertyBlock, m_MaterialIndex);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
                glow += 0.05f;
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
                glow -= 0.05f;
        }

        void Initialise(bool setStart)
        {
            if (m_Renderer == null)
            {
                m_Renderer = GetComponent<MeshRenderer>();
                m_PropertyBlock = new MaterialPropertyBlock();
                m_NameID = Shader.PropertyToID("_Glow");

                // Set the starting glow
                if (setStart)
                    glow = m_StartingGlow;
            }
        }
    }
}