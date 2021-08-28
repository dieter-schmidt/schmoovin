#if !UNITY_2018_OR_NEWER && UNITY_EDITOR
#define AGGRESIVE_CURSOR_LOCK
#endif

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
#if UNITY_EDITOR
using NeoFPS.ScriptGeneration;
using System;
#endif

namespace NeoFPS
{
    [CreateAssetMenu(fileName = "FpsManager_Input", menuName = "NeoFPS/Managers/Input Manager", order = NeoFpsMenuPriorities.manager_input)]
    [HelpURL("https://docs.neofps.com/manual/inputref-so-inputmanager.html")]
    public class NeoFpsInputManager : NeoFpsManager<NeoFpsInputManager>
    {
        private static RuntimeBehaviour s_ProxyBehaviour = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LoadInputManager()
        {
            GetInstance("FpsManager_Input");
        }

        protected override void Initialise()
        {
            InitialiseKeyboardLayouts();
            InitialiseGamepads();
            s_ProxyBehaviour = GetBehaviourProxy<RuntimeBehaviour>();
            m_Gamepad = GetBehaviourProxy<GamepadInput>();
        }

        public override bool IsValid()
        {
            return true;
        }

        #region CURSOR CAPTURE

        private static bool m_CaptureMouseCursor;
        public static bool captureMouseCursor
        {
            get { return m_CaptureMouseCursor; }
            set
            {
                m_CaptureMouseCursor = value;
                if (m_CaptureMouseCursor)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        #endregion

        #region INPUT AXES

        [SerializeField, Tooltip("The horizontal mouse axis in the Unity input settings.")]
        private string m_MouseXAxis = "Mouse X";

        [SerializeField, Tooltip("The vertical mouse axis in the Unity input settings.")]
        private string m_MouseYAxis = "Mouse Y";

        [SerializeField, Tooltip("The mouse scroll wheel axis in the Unity input settings.")]
        private string m_MouseScrollAxis = "Mouse ScrollWheel";

        public static string mouseXAxis
        {
            get { return instance.m_MouseXAxis; }
        }
        public static string mouseYAxis
        {
            get { return instance.m_MouseYAxis; }
        }
        public static string mouseScrollAxis
        {
            get { return instance.m_MouseScrollAxis; }
        }

        #endregion

        #region INPUT BUTTONS

        public static readonly string[] fixedInputButtons = { "None", "Menu", "Back", "Cancel" };

        // Fixed buttons include: None (0), Menu (1), Back (2), Cancel (3)
        [SerializeField, Tooltip("The input buttons the game should use.")]
        private InputButtonInfo[] m_InputButtons = { };

        [SerializeField, Tooltip("The keyboard layout for the default input buttons.")]
        private KeyboardLayout m_DefaultKeyboardLayout = KeyboardLayout.Qwerty;

        public static string GetButtonDisplayName(FpsInputButton button)
        {
            if (button < fixedInputButtons.Length)
                return fixedInputButtons[button];
            else
                return instance.m_InputButtons[button - fixedInputButtons.Length].displayName;
        }

        public static InputCategory GetButtonCategory(FpsInputButton button)
        {
            if (button < fixedInputButtons.Length)
                return InputCategory.Miscellaneous;
            else
                return instance.m_InputButtons[button - fixedInputButtons.Length].category;
        }

        public static KeyBindingContext GetKeyBindingContext(FpsInputButton button)
        {
            if (button < fixedInputButtons.Length)
                return KeyBindingContext.Default;
            else
                return instance.m_InputButtons[button - fixedInputButtons.Length].context;
        }

        public static KeyCode GetDefaultPrimaryKey(FpsInputButton button, KeyboardLayout layout)
        {
            if (button < fixedInputButtons.Length)
                return KeyCode.None;
            else
            {
                var result = instance.m_InputButtons[button - fixedInputButtons.Length].defaultPrimary;
                if (layout != instance.m_DefaultKeyboardLayout)
                    result = GetTranslatedKey(result, layout);
                return result;
            }
        }

        public static KeyCode GetDefaultSecondaryKey(FpsInputButton button, KeyboardLayout layout)
        {
            if (button < fixedInputButtons.Length)
                return KeyCode.None;
            else
            {
                var result = instance.m_InputButtons[button - fixedInputButtons.Length].defaultSecondary;
                if (layout != instance.m_DefaultKeyboardLayout)
                    result = GetTranslatedKey(result, layout);
                return result;
            }
        }

        #endregion

        #region KEYBOARD LAYOUTS

        // s_Layouts is from qwerty, s_ReverseLayouts is to qwerty
        static Dictionary<KeyCode, KeyCode>[] s_Layouts = null;
        static Dictionary<KeyCode, KeyCode>[] s_ReverseLayouts = null;

        static KeyCode GetTranslatedKey(KeyCode key, KeyboardLayout layout)
        {
            var defaultLayout = instance.m_DefaultKeyboardLayout;
            if (layout != defaultLayout)
            {
                // Get qwerty key
                var qwerty = key;
                if (defaultLayout != KeyboardLayout.Qwerty)
                {
                    var map = s_ReverseLayouts[(int)defaultLayout - 1];
                    if (map.ContainsKey(key))
                        qwerty = map[key];
                }

                // Get translated from qwerty to layout
                if (layout == KeyboardLayout.Qwerty)
                    return qwerty;
                else
                {
                    var map = s_Layouts[(int)layout - 1];
                    if (map.ContainsKey(qwerty))
                        return map[qwerty];
                    else
                        return qwerty;
                }
            }
            else
                return key;
        }

        static void InitialiseKeyboardLayouts()
        {
            s_Layouts = new Dictionary<KeyCode, KeyCode>[4];
            s_Layouts[0] = GetLayoutAzerty();
            s_Layouts[1] = GetLayoutQwertz();
            s_Layouts[2] = GetLayoutDvorak();
            s_Layouts[3] = GetLayoutColemak();

            // Create reverse maps
            s_ReverseLayouts = new Dictionary<KeyCode, KeyCode>[s_Layouts.Length];
            for (int i = 0; i < s_Layouts.Length; ++i)
            {
                s_ReverseLayouts[i] = new Dictionary<KeyCode, KeyCode>();
                foreach(var pair in s_Layouts[i])
                    s_ReverseLayouts[i].Add(pair.Value, pair.Key);
            }
        }

        static Dictionary<KeyCode, KeyCode> GetLayoutAzerty()
        {
            var layout = new Dictionary<KeyCode, KeyCode>();
            layout.Add(KeyCode.Q, KeyCode.A);
            layout.Add(KeyCode.W, KeyCode.Z);
            layout.Add(KeyCode.E, KeyCode.E);
            layout.Add(KeyCode.R, KeyCode.R);
            layout.Add(KeyCode.T, KeyCode.T);
            layout.Add(KeyCode.Y, KeyCode.Y);
            layout.Add(KeyCode.U, KeyCode.U);
            layout.Add(KeyCode.I, KeyCode.I);
            layout.Add(KeyCode.O, KeyCode.O);
            layout.Add(KeyCode.P, KeyCode.P);

            layout.Add(KeyCode.A, KeyCode.Q);
            layout.Add(KeyCode.S, KeyCode.S);
            layout.Add(KeyCode.D, KeyCode.D);
            layout.Add(KeyCode.F, KeyCode.F);
            layout.Add(KeyCode.G, KeyCode.G);
            layout.Add(KeyCode.H, KeyCode.H);
            layout.Add(KeyCode.J, KeyCode.J);
            layout.Add(KeyCode.K, KeyCode.K);
            layout.Add(KeyCode.L, KeyCode.L);
            layout.Add(KeyCode.Semicolon, KeyCode.M);

            layout.Add(KeyCode.Z, KeyCode.W);
            layout.Add(KeyCode.X, KeyCode.X);
            layout.Add(KeyCode.C, KeyCode.C);
            layout.Add(KeyCode.V, KeyCode.V);
            layout.Add(KeyCode.B, KeyCode.B);
            layout.Add(KeyCode.N, KeyCode.N);
            layout.Add(KeyCode.M, KeyCode.Comma);
            return layout;
        }

        static Dictionary<KeyCode, KeyCode> GetLayoutQwertz()
        {
            var layout = new Dictionary<KeyCode, KeyCode>();
            layout.Add(KeyCode.Q, KeyCode.Q);
            layout.Add(KeyCode.W, KeyCode.W);
            layout.Add(KeyCode.E, KeyCode.E);
            layout.Add(KeyCode.R, KeyCode.R);
            layout.Add(KeyCode.T, KeyCode.T);
            layout.Add(KeyCode.Y, KeyCode.Z);
            layout.Add(KeyCode.U, KeyCode.U);
            layout.Add(KeyCode.I, KeyCode.I);
            layout.Add(KeyCode.O, KeyCode.O);
            layout.Add(KeyCode.P, KeyCode.P);

            layout.Add(KeyCode.A, KeyCode.A);
            layout.Add(KeyCode.S, KeyCode.S);
            layout.Add(KeyCode.D, KeyCode.D);
            layout.Add(KeyCode.F, KeyCode.F);
            layout.Add(KeyCode.G, KeyCode.G);
            layout.Add(KeyCode.H, KeyCode.H);
            layout.Add(KeyCode.J, KeyCode.J);
            layout.Add(KeyCode.K, KeyCode.K);
            layout.Add(KeyCode.L, KeyCode.L);

            layout.Add(KeyCode.Z, KeyCode.Y);
            layout.Add(KeyCode.X, KeyCode.X);
            layout.Add(KeyCode.C, KeyCode.C);
            layout.Add(KeyCode.V, KeyCode.V);
            layout.Add(KeyCode.B, KeyCode.B);
            layout.Add(KeyCode.N, KeyCode.N);
            layout.Add(KeyCode.M, KeyCode.M);
            return layout;
        }

        static Dictionary<KeyCode, KeyCode> GetLayoutDvorak()
        {
            var layout = new Dictionary<KeyCode, KeyCode>();
            layout.Add(KeyCode.Q, KeyCode.Quote);
            layout.Add(KeyCode.W, KeyCode.Comma);
            layout.Add(KeyCode.E, KeyCode.Period);
            layout.Add(KeyCode.R, KeyCode.P);
            layout.Add(KeyCode.T, KeyCode.Y);
            layout.Add(KeyCode.Y, KeyCode.F);
            layout.Add(KeyCode.U, KeyCode.G);
            layout.Add(KeyCode.I, KeyCode.C);
            layout.Add(KeyCode.O, KeyCode.R);
            layout.Add(KeyCode.P, KeyCode.L);

            layout.Add(KeyCode.A, KeyCode.A);
            layout.Add(KeyCode.S, KeyCode.O);
            layout.Add(KeyCode.D, KeyCode.E);
            layout.Add(KeyCode.F, KeyCode.U);
            layout.Add(KeyCode.G, KeyCode.I);
            layout.Add(KeyCode.H, KeyCode.D);
            layout.Add(KeyCode.J, KeyCode.H);
            layout.Add(KeyCode.K, KeyCode.T);
            layout.Add(KeyCode.L, KeyCode.N);
            layout.Add(KeyCode.Semicolon, KeyCode.S);

            layout.Add(KeyCode.Z, KeyCode.Q);
            layout.Add(KeyCode.X, KeyCode.J);
            layout.Add(KeyCode.C, KeyCode.K);
            layout.Add(KeyCode.V, KeyCode.X);
            layout.Add(KeyCode.B, KeyCode.B);
            layout.Add(KeyCode.N, KeyCode.M);
            layout.Add(KeyCode.M, KeyCode.W);
            layout.Add(KeyCode.Comma, KeyCode.V);
            layout.Add(KeyCode.Period, KeyCode.Z);
            return layout;
        }

        static Dictionary<KeyCode, KeyCode> GetLayoutColemak()
        {
            var layout = new Dictionary<KeyCode, KeyCode>();
            layout.Add(KeyCode.Q, KeyCode.Q);
            layout.Add(KeyCode.W, KeyCode.W);
            layout.Add(KeyCode.E, KeyCode.F);
            layout.Add(KeyCode.R, KeyCode.P);
            layout.Add(KeyCode.T, KeyCode.G);
            layout.Add(KeyCode.Y, KeyCode.J);
            layout.Add(KeyCode.U, KeyCode.L);
            layout.Add(KeyCode.I, KeyCode.U);
            layout.Add(KeyCode.O, KeyCode.Y);
            layout.Add(KeyCode.P, KeyCode.Semicolon);

            layout.Add(KeyCode.A, KeyCode.A);
            layout.Add(KeyCode.S, KeyCode.R);
            layout.Add(KeyCode.D, KeyCode.S);
            layout.Add(KeyCode.F, KeyCode.T);
            layout.Add(KeyCode.G, KeyCode.D);
            layout.Add(KeyCode.H, KeyCode.H);
            layout.Add(KeyCode.J, KeyCode.N);
            layout.Add(KeyCode.K, KeyCode.E);
            layout.Add(KeyCode.L, KeyCode.I);
            layout.Add(KeyCode.Semicolon, KeyCode.O);

            layout.Add(KeyCode.Z, KeyCode.Z);
            layout.Add(KeyCode.X, KeyCode.X);
            layout.Add(KeyCode.C, KeyCode.C);
            layout.Add(KeyCode.V, KeyCode.V);
            layout.Add(KeyCode.B, KeyCode.B);
            layout.Add(KeyCode.N, KeyCode.K);
            layout.Add(KeyCode.M, KeyCode.M);
            return layout;
        }

        #endregion

        #region GAMEPADS

        [SerializeField, Tooltip("The different gamepad control layouts")]
        private GamepadProfile[] m_GamepadProfiles = { };

        const int k_GamepadIndexXbox360 = 0;
        const int k_GamepadIndexXboxOne = 1;
        const int k_GamepadIndexPS4 = 2;
        const int k_NumSupportedGamepads = 3;

        private GamepadUnityInputs[] m_GamepadUnityInputs = { };
        private int m_CurrentGamepadIndex = -1;

        public static event UnityAction<bool> onIsGamepadConnectedChanged;

        public static bool isGamepadConnected
        {
            get { return instance.m_CurrentGamepadIndex != -1; }
        }

        public static GamepadProfile[] gamepadProfiles
        {
            get { return instance.m_GamepadProfiles; }
        }

        public static GamepadUnityInputs connectedGamepad
        {
            get
            {
                if (instance.m_CurrentGamepadIndex == -1)
                    return null;
                else
                    return instance.m_GamepadUnityInputs[instance.m_CurrentGamepadIndex];
            }
        }

        private GamepadInput m_Gamepad = null;
        public static GamepadInput gamepad
        {
            get { return instance.m_Gamepad; }
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            foreach (var p in m_GamepadProfiles)
                p.OnValidate();
#endif
        }

        void InitialiseGamepads()
        {
            m_GamepadUnityInputs = new GamepadUnityInputs[k_NumSupportedGamepads];

            // Mappings based on information from: https://wiki.unity3d.com/index.php?title=Xbox360Controller
            // Annoyingly, axes and buttons aren't consistent across platforms so we need per-platform profiles
            // More will be added if requested (eg. consoles)

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            
            var xbox360 = new GamepadUnityInputs(
                "XBox 360", // Name
                "Gamepad Axis 1", "Gamepad Axis 2", // Left analogue
                "Gamepad Axis 3", "Gamepad Axis 4", // Right analogue
                "", "", // Gyro
                new string[]
                {
                    "Gamepad Button 16", //ButtonAorX
                    "Gamepad Button 17", //ButtonBorCircle
                    "Gamepad Button 18", //ButtonXorSquare
                    "Gamepad Button 19", //ButtonYorTriangle
                    "Gamepad Button 13", //LBumper
                    "Gamepad AxisBtn 5", //LTrigger
                    "Gamepad Button 14", //RBumper
                    "Gamepad AxisBtn 6", //RTrigger
                    "Gamepad Button 5", //DPad_Up
                    "Gamepad Button 6", //DPad_Down
                    "Gamepad Button 7", //DPad_Left
                    "Gamepad Button 8", //DPad_Right
                    "Gamepad Button 11", //AnaloguePressL
                    "Gamepad Button 12", //AnaloguePressR
                    "Gamepad Button 9", //Start
                    "Gamepad Button 10" //Select
                });
            
            var xboxOne = new GamepadUnityInputs(
                "XBox One", // Name
                "Gamepad Axis 1", "Gamepad Axis 2", // Left analogue
                "Gamepad Axis 3", "Gamepad Axis 4", // Right analogue
                "", "", // Gyro
                new string[]
                {
                    "Gamepad Button 16", //ButtonAorX
                    "Gamepad Button 17", //ButtonBorCircle
                    "Gamepad Button 18", //ButtonXorSquare
                    "Gamepad Button 19", //ButtonYorTriangle
                    "Gamepad Button 13", //LBumper
                    "Gamepad AxisBtn 5", //LTrigger
                    "Gamepad Button 14", //RBumper
                    "Gamepad AxisBtn 6", //RTrigger
                    "Gamepad Button 5", //DPad_Up
                    "Gamepad Button 6", //DPad_Down
                    "Gamepad Button 7", //DPad_Left
                    "Gamepad Button 8", //DPad_Right
                    "Gamepad Button 11", //AnaloguePressL
                    "Gamepad Button 12", //AnaloguePressR
                    "Gamepad Button 9", //Start
                    "Gamepad Button 10" //Select
                });
            
            var ps4 = new GamepadUnityInputs(
                "Playstation Dualshock 4", // Name
                "Gamepad Axis 1", "Gamepad AxisInv 2", // Left analogue
                "Gamepad Axis 3", "Gamepad AxisInv 6", // Right analogue
                "", "", // Gyro
                new string[]
                {
                    "Gamepad Button 1", //ButtonAorX
                    "Gamepad Button 2", //ButtonBorCircle
                    "Gamepad Button 0", //ButtonXorSquare
                    "Gamepad Button 3", //ButtonYorTriangle
                    "Gamepad Button 4", //LBumper
                    "Gamepad Button 6", //LTrigger
                    "Gamepad Button 5", //RBumper
                    "Gamepad Button 7", //RTrigger
                    "Gamepad AxisBtn 8", //DPad_Up
                    "Gamepad AxisBtnInv 8", //DPad_Down
                    "Gamepad AxisBtnInv 7", //DPad_Left
                    "Gamepad AxisBtn 7", //DPad_Right
                    "Gamepad Button 10", //AnaloguePressL
                    "Gamepad Button 11", //AnaloguePressR
                    "Gamepad Button 9", //Start
                    "Gamepad Button 13" //Select
                });

#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
            
            var xbox360 = new GamepadUnityInputs(
                "XBox 360", // Name
                "Gamepad Axis 1", "Gamepad Axis 2", // Left analogue
                "Gamepad Axis 4", "Gamepad Axis 5", // Right analogue
                "", "", // Gyro
                new string[]
                {
                    "Gamepad Button 0", //ButtonAorX
                    "Gamepad Button 1", //ButtonBorCircle
                    "Gamepad Button 2", //ButtonXorSquare
                    "Gamepad Button 3", //ButtonYorTriangle
                    "Gamepad Button 4", //LBumper
                    "Gamepad AxisBtn 3", //LTrigger
                    "Gamepad Button 5", //RBumper
                    "Gamepad AxisBtn 6", //RTrigger
                    "Gamepad Button 13", //DPad_Up
                    "Gamepad Button 14", //DPad_Down
                    "Gamepad Button 11", //DPad_Left
                    "Gamepad Button 12", //DPad_Right
                    "Gamepad Button 9", //AnaloguePressL
                    "Gamepad Button 10", //AnaloguePressR
                    "Gamepad Button 7", //Start
                    "Gamepad Button 6" //Select
                });
            
            var xboxOne = new GamepadUnityInputs(
                "XBox One", // Name
                "Gamepad Axis 1", "Gamepad Axis 2", // Left analogue
                "Gamepad Axis 4", "Gamepad Axis 5", // Right analogue
                "", "", // Gyro
                new string[]
                {
                    "Gamepad Button 0", //ButtonAorX
                    "Gamepad Button 1", //ButtonBorCircle
                    "Gamepad Button 2", //ButtonXorSquare
                    "Gamepad Button 3", //ButtonYorTriangle
                    "Gamepad Button 4", //LBumper
                    "Gamepad AxisBtn 3", //LTrigger
                    "Gamepad Button 5", //RBumper
                    "Gamepad AxisBtn 6", //RTrigger
                    "Gamepad Button 13", //DPad_Up
                    "Gamepad Button 14", //DPad_Down
                    "Gamepad Button 11", //DPad_Left
                    "Gamepad Button 12", //DPad_Right
                    "Gamepad Button 9", //AnaloguePressL
                    "Gamepad Button 10", //AnaloguePressR
                    "Gamepad Button 7", //Start
                    "Gamepad Button 6" //Select
                });
            
            var ps4 = new GamepadUnityInputs(
                "Playstation Dualshock 4", // Name
                "Gamepad Axis 1", "Gamepad AxisInv 2", // Left analogue
                "Gamepad Axis 3", "Gamepad AxisInv 6", // Right analogue
                "", "", // Gyro
                new string[]
                {
                    "Gamepad Button 1", //ButtonAorX
                    "Gamepad Button 2", //ButtonBorCircle
                    "Gamepad Button 0", //ButtonXorSquare
                    "Gamepad Button 3", //ButtonYorTriangle
                    "Gamepad Button 4", //LBumper
                    "Gamepad Button 6", //LTrigger
                    "Gamepad Button 5", //RBumper
                    "Gamepad Button 7", //RTrigger
                    "Gamepad AxisBtn 8", //DPad_Up
                    "Gamepad AxisBtnInv 8", //DPad_Down
                    "Gamepad AxisBtnInv 7", //DPad_Left
                    "Gamepad AxisBtn 7", //DPad_Right
                    "Gamepad Button 10", //AnaloguePressL
                    "Gamepad Button 11", //AnaloguePressR
                    "Gamepad Button 9", //Start
                    "Gamepad Button 13" //Select
                });

#else // WINDOWS (default)

            var xbox360 = new GamepadUnityInputs(
                "XBox 360", // Name
                "Gamepad Axis 1", "Gamepad AxisInv 2", // Left analogue
                "Gamepad Axis 4", "Gamepad AxisInv 5", // Right analogue
                "", "", // Gyro
                new string[]
                {
                    "Gamepad Button 0", //ButtonAorCross
                    "Gamepad Button 1", //ButtonBorCircle
                    "Gamepad Button 2", //ButtonXorSquare
                    "Gamepad Button 3", //ButtonYorTriangle
                    "Gamepad Button 4", //LBumper
                    "Gamepad AxisBtn 9", //LTrigger
                    "Gamepad Button 5", //RBumper
                    "Gamepad AxisBtn 10", //RTrigger
                    "Gamepad AxisBtn 7", //DPad_Up
                    "Gamepad AxisBtnInv 7", //DPad_Down
                    "Gamepad AxisBtnInv 6", //DPad_Left
                    "Gamepad AxisBtn 6", //DPad_Right
                    "Gamepad Button 8", //AnaloguePressL
                    "Gamepad Button 9", //AnaloguePressR
                    "Gamepad Button 7", //Start
                    "Gamepad Button 6" //Select
                });

            var xboxOne = new GamepadUnityInputs(
                "XBox One", // Name
                "Gamepad Axis 1", "Gamepad AxisInv 2", // Left analogue
                "Gamepad Axis 4", "Gamepad AxisInv 5", // Right analogue
                "", "", // Gyro
                new string[]
                {
                    "Gamepad Button 0", //ButtonAorCross
                    "Gamepad Button 1", //ButtonBorCircle
                    "Gamepad Button 2", //ButtonXorSquare
                    "Gamepad Button 3", //ButtonYorTriangle
                    "Gamepad Button 4", //LBumper
                    "Gamepad AxisBtn 9", //LTrigger
                    "Gamepad Button 5", //RBumper
                    "Gamepad AxisBtn 10", //RTrigger
                    "Gamepad AxisBtn 7", //DPad_Up
                    "Gamepad AxisBtnInv 7", //DPad_Down
                    "Gamepad AxisBtnInv 6", //DPad_Left
                    "Gamepad AxisBtn 6", //DPad_Right
                    "Gamepad Button 8", //AnaloguePressL
                    "Gamepad Button 9", //AnaloguePressR
                    "Gamepad Button 7", //Start
                    "Gamepad Button 6" //Select
                });

            var ps4 = new GamepadUnityInputs(
                "Playstation Dualshock 4", // Name
                "Gamepad Axis 1", "Gamepad AxisInv 2", // Left analogue
                "Gamepad Axis 3", "Gamepad AxisInv 6", // Right analogue
                "", "", // Gyro
                new string[]
                {
                    "Gamepad Button 1", //ButtonAorCross
                    "Gamepad Button 2", //ButtonBorCircle
                    "Gamepad Button 0", //ButtonXorSquare
                    "Gamepad Button 3", //ButtonYorTriangle
                    "Gamepad Button 4", //LBumper
                    "Gamepad Button 6", //LTrigger
                    "Gamepad Button 5", //RBumper
                    "Gamepad Button 7", //RTrigger
                    "Gamepad AxisBtn 8", //DPad_Up
                    "Gamepad AxisBtnInv 8", //DPad_Down
                    "Gamepad AxisBtnInv 7", //DPad_Left
                    "Gamepad AxisBtn 7", //DPad_Right
                    "Gamepad Button 10", //AnaloguePressL
                    "Gamepad Button 11", //AnaloguePressR
                    "Gamepad Button 9", //Start
                    "Gamepad Button 13" //Select
                });
#endif

            // Apply the profiles
            m_GamepadUnityInputs[k_GamepadIndexXbox360] = xbox360;
            m_GamepadUnityInputs[k_GamepadIndexXboxOne] = xboxOne;
            m_GamepadUnityInputs[k_GamepadIndexPS4] = ps4;
        }

        void CheckGamepads()
        {
            int oldIndex = m_CurrentGamepadIndex;
            m_CurrentGamepadIndex = -1;

            var joystickNames = Input.GetJoystickNames();
            for (int i = 0; i < joystickNames.Length; ++i)
            {
                if (string.IsNullOrEmpty(joystickNames[i]))
                    continue;

                string lower = joystickNames[i].ToLower();

                // Check if PS4 controller
                if (joystickNames[i] == "Wireless Controller" ||
                    joystickNames[i] == "Sony Interactive Entertainment Wireless Controller" ||
                    joystickNames[i] == "Sony Computer Entertainment Wireless Controller" ||
                    joystickNames[i] == "Sony Interactive Entertainment DUALSHOCK®4 USB Wireless Adaptor")
                {
                    m_CurrentGamepadIndex = k_GamepadIndexPS4;
                    break;
                }

                // Check if XBox controller
                if (lower.Contains("xbox"))
                {
                    if (lower.Contains("xbox one"))
                        m_CurrentGamepadIndex = k_GamepadIndexXboxOne;
                    else
                        m_CurrentGamepadIndex = k_GamepadIndexXbox360;
                    break;
                }

                // Check if XBox Series controller (wireless)
                if (joystickNames[i] == "Ű" ||
                    joystickNames[i] == "耀")
                {
                    m_CurrentGamepadIndex = k_GamepadIndexXboxOne;
                    break;
                }
            }

            // Fire changed event if relevant
            if (m_CurrentGamepadIndex != oldIndex && onIsGamepadConnectedChanged != null)
                onIsGamepadConnectedChanged(m_CurrentGamepadIndex != -1);
        }

        public class GamepadInput : MonoBehaviour
        {
            private ButtonState[] m_States = null;
            private bool m_UseGamepad = false;
            private int m_ProfileIndex = 0;

            private enum ButtonState
            {
                Up,
                Pressed,
                Down,
                Released
            }
            
            private void Start()
            {
                // Allocate button states array
                m_States = new ButtonState[FpsInputButton.count];
                // Add controller disconnect handler (reset button states)
                onIsGamepadConnectedChanged += OnIsGamepadConnectedChanged;
                // Track settings use gamepad property
                FpsSettings.gamepad.onSettingsChanged += OnGamepadSettingsChanged;
                OnGamepadSettingsChanged();
            }

            void Update()
            {
                if (!isGamepadConnected || !m_UseGamepad)
                    return;

                // Track axes for current profile
                GamepadProfile p = gamepadProfiles[m_ProfileIndex];
                GamepadUnityInputs g = connectedGamepad;

                for (int i = 0; i < (int)GamepadButton.Count; ++i)
                {
                    var gamepadButton = (GamepadButton)i;

                    // Check if button is down
                    bool down = g.GetButton(gamepadButton);
                    var buttons = p.GetInputButtonsForGamepadButton(gamepadButton);

                    for (int j = 0; j < buttons.Length; ++j)
                    {
                        var button = buttons[j];
#if UNITY_EDITOR
                        // Error check
                        if (button >= m_States.Length)
                        {
                            Debug.Log("Button out of range: " + (GamepadButton)i);
                            return;
                        }
#endif

                        // Update state
                        switch (m_States[button])
                        {
                            case ButtonState.Up:
                                {
                                    if (down)
                                        m_States[button] = ButtonState.Pressed;
                                    break;
                                }
                            case ButtonState.Pressed:
                                {
                                    if (down)
                                        m_States[button] = ButtonState.Down;
                                    else
                                        m_States[button] = ButtonState.Released;
                                    break;
                                }
                            case ButtonState.Down:
                                {
                                    if (!down)
                                        m_States[button] = ButtonState.Released;
                                    break;
                                }
                            case ButtonState.Released:
                                {
                                    if (!down)
                                        m_States[button] = ButtonState.Up;
                                    else
                                        m_States[button] = ButtonState.Pressed;
                                    break;
                                }
                        }
                    }
                }
            }

            public float GetAxis(FpsInputAxis axis)
            {
                if (!isGamepadConnected || !m_UseGamepad)
                    return 0f;

                GamepadProfile p = gamepadProfiles[m_ProfileIndex];
                GamepadUnityInputs g = connectedGamepad;

                if (p.analogueSetup == GamepadProfile.AnalogueSetup.LeftMoveRightLook)
                {
                    switch (axis)
                    {
                        case FpsInputAxis.MoveX:
                            return g.GetLeftAnalogH();
                        case FpsInputAxis.MoveY:
                            return g.GetLeftAnalogV();
                        case FpsInputAxis.LookX:
                            return g.GetRightAnalogH();
                        case FpsInputAxis.LookY:
                            return g.GetRightAnalogV();
                    }
                }
                else
                {
                    switch (axis)
                    {
                        case FpsInputAxis.MoveX:
                            return g.GetRightAnalogH();
                        case FpsInputAxis.MoveY:
                            return g.GetRightAnalogV();
                        case FpsInputAxis.LookX:
                            return g.GetLeftAnalogH();
                        case FpsInputAxis.LookY:
                            return g.GetLeftAnalogV();
                    }
                }

                return 0f;
            }

            public float GetAxisRaw(FpsInputAxis axis)
            {
                if (!isGamepadConnected || !m_UseGamepad)
                    return 0f;

                GamepadProfile p = gamepadProfiles[m_ProfileIndex];
                GamepadUnityInputs g = connectedGamepad;

                if (p.analogueSetup == GamepadProfile.AnalogueSetup.LeftMoveRightLook)
                {
                    switch (axis)
                    {
                        case FpsInputAxis.MoveX:
                            return g.GetLeftAnalogRawH();
                        case FpsInputAxis.MoveY:
                            return g.GetLeftAnalogRawV();
                        case FpsInputAxis.LookX:
                            return g.GetRightAnalogRawH();
                        case FpsInputAxis.LookY:
                            return g.GetRightAnalogRawV();
                    }
                }
                else
                {
                    switch (axis)
                    {
                        case FpsInputAxis.MoveX:
                            return g.GetRightAnalogRawH();
                        case FpsInputAxis.MoveY:
                            return g.GetRightAnalogRawV();
                        case FpsInputAxis.LookX:
                            return g.GetLeftAnalogRawH();
                        case FpsInputAxis.LookY:
                            return g.GetLeftAnalogRawV();
                    }
                }

                return 0f;
            }

            public bool GetButton(FpsInputButton button)
            {
                if (!m_UseGamepad || !isGamepadConnected)
                    return false;
                return m_States[button] == ButtonState.Down || m_States[button] == ButtonState.Pressed;
            }

            public bool GetButtonDown(FpsInputButton button)
            {
                if (!m_UseGamepad || !isGamepadConnected)
                    return false;
                return m_States[button] == ButtonState.Pressed;
            }

            public bool GetButtonUp(FpsInputButton button)
            {
                if (!m_UseGamepad || !isGamepadConnected)
                    return false;
                return m_States[button] == ButtonState.Released;
            }

            void ResetButtonStates()
            {
                for (int i = 0; i < m_States.Length; ++i)
                    m_States[i] = ButtonState.Up;
            }

            void OnIsGamepadConnectedChanged(bool connected)
            {
                ResetButtonStates();
            }

            void OnGamepadSettingsChanged()
            {
                bool reset = false;
                // Check if use gamepad has changed
                var useGamepad = FpsSettings.gamepad.useGamepad;
                if (useGamepad != m_UseGamepad)
                {
                    m_UseGamepad = useGamepad;
                    reset = true;
                }
                // Check if gamepad profile index has changed
                var profile = FpsSettings.gamepad.profile;
                if (profile != m_ProfileIndex)
                {
                    m_ProfileIndex = profile;
                    reset = true;
                }
                // Reset button states if required
                if (reset)
                    ResetButtonStates();
            }
        }

        #endregion

        #region ESCAPE HANDLERS

#if UNITY_WEBGL
        private const KeyCode k_MenuKey = KeyCode.Tab;
#else
        private const KeyCode k_MenuKey = KeyCode.Escape;
#endif

        private static List<UnityAction> s_EscapeHandlers = new List<UnityAction>(4);

        public static void PushEscapeHandler(UnityAction handler)
        {
            // Check if earlier in the handler stack and move to top if so
            for (int i = 0; i < s_EscapeHandlers.Count - 1; ++i)
            {
                if (s_EscapeHandlers[i] == handler)
                {
                    s_EscapeHandlers.RemoveAt(i);
                    s_EscapeHandlers.Add(handler);
                    return;
                }
            }

            // Add as top handler if empty or not already top
            if (s_EscapeHandlers.Count == 0 || s_EscapeHandlers[s_EscapeHandlers.Count - 1] != handler)
                s_EscapeHandlers.Add(handler);
        }

        public static void PopEscapeHandler(UnityAction handler)
        {
            for (int i = s_EscapeHandlers.Count - 1; i >= 0; --i)
            {
                if (s_EscapeHandlers[i] == handler)
                    s_EscapeHandlers.RemoveAt(i);
            }
        }

        #endregion

        #region PROXY BEHAVIOUR

        class RuntimeBehaviour : MonoBehaviour
        {
            void Update()
            {
#if AGGRESIVE_CURSOR_LOCK
                if (captureMouseCursor)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
#endif

                // Check for menu button
                if (Input.GetKeyDown(k_MenuKey) || gamepad.GetButtonDown(FpsInputButton.Menu))
                {
                    if (s_EscapeHandlers.Count > 0)
                    {
                        if (s_EscapeHandlers[s_EscapeHandlers.Count - 1] != null)
                            s_EscapeHandlers[s_EscapeHandlers.Count - 1].Invoke();
                    }
                    else
                        captureMouseCursor = !captureMouseCursor;
                }

                // Check for gamepad connect/disconnect
                instance.CheckGamepads();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (s_ProxyBehaviour != null)
            {
                Destroy(s_ProxyBehaviour);
                s_ProxyBehaviour = null;
            }
        }

        #endregion

        #region CODE GENERATION (BUTTONS, AXES, CONTEXTS)

#if UNITY_EDITOR
#pragma warning disable 0414

        // Script templates
        [SerializeField] private UnityEngine.Object m_ScriptFolder = null;
        [SerializeField] private TextAsset m_ScriptTemplate = null;

        // Editor foldouts
        [SerializeField] private bool m_ExpandInputButtons = false;
        [SerializeField] private bool m_ExpandInputAxes = false;
        [SerializeField] private bool m_ExpandInputContexts = false;

        // Copies of the button setup to revert to. Copied from m_InputButtons when the FpsInputButton constants are generated
        [SerializeField] private InputButtonInfo[] m_Revert = { };
        [SerializeField] private InputButtonInfo[] m_Snapshot = { };

        // Dirty flags for buttons (requires code generation) and button props (display name, default keys, etc)
        [SerializeField] private bool m_ButtonsRequireRebuild = true;
        [SerializeField] private bool m_InputButtonsDirty = true;
        [SerializeField] private int m_InputButtonsError = 0;

        // Input axes
        [SerializeField] private GeneratorConstantsEntry[] m_InputAxisInfo = null;
        [SerializeField] private bool m_InputAxisDirty = false;
        [SerializeField] private int m_InputAxisError = 1;

        // Input contexts
        [SerializeField] private GeneratorConstantsEntry[] m_InputContextInfo = null;
        [SerializeField] private bool m_InputContextDirty = false;
        [SerializeField] private int m_InputContextError = 1;

        public bool showWarning
        {
            get { return m_InputButtonsError == 1; }
        }

        public bool showError
        {
            get { return m_InputButtonsError > 1; }
        }

#pragma warning restore 0414
#endif

        #endregion
    }
}
