using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
	public abstract class FpsInput : MonoBehaviour
	{
        private static List<FpsInput>[] s_ContextReferences;

		public static FpsInputContext currentContext
		{
			get
			{
                for (int i = FpsInputContext.count - 2; i >= 0; --i)
                {
                    if (s_ContextReferences[i].Count > 0)
                        return i + 1;
                }
                return FpsInputContext.None;
			}
		}

        private bool m_Pushed = false;
        private bool m_HadFocus = false;

		public virtual FpsInputContext inputContext
		{
			get { return FpsInputContext.None; }
		}

		public bool hasFocus
		{
			get { return inputContext == FpsInputContext.None || inputContext == currentContext; }
		}

        private void Awake()
        {
            if (s_ContextReferences == null)
            {
                s_ContextReferences = new List<FpsInput>[FpsInputContext.count - 1];
                for (int i = 0; i < FpsInputContext.count - 1; ++i)
                    s_ContextReferences[i] = new List<FpsInput>(4);
            }
            OnAwake();
        }

        protected virtual void OnAwake ()
        { }

        protected virtual void OnEnable()
        {
            PushContext();
        }

		protected virtual void OnDisable()
		{
			PopContext();
		}

		public void PushContext ()
		{
            if (m_Pushed || inputContext == FpsInputContext.None)
                return;

            var list = s_ContextReferences[inputContext - 1];
            list.Add(this);
            m_Pushed = true;
		}
        
		public void PopContext ()
        {
            if (!m_Pushed || inputContext == FpsInputContext.None)
                return;

            var list = s_ContextReferences[inputContext - 1];
            list.Remove(this);
            m_Pushed = false;
        }

		public float GetAxis (FpsInputAxis axis)
		{
			if (!hasFocus)
				return 0f;
			
			switch (axis)
			{
				case FpsInputAxis.MouseX:
					return Input.GetAxis (NeoFpsInputManager.mouseXAxis);
				case FpsInputAxis.MouseY:
					return Input.GetAxis (NeoFpsInputManager.mouseYAxis);
				case FpsInputAxis.MouseScroll:
					return Input.GetAxis (NeoFpsInputManager.mouseScrollAxis);
				default:
					return NeoFpsInputManager.gamepad.GetAxis (axis);
			}
		}

		public float GetAxisRaw (FpsInputAxis axis)
		{
			if (!hasFocus)
				return 0f;
			
			switch (axis)
			{
				case FpsInputAxis.MouseX:
					return Input.GetAxisRaw (NeoFpsInputManager.mouseXAxis);
				case FpsInputAxis.MouseY:
					return Input.GetAxisRaw (NeoFpsInputManager.mouseYAxis);
				case FpsInputAxis.MouseScroll:
					return Input.GetAxisRaw (NeoFpsInputManager.mouseScrollAxis);
				default:
					return NeoFpsInputManager.gamepad.GetAxisRaw (axis);
			}
		}

		public bool GetButton (FpsInputButton button)
		{
			if (!hasFocus)
				return false;

			if (FpsSettings.keyBindings.GetButton(button))
				return true;
			return NeoFpsInputManager.gamepad.GetButton(button);
		}

		public bool GetButtonDown (FpsInputButton button)
		{
			if (!hasFocus)
				return false;

			if (FpsSettings.keyBindings.GetButtonDown(button))
				return !NeoFpsInputManager.gamepad.GetButton(button);
			if (NeoFpsInputManager.gamepad.GetButtonDown(button))
				return !FpsSettings.keyBindings.GetButton(button);
			return false;
		}

		public bool GetButtonUp (FpsInputButton button)
		{
			if (!hasFocus)
				return false;

			if (FpsSettings.keyBindings.GetButtonUp(button))
				return !NeoFpsInputManager.gamepad.GetButton(button);
			if (NeoFpsInputManager.gamepad.GetButtonUp(button))
				return !FpsSettings.keyBindings.GetButton(button);
			return false;
		}

        protected abstract void UpdateInput();

        protected virtual void OnGainFocus() { }
        protected virtual void OnLoseFocus() { }

        protected virtual void Update()
		{
			if (inputContext == FpsInputContext.None || (inputContext == currentContext && m_Pushed))
            {
                if (!m_HadFocus)
                {
                    m_HadFocus = true;
                    OnGainFocus();
                }

				UpdateInput();
            }
            else
            {
                if (m_HadFocus)
                {
                    m_HadFocus = false;
                    OnLoseFocus();
                }
            }
        }
    }
}
