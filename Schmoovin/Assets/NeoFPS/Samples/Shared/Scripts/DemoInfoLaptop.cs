using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS.Samples.SinglePlayer
{
	[HelpURL("https://docs.neofps.com/manual/samplesref-mb-demoinfolaptop.html")]
	public class DemoInfoLaptop : MonoBehaviour
	{
        [SerializeField, Multiline, Tooltip("The title to display on the laptop screen.")]
        private string m_Title = "Information\nPoint";
        
		[SerializeField, Multiline, Tooltip("The info to display in the popup when interated with.")]
		private string m_Info = "Enter text here";

        private void Start()
        {
            Text t = GetComponentInChildren<Text>();
            t.text = m_Title;
        }

        public void Show ()
		{
			InfoPopup.ShowPopup (m_Info, null);
		}
	}
}