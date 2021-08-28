using UnityEngine;
using NeoFPS.Constants;
using System.Collections;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inputref-mb-inputlockpick.html")]
    public class InputLockpick : FpsInput
    {
        [SerializeField, Tooltip("The maximum turn rate of the pick object in degrees per second.")]
        private float m_AnalogueTurnRate = 90f;

        private IPickAngleLockpickPopup m_LockpickPopup = null;

        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Menu; }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            m_LockpickPopup = GetComponent<IPickAngleLockpickPopup>();
            if (m_LockpickPopup == null)
                Debug.LogError("InputLockpick is placed on a gameobject without a lockpick popup (ILockpickPopup)");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Capture mouse cursor
            NeoFpsInputManager.captureMouseCursor = true;
            StartCoroutine(MouseCapture());

            // Push escape handler
            NeoFpsInputManager.PushEscapeHandler(m_LockpickPopup.Cancel);
        }

        IEnumerator MouseCapture()
        {
            yield return null;
            NeoFpsInputManager.captureMouseCursor = true;
        }

        protected override void OnDisable()
        {
            // Pop escape handler
            NeoFpsInputManager.PopEscapeHandler(m_LockpickPopup.Cancel);

            // Capture mouse cursor
            NeoFpsInputManager.captureMouseCursor = false;

            base.OnDisable();
        }

        protected override void UpdateInput()
        {
            if (m_LockpickPopup != null)
            {
                // Get rotation (sum of mouse and both analogues
                float rotatePick = GetAxis(FpsInputAxis.MouseX);
                rotatePick += GetAxis(FpsInputAxis.LookX) * m_AnalogueTurnRate * Time.deltaTime;
                rotatePick += GetAxis(FpsInputAxis.MoveX) * m_AnalogueTurnRate * Time.deltaTime;

                // Get primary mouse button
                bool tension = Input.GetKey(KeyCode.Mouse0) || GetButton(FpsInputButton.Right);

                // Get A (Xbox) or Cross (PS) gamepad button
                var gamepad = NeoFpsInputManager.connectedGamepad;
                if (gamepad != null)
                    tension |= gamepad.GetButton(GamepadButton.ButtonAorCross);

                // Apply input
                m_LockpickPopup.ApplyInput(rotatePick, tension);

                // Escape / back button handling
                if (GetButtonDown(FpsInputButton.Back))
                    m_LockpickPopup.Cancel();
            }
        }
    }
}
