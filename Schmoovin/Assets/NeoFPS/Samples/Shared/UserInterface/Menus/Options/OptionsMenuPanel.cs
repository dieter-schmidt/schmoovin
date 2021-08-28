using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NeoFPS.Samples
{
	public abstract class OptionsMenuPanel : MenuPanel
	{
		public override void Show ()
		{
			base.Show ();
			ResetOptions ();
		}

		public override void Hide ()
		{
			SaveOptions ();
			base.Hide ();
		}

		public void Save ()
		{
			SaveOptions ();
		}

		protected abstract void SaveOptions ();
		protected abstract void ResetOptions ();
	}
}