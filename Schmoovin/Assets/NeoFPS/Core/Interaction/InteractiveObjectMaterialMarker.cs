using System.Collections;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-interactiveobjectcornermarkers.html")]
    [RequireComponent(typeof(InteractiveObject))]
    public class InteractiveObjectMaterialMarker : MonoBehaviour
    {
        [SerializeField, Tooltip("The mesh renderer of the object (used to set the relevant material property blocks).")]
        private MeshRenderer m_MeshRenderer = null;
        [SerializeField, Tooltip("The index of the highlight material on the mesh renderer.")]
        private int m_MaterialIndex = 0;
        [SerializeField, Tooltip("A fade time between he highlighted state and the non-highlighted state.")]
        private float m_TransitionDuration = 0.25f;

        MaterialPropertyBlock m_PropertyBlock = null;
        private int m_NameID = -1;
        private float m_Highlight = 0f;
        private bool m_Highlighted = false;
        private Coroutine m_HighlightLerpCoroutine = null;

        public bool highlighted
        {
            get { return m_Highlighted; }
            set
            {
                if (m_Highlighted != value)
                {
                    m_Highlighted = value;
                    // Stop the existing lerp coroutine
                    if (m_HighlightLerpCoroutine != null)
                        StopCoroutine(m_HighlightLerpCoroutine);
                    // Lerp to the desired value
                    if (m_TransitionDuration < 0.0001f)
                    {
                        if (m_Highlighted)
                            m_Highlight = 1f;
                        else
                            m_Highlight = 0f;
                        // Apply
                        SetHighlightLevel(m_Highlight);
                    }
                    else
                    {
                        if (m_Highlighted)
                            m_HighlightLerpCoroutine = StartCoroutine(LerpToOne());
                        else
                            m_HighlightLerpCoroutine = StartCoroutine(LerpToZero());
                    }
                }
            }
        }

        private void OnValidate()
        {
            if (m_MeshRenderer == null)
                m_MeshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        private void Awake()
        {
            m_PropertyBlock = new MaterialPropertyBlock();
            m_NameID = Shader.PropertyToID("_Highlight");
            SetHighlightLevel(0f);
        }

        private void Start()
        {
            var interactable = GetComponent<InteractiveObject>();
            interactable.onCursorEnter += Show;
            interactable.onCursorExit += Hide;
        }

        void SetHighlightLevel(float h)
        {
            h = Mathf.Clamp01(h);
            m_PropertyBlock.SetFloat(m_NameID, h);
            m_MeshRenderer.SetPropertyBlock(m_PropertyBlock, m_MaterialIndex);
        }

        public void Show()
        {
            if (!m_Highlighted)
            {
                m_Highlighted = true;
                // Stop the existing lerp coroutine
                if (m_HighlightLerpCoroutine != null)
                    StopCoroutine(m_HighlightLerpCoroutine);
                // Lerp to the desired value
                if (m_TransitionDuration < 0.0001f)
                {
                    m_Highlight = 1f;
                    SetHighlightLevel(m_Highlight);
                }
                else
                    m_HighlightLerpCoroutine = StartCoroutine(LerpToOne());
            }
        }

        public void Hide()
        {
            if (m_Highlighted)
            {
                m_Highlighted = false;
                // Stop the existing lerp coroutine
                if (m_HighlightLerpCoroutine != null)
                    StopCoroutine(m_HighlightLerpCoroutine);
                // Lerp to the desired value
                if (m_TransitionDuration < 0.0001f)
                {
                    m_Highlight = 0f;
                    SetHighlightLevel(m_Highlight);
                }
                else
                    m_HighlightLerpCoroutine = StartCoroutine(LerpToZero());
            }
        }

        IEnumerator LerpToOne()
        {
            while (m_Highlight < 1f)
            {
                yield return null;
                m_Highlight += Time.deltaTime / m_TransitionDuration;
                SetHighlightLevel(m_Highlight);
            }

            m_Highlight = 1f;
            m_HighlightLerpCoroutine = null;
        }

        IEnumerator LerpToZero()
        {
            while (m_Highlight > 0f)
            {
                yield return null;
                m_Highlight -= Time.deltaTime / m_TransitionDuration;
                SetHighlightLevel(m_Highlight);
            }

            m_Highlight = 0f;
            m_HighlightLerpCoroutine = null;
        }
    }
}