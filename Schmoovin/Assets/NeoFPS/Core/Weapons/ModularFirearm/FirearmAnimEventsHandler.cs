using System;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-firearmanimeventshandler.html")]
	public class FirearmAnimEventsHandler : MonoBehaviour
	{
		private ModularFirearm m_Firearm = null;

        void Awake ()
		{
			m_Firearm = GetComponentInParent<ModularFirearm> ();
			if (m_Firearm == null)
				Debug.LogError ("FirearmAnimEventsHandler requires a ModularFirearm component on this or a parent object.", gameObject);
		}

		public void WeaponRaised ()
		{
			if (m_Firearm != null)
				m_Firearm.ManualWeaponRaised ();
		}
		public void FirearmReloadPartial ()
		{
			if (m_Firearm != null && m_Firearm.reloader != null)
				m_Firearm.reloader.ManualReloadPartial ();
		}
		public void FirearmReloadComplete ()
		{
			if (m_Firearm != null && m_Firearm.reloader != null)
				m_Firearm.reloader.ManualReloadComplete ();
		}
		public void FirearmEjectShell ()
		{
			if (m_Firearm != null && m_Firearm.ejector != null)
				m_Firearm.ejector.Eject ();
		}
	}
}

