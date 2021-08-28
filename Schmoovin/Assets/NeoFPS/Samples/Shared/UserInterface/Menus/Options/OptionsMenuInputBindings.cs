using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoFPS.Constants;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace NeoFPS.Samples
{
	public class OptionsMenuInputBindings : OptionsMenuPanel
	{
		[SerializeField] private Transform m_ContainerTransform = null;
		[SerializeField] private MultiInputMultiChoice m_KeyboardLayoutMultiChoice = null;
		[SerializeField] private MultiInputButton m_ResetToDefaultsButton = null;
		[SerializeField] private MultiInputGroup m_PrototypeDivider = null;
        [SerializeField] private MultiInputKeyBinding m_PrototypeBinding = null;
		[SerializeField] private bool[] m_RebindableButtons = null;

#if UNITY_EDITOR
		[HideInInspector] public bool expandRebindableButtons = false;
#endif

		private List<MultiInputGroup> m_Dividers = null;
        private List<MultiInputKeyBinding> m_Entries = null;

        int ignoreButtonCount
        {
            get { return NeoFpsInputManager.fixedInputButtons.Length; }
        }

        private void OnValidate()
        {
			if (m_RebindableButtons == null || m_RebindableButtons.Length == 0)
				ResetRebindFlags();
			else
            {
				if (m_RebindableButtons.Length != FpsInputButton.count - ignoreButtonCount)
                {
					Debug.LogWarning("NeoFPS input button count has changed. Resetting rebindable filters on in-game options UI.");
					ResetRebindFlags();
                }
            }
        }

        public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);
			LoadOptions ();
		}

        public override void Show()
        {
            base.Show();
            FpsSettings.keyBindings.onRebind += OnRebindInput;
        }

        public override void Hide()
        {
            base.Hide();
            SaveOptions();
            FpsSettings.keyBindings.onRebind -= OnRebindInput;
        }

        protected void LoadOptions ()
		{
			// Hook in reset controls
			if (m_ResetToDefaultsButton != null)
				m_ResetToDefaultsButton.onClick.AddListener(OnResetToDefaultsButtonClicked);

			// Instantiate entries
			m_Entries = new List<MultiInputKeyBinding>();
				
			Dictionary<string, List<MultiInputKeyBinding>> sorted = new Dictionary<string, List<MultiInputKeyBinding>>();

			for (int i = ignoreButtonCount; i < FpsInputButton.count; ++i)
			{
				// Initialise the entry
				FpsInputButton button = i;
				if (!m_RebindableButtons[i - ignoreButtonCount])
					continue;

				var entry = m_Entries.Count == 0 ? m_PrototypeBinding : Instantiate(m_PrototypeBinding);
				entry.Initialise(
					button,
					NeoFpsInputManager.GetButtonDisplayName(button),
					FpsSettings.keyBindings.GetPrimaryKey(button),
					FpsSettings.keyBindings.GetSecondaryKey(button)
					);

				// Rename the entry gameobject
				entry.name = "UnityAction-" + FpsInputButton.names[i];

				// Add to entries list
				m_Entries.Add(entry);

				// Set the category
				string category = NeoFpsInputManager.GetButtonCategory(button).ToString();
				if (sorted.ContainsKey (category))
					sorted [category].Add (entry);
				else
				{
					List<MultiInputKeyBinding> list = new List<MultiInputKeyBinding> ();
					list.Add (entry);
					sorted.Add (category, list);
				}
			}

			// Set up heirarchy
			m_Dividers = new List<MultiInputGroup>(sorted.Count);
			m_Dividers.Add(m_PrototypeDivider);
			for (int i = 1; i < sorted.Count; ++i)
				m_Dividers.Add(Instantiate<MultiInputGroup> (m_PrototypeDivider));
			int itr = 0;
			foreach (string key in sorted.Keys)
			{
				m_Dividers [itr].label = (key == string.Empty) ? "Misc" : key;
				m_Dividers [itr].transform.SetParent (m_ContainerTransform);
				m_Dividers [itr].transform.localScale = Vector3.one;

				List<MultiInputKeyBinding> category = sorted [key];
				GameObject[] groupContents = new GameObject[category.Count];
				for (int i = 0; i < category.Count; ++i)
				{
					category [i].transform.SetParent (m_ContainerTransform);
					category [i].transform.localScale = Vector3.one;
					groupContents [i] = category [i].gameObject;
				}
				m_Dividers [itr].contents = groupContents;

				++itr;
			}
		}

        private void OnResetToDefaultsButtonClicked()
        {
			var layout = KeyboardLayout.Qwerty;
			if (m_KeyboardLayoutMultiChoice != null)
				layout = (KeyboardLayout)m_KeyboardLayoutMultiChoice.index;

			FpsSettings.keyBindings.ResetToDefault(layout);

			for (int i = ignoreButtonCount; i < FpsInputButton.count; ++i)
			{
				// Initialise the entry
				FpsInputButton button = (FpsInputButton)i;
				int index = i - ignoreButtonCount;
				m_Entries[index].Initialise(
					button,
					NeoFpsInputManager.GetButtonDisplayName(button),
					FpsSettings.keyBindings.GetPrimaryKey(button),
					FpsSettings.keyBindings.GetSecondaryKey(button)
					);
			}
		}

        protected override void SaveOptions ()
		{
			FpsSettings.keyBindings.Save ();
		}

		protected override void ResetOptions ()
		{
            FpsSettings.keyBindings.ResetToDefault();
		}

        void OnRebindInput(FpsInputButton button, bool isPrimary, KeyCode newKey)
        {
            if (isPrimary)
                m_Entries[button - ignoreButtonCount].primary = newKey;
            else
                m_Entries[button - ignoreButtonCount].secondary = newKey;
        }

		void ResetRebindFlags()
        {
			m_RebindableButtons = new bool[FpsInputButton.count - ignoreButtonCount];
			for (int i = 0; i < m_RebindableButtons.Length; ++i)
				m_RebindableButtons[i] = true;
        }
	}
}