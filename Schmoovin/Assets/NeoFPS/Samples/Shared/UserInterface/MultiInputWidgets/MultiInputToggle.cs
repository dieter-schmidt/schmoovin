using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace NeoFPS.Samples
{
	public class MultiInputToggle : MultiInputMultiChoiceBase
	{
#pragma warning disable 0414
        [SerializeField] private ToggleType m_ToggleType = ToggleType.YesNo;
#pragma warning restore 0414
        [SerializeField] private bool m_StartingValue = true;
        [SerializeField] private ValueChangeEvent m_OnValueChanged = new ValueChangeEvent();

		[Serializable]
		public class ValueChangeEvent : UnityEvent<bool> {}

		public enum ToggleType
		{
			YesNo,
			OnOff,
			EnabledDisabled,
			TrueFalse,
			ToggleHold
		}

		public bool value
		{
			get { return index != 0; }
			set
			{
				if (value)
					index = 1;
				else
					index = 0;
			}
		}

		public ValueChangeEvent onValueChanged
		{
			get { return m_OnValueChanged; }
		}

		#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			base.OnValidate ();
			string[] currentOptions = options;
			if (currentOptions.Length != 2)
				currentOptions = new string[2];
			switch (m_ToggleType)
			{
				case ToggleType.YesNo:
					currentOptions [0] = "No";
					currentOptions [1] = "Yes";
					break;
				case ToggleType.OnOff:
					currentOptions [0] = "Off";
					currentOptions [1] = "On";
					break;
				case ToggleType.EnabledDisabled:
					currentOptions [0] = "Disabled";
					currentOptions [1] = "Enabled";
					break;
				case ToggleType.TrueFalse:
					currentOptions [0] = "false";
					currentOptions [1] = "true";
					break;
				case ToggleType.ToggleHold:
					currentOptions [0] = "Hold";
					currentOptions [1] = "Toggle";
					break;
			}
			options = currentOptions;
		}
		#endif

		protected override void Awake ()
		{
			base.Awake ();
			if (index == -1)
				SetStartingIndex (m_StartingValue ? 1 : 0);
		}

		protected override void OnIndexChanged (int to)
		{
			onValueChanged.Invoke (to != 0);
		}

        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(DelayedAlign());
        }

        IEnumerator DelayedAlign()
        {
            yield return null;
            // Fix child rects randomly resizing
            Transform t = transform;
            if (t.childCount == 2)
            {
                RectTransform rt = (RectTransform)t.GetChild(1);
                rt.anchoredPosition = new Vector2(1f, 0f);
            }
        }
    }
}
