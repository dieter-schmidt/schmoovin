using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudinventoryitemmeter.html")]
    [RequireComponent(typeof(RectTransform))]
    public class HudInventoryItemMeter : HudInventoryItemTracker
    {
        [SerializeField, Tooltip("The rect transform of the filled bar.")]
        private RectTransform m_BarRect = null;
        [SerializeField, Tooltip("Should the meter still be shown if the inventory does not contain the item.")]
        private bool m_ShowIfZero = true;

        protected override void OnQuantityChanged()
        {
            if (m_BarRect == null)
                return;

            if (item != null)
            {
                float scale = Mathf.Clamp01((float)item.quantity / item.maxQuantity);
                m_BarRect.localScale = new Vector2(scale, 1f);
                gameObject.SetActive(true);
            }
            else
            {
                if (m_ShowIfZero)
                {
                    m_BarRect.localScale = Vector2.zero;
                    gameObject.SetActive(true);
                }
                else
                    gameObject.SetActive(false);
            }
        }
    }
}
