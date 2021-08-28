using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
	public class OptionsMenuGameplay : OptionsMenuPanel
	{
		[SerializeField] private MultiInputMultiChoice m_CrosshairColourChoice = null;
		[SerializeField] private MultiInputToggle m_AutoReloadToggle = null;
		[SerializeField] private MultiInputSlider m_HeadBobSlider = null;

		public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);

			// Add listeners from code (saves user doing it as prefabs have a tendency to break)
			m_CrosshairColourChoice.onIndexChanged.AddListener (OnCrosshairColourChanged);
			if (m_AutoReloadToggle != null)
				m_AutoReloadToggle.onValueChanged.AddListener(OnAutoReloadChanged);
			if (m_HeadBobSlider != null)
				m_HeadBobSlider.onValueChanged.AddListener(OnHeadBobChanged);
		}

		void OnAutoReloadChanged(bool value)
        {
			FpsSettings.gameplay.autoReload = value;
        }

		public void OnHeadBobChanged(int value)
		{
			FpsSettings.gameplay.headBob = value * 0.01f;
		}

		protected override void SaveOptions ()
		{
			FpsSettings.gameplay.Save ();
		}

		protected override void ResetOptions ()
		{
			m_CrosshairColourChoice.index = ColourToIndex (FpsSettings.gameplay.crosshairColor);

			if (m_AutoReloadToggle != null)
				m_AutoReloadToggle.value = FpsSettings.gameplay.autoReload;

			if (m_HeadBobSlider != null)
			{
				// Setup head bob from settings
				int current = Mathf.RoundToInt(FpsSettings.gameplay.headBob * 100f);
				m_HeadBobSlider.value = current;
			}
		}

		public void OnCrosshairColourChanged (int index)
		{
            // White
            // Green
            // Red
            // Blue
            // Cyan
            // Magenta
            
            // Store setting
            switch (index)
			{
				case 0:
                    FpsSettings.gameplay.crosshairColor = new Color(1f, 1f, 1f, 0.5f);// Color.white;
					break;
				case 1:
					FpsSettings.gameplay.crosshairColor = new Color(0f, 1f, 0f, 0.5f);//Color.green;
                    break;
				case 2:
					FpsSettings.gameplay.crosshairColor = new Color(1f, 0f, 0f, 0.5f);//Color.red;
                    break;
				case 3:
					FpsSettings.gameplay.crosshairColor = new Color(0f, 0f, 1f, 0.5f);//Color.blue;
                    break;
				case 4:
					FpsSettings.gameplay.crosshairColor = new Color(0f, 1f, 1f, 0.5f);//Color.cyan;
                    break;
				case 5:
					FpsSettings.gameplay.crosshairColor = new Color(1f, 0f, 1f, 0.5f);//Color.magenta;
                    break;
				case 6:
					FpsSettings.gameplay.crosshairColor = new Color(1f, 1f, 0f, 0.5f);//Color.yellow;
                    break;
			}
		}

		int ColourToIndex (Color c)
		{
			if (c.a != 1f)
				return 0; // Not preset - default to 0

			// 3 components
			if (c.r == 1f && c.g == 1f && c.b == 1f)
				return 0; // White

			// 2 components
			if (c.r == 1f && c.b == 1f)
				return 5; // Magenta
			if (c.g == 1f && c.b == 1f)
				return 4; // Cyan
			if (c.r == 1f && c.g == 1f)
				return 6; // Yellow

			// 1 component
			if (c.r == 1f)
				return 2; // Red
			if (c.g == 1f)
				return 1; // Green
			if (c.b == 1f)
				return 3; // Blue

			// Not preset - default to 0
			return 0;
		}
	}
}