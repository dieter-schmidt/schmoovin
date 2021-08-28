using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudadvancedcrosshair.html")]
    [RequireComponent(typeof(RectTransform))]
    public class HudInventoryItemCounter : HudInventoryItemTracker
    {
        [SerializeField, Tooltip("The UI text element to output the quantity to")]
        private Text m_CounterText = null;

        [SerializeField, Tooltip("How to format the quantity")]
        private Format m_Format = Format.QuantityOfMaximum;

        public enum Format
        {
            Quantity,
            QuantityAlwaysVisible,
            QuantityOfMaximum,
            Percent
        }
        
        protected override void OnQuantityChanged()
        {
            if (m_CounterText == null)
                return;

            if (item != null)
            {
                switch(m_Format)
                {
                    case Format.Quantity:
                        m_CounterText.text = item.quantity.ToString();
                        break;
                    case Format.QuantityAlwaysVisible:
                        m_CounterText.text = item.quantity.ToString();
                        break;
                    case Format.QuantityOfMaximum:
                        m_CounterText.text = string.Format("{0} / {1}", item.quantity, item.maxQuantity);
                        break;
                    case Format.Percent:
                        {
                            float percentage = (item.quantity * 100) / item.maxQuantity;
                            m_CounterText.text = string.Format("{0}%", percentage);
                        }
                        break;
                }
                gameObject.SetActive(true);
            }
            else
            {
                if (m_Format == Format.QuantityAlwaysVisible)
                {
                    m_CounterText.text = "0";
                    gameObject.SetActive(true);
                }
                else
                    gameObject.SetActive(false);
            }
        }
    }
}
