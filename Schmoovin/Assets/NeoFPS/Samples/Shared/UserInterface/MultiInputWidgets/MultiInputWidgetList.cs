using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    public class MultiInputWidgetList : MonoBehaviour
	{
		[SerializeField] private MultiInputWidgetList m_NavigateLeft = null;
        [SerializeField] private MultiInputWidgetList m_NavigateRight = null;

        public void SelectDirection (MoveDirection dir)
		{
			if (dir == MoveDirection.Left && m_NavigateLeft != null && m_NavigateLeft.isActiveAndEnabled)
				m_NavigateLeft.SelectFirst ();
			if (dir == MoveDirection.Right && m_NavigateRight != null && m_NavigateRight.isActiveAndEnabled)
				m_NavigateRight.SelectFirst ();
		}

		public void SelectFirst ()
		{
			MultiInputWidget first = GetComponentInChildren<MultiInputWidget> (false);
			if (first != null)
				EventSystem.current.SetSelectedGameObject (first.gameObject);
		}

		public void ResetWidgetNavigation ()
		{
			MultiInputWidget[] children = GetComponentsInChildren<MultiInputWidget> (false);
            				
			switch (children.Length)
			{
				case 0:
					break;
				case 1:
					{
						Navigation n = children [0].navigation;
						n.selectOnUp = null;
						n.selectOnDown = null;
						children [0].navigation = n;
					}
					break;
				case 2:
					{
						// Set up first widget
						Navigation n = children [0].navigation;
						n.selectOnUp = children [1];
						n.selectOnDown = children [1];
						children [0].navigation = n;

						// set up second widget
						n = children [0].navigation;
						n.selectOnUp = children [0];
						n.selectOnDown = children [0];
						children [1].navigation = n;
					}
					break;
				default:
					{
						// Set up first widget
						Navigation n = children [0].navigation;
						n.selectOnUp = children [children.Length - 1];
						n.selectOnDown = children [1];
						children [0].navigation = n;

						// Set up last widget
						n = children [children.Length - 1].navigation;
						n.selectOnUp = children [children.Length - 2];
						n.selectOnDown = children [0];
						children [children.Length - 1].navigation = n;

						// Set up middle widgets
						for (int i = 1; i < children.Length - 1; ++i)
						{
							n = children [i].navigation;
							n.selectOnUp = children [i - 1];
							n.selectOnDown = children [i + 1];
							children [i].navigation = n;
						}
					}
					break;
			}
		}
	}
}

