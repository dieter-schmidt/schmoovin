using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS.Samples.SinglePlayer
{
	[RequireComponent (typeof (Text))]
	[HelpURL("https://docs.neofps.com/manual/samplesref-mb-firingrangereadout.html")]
	public class FiringRangeReadout : MonoBehaviour
	{
		const string k_ReadoutString = "Hits: {0}\nMisses: {1}";

		private Text m_ReadoutText = null;
        private int m_Hits = 0;
        private int m_Misses = 0;

		void Awake ()
		{
			m_ReadoutText = GetComponent<Text> ();
            m_ReadoutText.text = string.Format(k_ReadoutString, 0, 0);
        }

		public void OnHitsChanged (int total)
		{
			m_Hits = total;
			m_ReadoutText.text = string.Format (k_ReadoutString, m_Hits, m_Misses);
		}

		public void OnMissesChanged (int total)
		{
			m_Misses = total;
			m_ReadoutText.text = string.Format (k_ReadoutString, m_Hits, m_Misses);
		}
	}
}