#if UNITY_STANDALONE 
// Should other platforms use Json text files saved to disk?
#define SETTINGS_USES_JSON
using System.IO;
#endif

using System;
using System.Collections;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/neofpsref-mb-fpskeybindings.html")]
    [CreateAssetMenu(fileName = "FpsSettings_KeyBindings", menuName = "NeoFPS/Settings/KeyBindings", order = NeoFpsMenuPriorities.settings_keybindings)]
    public class FpsKeyBindings : SettingsContext<FpsKeyBindings>
    {
        protected override string contextName { get { return "KeyBindings"; } }

        public override string displayTitle { get { return "NeoFPS Key Bindings"; } }

        public override string tocName { get { return "Key Bindings"; } }

        public override string tocID { get { return "settings_keybindings"; } }

        [SerializeField, Tooltip("The keyboard keycodes assigned based on the FpsInputButton generated constant.")]
        private KeyCodePair[] m_KeyCodes = null;

        [Serializable]
        private struct KeyCodePair
        {
            [Tooltip("The primary key or button.")]
            public KeyCode primary;
            [Tooltip("The secondary (alternative) key or button.")]
            public KeyCode secondary;

            public KeyCodePair(KeyCode k1, KeyCode k2)
            {
                primary = k1;
                secondary = k2;
            }
        }

        public void ResetToDefault(KeyboardLayout layout = KeyboardLayout.Qwerty)
        {
            m_KeyCodes = new KeyCodePair[FpsInputButton.count];
            for (int i = 0; i < m_KeyCodes.Length; ++i)
            {
                m_KeyCodes[i] = new KeyCodePair(
                    NeoFpsInputManager.GetDefaultPrimaryKey(i, layout),
                    NeoFpsInputManager.GetDefaultSecondaryKey(i, layout)
                    );
            }
        }

        public override void OnLoad()
        {
            // This needs improving to keep existing bindings
            if (m_KeyCodes.Length != FpsInputButton.count)
                ResetToDefault();

            //settingsObject.AddComponent<ProceduralInputBehaviour>();
        }

        protected override bool CheckIfCurrent()
        {
            return FpsSettings.keyBindings == this;
        }

        public KeyCode GetPrimaryKey(FpsInputButton button)
        {
            return m_KeyCodes[button].primary;
        }

        public KeyCode GetSecondaryKey(FpsInputButton button)
        {
            return m_KeyCodes[button].secondary;
        }

        public bool GetButton(FpsInputButton button)
        {
#if UNITY_EDITOR
            if (ProceduralInputBehaviour.GetButton(button))
                return true;
#endif

            KeyCode p = m_KeyCodes[button].primary;
            KeyCode s = m_KeyCodes[button].secondary;

            // Not bound
            if (p == KeyCode.None && s == KeyCode.None)
                return false;

            // One binding
            if (s == KeyCode.None)
                return Input.GetKey(p);
            if (p == KeyCode.None)
                return Input.GetKey(s);

            // Two bindings
            return Input.GetKey(p) || Input.GetKey(s);
        }

        public bool GetButtonDown(FpsInputButton button)
        {
#if UNITY_EDITOR
            if (ProceduralInputBehaviour.GetButtonDown(button))
                return true;
#endif

            KeyCode p = m_KeyCodes[button].primary;
            KeyCode s = m_KeyCodes[button].secondary;

            // Not bound
            if (p == KeyCode.None && s == KeyCode.None)
                return false;

            // One binding
            if (s == KeyCode.None)
                return Input.GetKeyDown(p);
            if (p == KeyCode.None)
                return Input.GetKeyDown(s);

            // Two bindings
            int down = 0;
            if (Input.GetKeyDown(p))
                ++down;
            if (Input.GetKeyDown(s))
                ++down;
            switch (down)
            {
                case 0:
                    return false;
                case 2:
                    return true;
                default:
                    {
                        int held = 0;
                        if (Input.GetKey(p))
                            ++held;
                        if (Input.GetKey(s))
                            ++held;
                        return held == 1;
                    }
            }
        }

        public bool GetButtonUp(FpsInputButton button)
        {
#if UNITY_EDITOR
            if (ProceduralInputBehaviour.GetButtonReleased(button))
                return true;
#endif

            KeyCode p = m_KeyCodes[button].primary;
            KeyCode s = m_KeyCodes[button].secondary;

            // Not bound
            if (p == KeyCode.None && s == KeyCode.None)
                return false;

            // One binding
            if (s == KeyCode.None)
                return Input.GetKeyUp(p);
            if (p == KeyCode.None)
                return Input.GetKeyUp(s);

            // Two bindings
            int up = 0;
            if (Input.GetKeyUp(p))
                ++up;
            if (Input.GetKeyUp(s))
                ++up;
            switch (up)
            {
                case 0:
                    return false;
                case 2:
                    return true;
                default:
                    {
                        int held = 0;
                        if (Input.GetKey(p))
                            ++held;
                        if (Input.GetKey(s))
                            ++held;
                        return held == 0;
                    }
            }
        }

        #region BINDING

        // Rebinding callback
        public delegate void OnRebindDelegate(FpsInputButton button, bool primary, KeyCode to);
        public event OnRebindDelegate onRebind;

        private KeyRebinderBehaviour m_Rebinder = null;
        private KeyRebinderBehaviour rebinder
        {
            get
            {
                if (m_Rebinder == null)
                {
                    var settingsObject = FpsSettings.runtimeSettingsObject;
                    m_Rebinder = settingsObject.AddComponent<KeyRebinderBehaviour>();
                }
                return m_Rebinder;
            }
        }

        public void RebindInput(FpsInputButton button, bool primary)
        {
            rebinder.RebindInput(button, primary);
        }

        private class KeyRebinderBehaviour : MonoBehaviour
        {
            private Coroutine m_CheckRebindCoroutine;
            private FpsInputButton m_Rebinding = FpsInputButton.None;
            private bool m_RebindingPrimary = false;

            enum SpecialKey
            {
                None,
                Cancel,
                Clear
            }

            private SpecialKey CheckSpecials()
            {
#if UNITY_WEBGL
                if (Input.GetKeyDown(KeyCode.Tab))
                    return SpecialKey.Cancel;
#endif
                if (Input.GetKeyDown(KeyCode.Escape) ||
                    Input.GetKeyDown(KeyCode.LeftWindows) ||
                    Input.GetKeyDown(KeyCode.RightWindows) ||
                    Input.GetKeyDown(KeyCode.LeftApple) ||
                    Input.GetKeyDown(KeyCode.RightApple))
                    return SpecialKey.Cancel;

                if (Input.GetKeyDown(KeyCode.Backspace) ||
                    Input.GetKeyDown(KeyCode.Delete))
                    return SpecialKey.Clear;

                return SpecialKey.None;
            }

            private bool CheckValid(KeyCode code)
            {
                if (code == KeyCode.None)
                    return false;
                if (code >= KeyCode.JoystickButton0 && code <= KeyCode.Joystick8Button19)
                    return false;
                return true;
            }

            private KeyCode GetKeyDown()
            {
                // Check if any key pressed
                if (!Input.anyKeyDown)
                    return KeyCode.None;

                // Loop through possible keycodes
                foreach (var uncast in Enum.GetValues(typeof(KeyCode)))
                {
                    KeyCode current = (KeyCode)uncast;
                    if (Input.GetKeyDown(current) && CheckValid(current))
                        return current;
                }

                return KeyCode.None;
            }

            private IEnumerator CheckForRebind()
            {
                NeoFpsInputManager.PushEscapeHandler(StubEscapeHandler);

                while (m_Rebinding != FpsInputButton.None)
                {
                    yield return null;

                    // In rebinding mode here. First valid key pressed
                    // will be bound to pending action and saved in settings

                    if (Input.anyKeyDown)
                    {
                        switch (CheckSpecials())
                        {
                            case SpecialKey.Cancel:
                                CancelRebinding();
                                break;
                            case SpecialKey.Clear:
                                ApplyRebind(KeyCode.None);
                                break;
                            default:
                                {
                                    KeyCode down = GetKeyDown();
                                    if (CheckValid(down))
                                        ApplyRebind(down);
                                }
                                break;
                        }

                        Input.ResetInputAxes();
                    }
                }

                NeoFpsInputManager.PopEscapeHandler(StubEscapeHandler);
                m_CheckRebindCoroutine = null;
            }

            void StubEscapeHandler()
            {
            }

            public void RebindInput(FpsInputButton button, bool primary)
            {
                if (button != FpsInputButton.None)
                {
                    m_Rebinding = button;

                    if (m_CheckRebindCoroutine != null)
                    {
                        StopCoroutine(m_CheckRebindCoroutine);
                        NeoFpsInputManager.PopEscapeHandler(StubEscapeHandler);
                    }

                    m_RebindingPrimary = primary;
                    instance.dirty = true;

                    m_CheckRebindCoroutine = StartCoroutine(CheckForRebind());
                }
                else
                    CancelRebinding();
            }

            private void ApplyRebind(KeyCode keyCode)
            {
                // If setting a key, check for clashes with other bindings
                if (keyCode != KeyCode.None)
                {
                    for (int i = 0; i < FpsInputButton.count; ++i)
                    {
                        if (instance.m_KeyCodes[i].primary == keyCode)
                        {
                            // Cancel if rebinding to same key. Swap if not
                            FpsInputButton b = i;
                            if (b == m_Rebinding && m_RebindingPrimary)
                            {
                                CancelRebinding();
                                return;
                            }
                            else
                            {
                                // Clear the other binding if required
                                bool clash = !KeyBindingContextMatrix.CanOverlap(NeoFpsInputManager.GetKeyBindingContext(m_Rebinding), NeoFpsInputManager.GetKeyBindingContext(b));
                                //(
                                //    NeoFpsInputManager.GetKeyBindingContext(m_Rebinding) == KeyBindingContext.Default ||
                                //    NeoFpsInputManager.GetKeyBindingContext(b) == KeyBindingContext.Default ||
                                //    NeoFpsInputManager.GetKeyBindingContext(b) == NeoFpsInputManager.GetKeyBindingContext(m_Rebinding)
                                //    );
                                if (clash)
                                {
                                    instance.m_KeyCodes[i].primary = FpsInputButton.None;
                                    if (instance.onRebind != null)
                                        instance.onRebind(i, true, FpsInputButton.None);
                                }
                            }
                        }

                        if (instance.m_KeyCodes[i].secondary == keyCode)
                        {
                            // Cancel if rebinding to same key. Swap if not
                            FpsInputButton b = i;
                            if (b == m_Rebinding && !m_RebindingPrimary)
                            {
                                CancelRebinding();
                                return;
                            }
                            else
                            {
                                // Clear the other binding if required
                                bool clash = !KeyBindingContextMatrix.CanOverlap(NeoFpsInputManager.GetKeyBindingContext(m_Rebinding), NeoFpsInputManager.GetKeyBindingContext(b));
                                //bool clash = (
                                //    NeoFpsInputManager.GetKeyBindingContext(m_Rebinding) == KeyBindingContext.Default ||
                                //    NeoFpsInputManager.GetKeyBindingContext(b) == KeyBindingContext.Default ||
                                //    NeoFpsInputManager.GetKeyBindingContext(b) == NeoFpsInputManager.GetKeyBindingContext(m_Rebinding)
                                //    );
                                if (clash)
                                {
                                    instance.m_KeyCodes[i].secondary = FpsInputButton.None;
                                    if (instance.onRebind != null)
                                        instance.onRebind(i, false, FpsInputButton.None);
                                }
                            }
                        }
                    }
                }

                // Rebind
                if (m_RebindingPrimary)
                    instance.m_KeyCodes[m_Rebinding].primary = keyCode;
                else
                    instance.m_KeyCodes[m_Rebinding].secondary = keyCode;

                // Notify
                if (instance.onRebind != null)
                    instance.onRebind(m_Rebinding, m_RebindingPrimary, keyCode);

                m_Rebinding = FpsInputButton.None;
            }

            private void CancelRebinding()
            {
                if (instance.onRebind != null && m_Rebinding != FpsInputButton.None)
                {
                    if (m_RebindingPrimary)
                        instance.onRebind(m_Rebinding, true, instance.m_KeyCodes[m_Rebinding].primary);
                    else
                        instance.onRebind(m_Rebinding, false, instance.m_KeyCodes[m_Rebinding].secondary);
                }

                m_Rebinding = FpsInputButton.None;
            }

            private KeyCode GetOldBinding()
            {
                if (m_Rebinding == FpsInputButton.None)
                    return KeyCode.None;
                if (m_RebindingPrimary)
                    return instance.m_KeyCodes[m_Rebinding].primary;
                else
                    return instance.m_KeyCodes[m_Rebinding].secondary;
            }
        }

        #endregion

        #region PROCEDURAL INPUT (FOR TESTING AND RECORDING)
#if UNITY_EDITOR

        private class ProceduralInputBehaviour : MonoBehaviour
        {
            Coroutine m_ProceduralInputCoroutine = null;
            ButtonState[] m_ProceduralButtonStates = new ButtonState[FpsInputButton.count];

            private static ProceduralInputBehaviour s_Instance = null;

            public enum ButtonState
            {
                Up,
                Pressed,
                Down,
                Released
            }

            public static bool GetButtonDown(FpsInputButton button)
            {
                if (s_Instance != null)
                    return s_Instance.m_ProceduralButtonStates[button] == ButtonState.Pressed;
                else
                    return false;
            }

            public static bool GetButton(FpsInputButton button)
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_ProceduralButtonStates[button] == ButtonState.Down ||
                        s_Instance.m_ProceduralButtonStates[button] == ButtonState.Pressed;
                }
                else
                    return false;
            }

            public static bool GetButtonReleased(FpsInputButton button)
            {
                if (s_Instance != null)
                    return s_Instance.m_ProceduralButtonStates[button] == ButtonState.Released;
                else
                    return false;
            }

            void Awake()
            {
                s_Instance = this;
            }

            void Update()
            {
                if (Input.GetKeyDown(KeyCode.Insert))
                {
                    if (m_ProceduralInputCoroutine != null)
                        StopCoroutine(m_ProceduralInputCoroutine);
                    ResetProceduralInput();
                    m_ProceduralInputCoroutine = StartCoroutine(ProceduralInputCoroutine());
                }
            }

            void ResetProceduralInput()
            {
                for (int i = 0; i < m_ProceduralButtonStates.Length; ++i)
                    m_ProceduralButtonStates[i] = ButtonState.Up;
                m_ProceduralInputCoroutine = null;
            }

            private float m_ProceduralInputStartTime;
            IEnumerator ProceduralInputCoroutine()
            {
                m_ProceduralInputStartTime = Time.realtimeSinceStartup;
                yield return WaitForTime(1f);
                yield return TapButton(FpsInputButton.Quickslot1);
                yield return WaitForTime(2f);
                yield return TapButton(FpsInputButton.PrimaryFire);
                yield return WaitForTime(4f);
                yield return PressButton(FpsInputButton.SecondaryFire);
                yield return WaitForTime(6f);
                yield return ReleaseButton(FpsInputButton.SecondaryFire);
                yield return WaitForTime(8f);
                yield return TapButton(FpsInputButton.Quickslot8);
                yield return WaitForTime(9f);
                yield return TapButton(FpsInputButton.PrimaryFire);
                yield return WaitForTime(14f);
                yield return TapButton(FpsInputButton.SecondaryFire);

                // Cleanup
                ResetProceduralInput();
            }


            IEnumerator TapButton(FpsInputButton button)
            {
                m_ProceduralButtonStates[button] = ButtonState.Pressed;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Down;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Released;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Up;
            }
            IEnumerator PressButton(FpsInputButton button)
            {
                m_ProceduralButtonStates[button] = ButtonState.Pressed;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Down;
            }
            IEnumerator ReleaseButton(FpsInputButton button)
            {
                m_ProceduralButtonStates[button] = ButtonState.Released;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Up;
            }
            IEnumerator HoldButton(FpsInputButton button, float duration)
            {
                m_ProceduralButtonStates[button] = ButtonState.Pressed;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Down;
                yield return new WaitForSecondsRealtime(duration - (Time.deltaTime * 2f));
                m_ProceduralButtonStates[button] = ButtonState.Released;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Up;
            }

            IEnumerator WaitForTime(float targetTime)
            {
                targetTime += m_ProceduralInputStartTime;
                while (Time.realtimeSinceStartup < targetTime)
                    yield return null;
            }

            IEnumerator TapButtonCoroutine(FpsInputButton button)
            {
                m_ProceduralButtonStates[button] = ButtonState.Pressed;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Down;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Released;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Up;
            }

            IEnumerator PressButtonCoroutine(FpsInputButton button)
            {
                m_ProceduralButtonStates[button] = ButtonState.Pressed;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Down;
            }

            IEnumerator ReleaseButtonCoroutine(FpsInputButton button)
            {
                m_ProceduralButtonStates[button] = ButtonState.Released;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Up;
            }

            IEnumerator HoldButtonCoroutine(FpsInputButton button, float duration)
            {
                m_ProceduralButtonStates[button] = ButtonState.Pressed;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Down;
                yield return new WaitForSecondsRealtime(duration - (Time.deltaTime * 2f));
                m_ProceduralButtonStates[button] = ButtonState.Released;
                yield return null;
                m_ProceduralButtonStates[button] = ButtonState.Up;
            }
        }

#endif
        #endregion
    }
}