using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
	public interface IInteractiveObject
	{
        string tooltipName { get; }
        string tooltipAction { get; }

        event UnityAction onTooltipChanged;
        event UnityAction onUsed;
        event UnityAction onCursorEnter;
        event UnityAction onCursorExit;

        bool highlighted { get; set; }
		bool interactable { get; set; }
		float holdDuration { get; }

		void Interact (ICharacter character);
	}
}