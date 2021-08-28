using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace NeoFPS.Samples
{
	public class MultiInputMultiChoice : MultiInputMultiChoiceBase
	{
		[SerializeField] private int m_StartingIndex = 0;
        [SerializeField] private IndexChangeEvent m_OnIndexChanged = new IndexChangeEvent();

		[Serializable]
		public class IndexChangeEvent : UnityEvent<int> {}

		public IndexChangeEvent onIndexChanged
		{
			get { return m_OnIndexChanged; }
		}

		#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			base.OnValidate ();
			m_StartingIndex = Mathf.Clamp (m_StartingIndex, 0, options.Length - 1);
		}
		#endif

		protected override void Awake ()
		{
			base.Awake ();
			if (index == -1)
				SetStartingIndex (m_StartingIndex);
		}

		protected override void OnIndexChanged (int to)
		{
			onIndexChanged.Invoke (to);
		}
	}
}
