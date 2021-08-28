using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudadvancedcrosshairstylestandard.html")]
    public class HudAdvancedCrosshairStyleStandard : HudAdvancedCrosshairStyleBase
    {
        [Header("Crosshair")]

        [SerializeField, Tooltip("The parent rect transform of the crosshair.")]
        private RectTransform m_CrosshairRect = null;

        [SerializeField, Tooltip("The size the UI element will reach at 100% accuracy.")]
        private float m_MaxAccuracySize = 64f;

        [SerializeField, Tooltip("The size the UI element will reach at 0% accuracy.")]
        private float m_MinAccuracySize = 256f;

        [Header("Hit Marker")]

        [SerializeField, Tooltip("Should the hit marker only show critical hits, or any hit that dealt damage.")]
        private bool m_OnlyShowCriticals = false;

        [SerializeField, Tooltip("The colour of the hit markers if for critical hits. Non-critical will use the crosshair colour.")]
        private Color m_CritColour = Color.red;

        [SerializeField, Tooltip("The amount of time the hit marker will be visible.")]
        private float m_HitmarkerDuration = 0.25f;

        [SerializeField, Tooltip("The animation easing function of the hit marker size.")]
        private HitMarkerAnimation m_Animation = HitMarkerAnimation.Bounce;

        [SerializeField, Tooltip("The parent rect transform of the hit marker.")]
        private RectTransform m_HitMarkerRect = null;

        [SerializeField, Tooltip("The starting size of the hit marker.")]
        private float m_HitStartSize = 48f;

        [SerializeField, Tooltip("The size of the hit marker just before it vanishes.")]
        private float m_HitEndSize = 64f;

        private List<Graphic> m_HitMarkerGraphics = new List<Graphic>();
        private Color m_BaseColour = Color.white;
        private float m_HitMarkerLerp = 0f;
        private float m_HitMarkerTimeScale = 1f;
        private bool m_WasCrit = false;
        
        public enum HitMarkerAnimation
        {
            Lerp,
            EaseIn,
            EaseOut,
            EaseInOut,
            Spring,
            Bounce
        }

        protected float hitMarkerLerp
        {
            get { return m_HitMarkerLerp; }
            set
            {
                if (m_HitMarkerLerp != value)
                {
                    m_HitMarkerLerp = Mathf.Clamp01(value);
                    if (m_HitMarkerLerp == 0f)
                            m_HitMarkerRect.gameObject.SetActive(true);
                    else
                    {
                        if (m_HitMarkerLerp == 1f)
                            m_HitMarkerRect.gameObject.SetActive(false);
                        else
                        {
                            switch (m_Animation)
                            {
                                case HitMarkerAnimation.Lerp:
                                    SetHitMarkerSizeNormalized(m_HitMarkerLerp);
                                    break;
                                case HitMarkerAnimation.EaseIn:
                                    SetHitMarkerSizeNormalized(EasingFunctions.EaseInQuadratic(m_HitMarkerLerp));
                                    break;
                                case HitMarkerAnimation.EaseOut:
                                    SetHitMarkerSizeNormalized(EasingFunctions.EaseOutQuadratic(m_HitMarkerLerp));
                                    break;
                                case HitMarkerAnimation.EaseInOut:
                                    SetHitMarkerSizeNormalized(EasingFunctions.EaseInOutQuadratic(m_HitMarkerLerp));
                                    break;
                                case HitMarkerAnimation.Spring:
                                    SetHitMarkerSizeNormalized(EasingFunctions.EaseInSpring(m_HitMarkerLerp));
                                    break;
                                case HitMarkerAnimation.Bounce:
                                    SetHitMarkerSizeNormalized(EasingFunctions.EaseInBounce(m_HitMarkerLerp));
                                    break;
                            }
                        }
                    }
                }
            }
        }

        void OnValidate()
        {
            m_HitmarkerDuration = Mathf.Clamp(m_HitmarkerDuration, 0.01f, 2f);
        }

        void Awake()
        {
            // Get the hit marker graphics
            m_HitMarkerRect.GetComponentsInChildren(true, m_HitMarkerGraphics);
            // Get hit marker time scale
            m_HitMarkerTimeScale = 1f / m_HitmarkerDuration;
            hitMarkerLerp = 1f;
        }

        private void OnDisable()
        {
            hitMarkerLerp = 1f;
        }

        void Update()
        {
            if (hitMarkerLerp < 1f)
                hitMarkerLerp += Time.deltaTime * m_HitMarkerTimeScale;
        }

        public override void SetAccuracy(float accuracy)
        {
            if (m_CrosshairRect != null)
            {
                float size = Mathf.Lerp(m_MinAccuracySize, m_MaxAccuracySize, Mathf.Clamp01(accuracy));
                m_CrosshairRect.sizeDelta = new Vector2(size, size);
            }
        }

        public override void SetColour(Color c)
        {
            m_BaseColour = c;

            Graphic[] graphics = GetComponentsInChildren<Graphic>();
            for (int i = 0; i < graphics.Length; ++i)
                graphics[i].color = c;
        }

        public override void ShowHitMarker(bool critical)
        {
            if (m_OnlyShowCriticals && !critical)
                return;

            // Set colour based on critical or not
            if (!m_OnlyShowCriticals)
            {
                if (critical)
                {
                    if (!m_WasCrit)
                    {
                        for (int i = 0; i < m_HitMarkerGraphics.Count; ++i)
                            m_HitMarkerGraphics[i].color = m_CritColour;
                    }
                }
                else
                {
                    if (m_WasCrit)
                    {
                        for (int i = 0; i < m_HitMarkerGraphics.Count; ++i)
                            m_HitMarkerGraphics[i].color = m_BaseColour;
                    }
                }
            }

            // Show the hit marker
            hitMarkerLerp = 0f;

            // Record if critical
            m_WasCrit = critical;
        }

        void SetHitMarkerSizeNormalized(float size)
        {
            float scaled = m_HitStartSize + (m_HitEndSize - m_HitStartSize) * size;
            m_HitMarkerRect.sizeDelta = new Vector2(scaled, scaled);
        }
    }
}