using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS.Samples.SinglePlayer
{
	[HelpURL("https://docs.neofps.com/manual/samplesref-mb-infopopuptrigger.html")]
	public class InfoPopupTrigger : MonoBehaviour
	{
		[SerializeField, Multiline, Tooltip("The info to display in the popup when triggeres.")]
		private string m_Info = "Enter text here";

        public void Show ()
		{
			InfoPopup.ShowPopup (m_Info, null);
		}
	}
}