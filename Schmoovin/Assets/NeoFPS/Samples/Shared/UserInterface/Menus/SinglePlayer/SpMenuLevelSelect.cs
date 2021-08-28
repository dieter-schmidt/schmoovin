using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NeoFPS.Samples
{
	public class SpMenuLevelSelect : MenuPanel
	{
		[SerializeField] private MultiInputLevelSelect m_PrototypeEntry = null;
		[SerializeField] private LevelInfo[] m_Levels = new LevelInfo[0];

		private MultiInputLevelSelect[] m_Entries;

		[Serializable]
		private struct LevelInfo
		{
            #pragma warning disable 0649

            public string displayName;
			public string loadName;
			[Multiline]
			public string description;
			public Sprite screenshot;

            #pragma warning restore 0649
        }

        public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);

			if (m_Levels.Length == 0)
			{
				m_PrototypeEntry.gameObject.SetActive (false);
				return;
			}

			m_Entries = new MultiInputLevelSelect[m_Levels.Length];
			m_Entries [0] = m_PrototypeEntry;
			Transform root = m_PrototypeEntry.transform.parent;

			for (int i = 0; i < m_Levels.Length; ++i)
			{
				// Instantiate entry
				if (i > 0)
					m_Entries [i] = Instantiate (m_PrototypeEntry);

				// Parent and position
				Transform t = m_Entries [i].transform;
				t.SetParent (root);
				t.localPosition = Vector3.zero;
				t.localRotation = Quaternion.identity;
				t.localScale = Vector3.one;

				// Set up info
				m_Entries [i].label = m_Levels [i].displayName;
				m_Entries [i].level = m_Levels [i].loadName;
				m_Entries [i].description = m_Levels [i].description;
				m_Entries [i].screenshot = m_Levels [i].screenshot;
			}
		}
	}
}