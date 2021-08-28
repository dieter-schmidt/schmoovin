using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudshieldmeterstep.html")]
    [RequireComponent(typeof(RectTransform))]
    public class HudShieldMeterStep : MonoBehaviour
    {
        [SerializeField, Tooltip("The spacing between steps")]
        private int m_Spacing = 2;

        private RectTransform m_RectTransform = null;
        private Vector2 m_Dimensions = Vector2.zero;

        private float m_Fill = 1f;
        public float fill
        {
            get { return m_Fill; }
            set
            {
                value = Mathf.Clamp01(value);
                if (m_Fill != value)
                {
                    if (m_Fill == 0f)
                        gameObject.SetActive(true);

                    m_Fill = value;

                    if (m_Fill == 0f)
                        gameObject.SetActive(false);
                    else
                        OnFillChanged();
                }                    
            }
        }

        public void ResetLayout(int index, int count)
        {
            // Get the rect layout
            if (m_RectTransform == null)
                m_RectTransform = transform as RectTransform;

            // Get the full size of the parent
            var full = ((RectTransform)transform.parent).rect;

            // Get the size of each bar
            float barWidth = (full.width - (count - 1) * m_Spacing) / count;

            // Get the anchored position
            m_RectTransform.anchoredPosition = new Vector2(index * barWidth + index * m_Spacing, 0f);

            // Get (and apply) the dimensions
            m_Dimensions = new Vector2(barWidth, full.height);
            m_RectTransform.sizeDelta = new Vector2(m_Dimensions.x * m_Fill, m_Dimensions.y);
        }
        

        protected virtual void OnFillChanged()
        {
            m_RectTransform.sizeDelta = new Vector2(m_Dimensions.x * m_Fill, m_Dimensions.y);
        }
    }
}