using System;
using UnityEngine;

namespace NeoFPS
{
    [Serializable]
    public struct InputButtonInfo
    {
        [SerializeField, Tooltip("The name of the button as used in code (must start with a capital letter and only use valid characters - no spaces).")]
        private string m_Name;
        [SerializeField, Tooltip("The primary key or button.")]
        private KeyCode m_DefaultPrimary;
        [SerializeField, Tooltip("The secondary (alternative) key or button.")]
        private KeyCode m_DefaultSecondary;
        [SerializeField, Tooltip("The button context. Used to specify which inputs can share keybindings.")]
        private KeyBindingContext m_Context;
        [SerializeField, Tooltip("The name to show in the key rebinding settings in-game.")]
        private string m_DisplayName;
        [SerializeField, Tooltip("The category to place the input under in the key rebinding settings in-game.")]
        private InputCategory m_Category;

#if UNITY_EDITOR
#pragma warning disable 0414

        [SerializeField] private bool m_NameInvalidError;
        [SerializeField] private bool m_NameDuplicateError;
        [SerializeField] private bool m_NameReservedError;
        [SerializeField] private bool m_DisplayNameInvalidError;
        [SerializeField] private bool m_DisplayNameDuplicateError;

#pragma warning restore 0414
#endif

        public string name
        {
            get { return m_Name; }
        }

        public string displayName
        {
            get { return m_DisplayName; }
        }

        public InputCategory category
        {
            get { return m_Category; }
        }

        public KeyBindingContext context
        {
            get { return m_Context; }
        }

        public KeyCode defaultPrimary
        {
            get { return m_DefaultPrimary; }
        }

        public KeyCode defaultSecondary
        {
            get { return m_DefaultSecondary; }
        }

        public InputButtonInfo(string n, string d, KeyCode k1, KeyCode k2, InputCategory c, KeyBindingContext keyContext)
        {
            m_Name = n;
            m_DisplayName = d;
            m_Category = c;
            m_DefaultPrimary = k1;
            m_DefaultSecondary = k2;
            m_Context = keyContext;

#if UNITY_EDITOR
            m_NameInvalidError = false;
            m_NameDuplicateError = false;
            m_NameReservedError = false;
            m_DisplayNameInvalidError = false;
            m_DisplayNameDuplicateError = false;
#endif
        }
    }
}