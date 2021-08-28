using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS.Samples.SinglePlayer
{
	[RequireComponent (typeof (Canvas))]
	[HelpURL("https://docs.neofps.com/manual/samplesref-mb-doorsdemoelevatorreadout.html")]
    public class DoorsDemoElevatorReadout : MonoBehaviour
    {
        private Text m_Readout = null;

        void Awake()
        {
            m_Readout = GetComponentInChildren<Text>();
        }

        public void OnFloorChange(int floorIndex)
        {
			if (m_Readout != null)
                m_Readout.text = "Floor: " + (floorIndex + 1);
        }
    }
}