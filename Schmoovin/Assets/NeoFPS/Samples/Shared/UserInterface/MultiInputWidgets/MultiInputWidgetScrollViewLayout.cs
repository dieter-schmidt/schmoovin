using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    public class MultiInputWidgetScrollViewLayout : MultiInputWidgetLayout
	{
		ScrollRect m_ScrollRect = null;

        private Vector3[] m_Corners = new Vector3[4];

		protected override void Awake ()
		{
			base.Awake ();
			m_ScrollRect = GetComponent<ScrollRect> ();
		}

		public override void OnWidgetSelected (MultiInputWidget widget)
		{
			base.OnWidgetSelected (widget);

			// Check if content is large enough to require scrolling
			RectTransform rt = transform as RectTransform;
			RectTransform ct = m_ScrollRect.content;
			float contentHeight = ct.rect.height;
			if (contentHeight > rt.rect.height)
			{
				// Check if widget is within visible scroll area
				RectTransform wt = (RectTransform)widget.transform;

				rt.GetWorldCorners (m_Corners);

				float worldBottom = m_Corners [0].y;
				float worldTop = m_Corners [1].y;
				float worldHeight = worldTop - worldBottom;

				wt.GetWorldCorners (m_Corners);

				contentHeight *= ct.lossyScale.y;
				float multiplier = 1f / (contentHeight - worldHeight);

				// Check if off bottom of scroll rect
				if (m_Corners [0].y < worldBottom)
				{
					float diff = worldBottom - m_Corners [0].y;
					m_ScrollRect.verticalNormalizedPosition -= diff * multiplier;
				}
				else
				{
					// Check if off top of scroll rect
					if (m_Corners [1].y > worldTop)
					{
						float diff = m_Corners [1].y - worldTop;
						m_ScrollRect.verticalNormalizedPosition += diff * multiplier;
					}
				}
			}
		}
	}
}