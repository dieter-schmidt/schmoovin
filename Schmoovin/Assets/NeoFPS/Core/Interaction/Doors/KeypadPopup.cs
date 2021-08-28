using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using NeoFPS.Samples;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-keypadpopup.html")]
    [RequireComponent(typeof(AudioSource))]
    public class KeypadPopup : MonoBehaviour, IPrefabPopup
    {
        [SerializeField, Tooltip("The initial UI element to select when the popup is shown.")]
        private Selectable m_StartingSelection = null;

        [Header("Readout")]
        [SerializeField, Tooltip("Output text when typing code.")]
        private Text m_ReadoutText = null;
        [SerializeField, Tooltip("The character to use for missing digits in the code.")]
        private MissingCharacter m_MissingCharacters = MissingCharacter.Blank;

        [Header("Buttons")]
        [SerializeField, Tooltip("The number buttons of the keypad in numberic order (eg 0,1,2,3,4...).")]
        private Button[] m_DigitButtons = { };
        [SerializeField, Tooltip("The lowest numbered button.")]
        private int m_StartingDigit = 0;
        [SerializeField, Tooltip("The delete button is used to delete the last input digit.")]
        private Button m_DeleteButton = null;
        [SerializeField, Tooltip("The clear button clears all typed digits.")]
        private Button m_ClearButton = null;
        [SerializeField, Tooltip("The audio clip to play for each button press.")]
        private AudioClip[] m_ButtonAudio = null;

        [Header("Completion")]
        [SerializeField, Tooltip("The time after the last digit is input before the popup closes.")]
        private float m_CompletionDelay = 1f;
        [SerializeField, Tooltip("The audio clip to play if the correct code is input.")]
        private AudioClip m_PassAudio = null;
        [SerializeField, Tooltip("The audio clip to play if an incorrect code is input.")]
        private AudioClip m_FailAudio = null;
        [SerializeField, Tooltip("Should the popup close after the wrong code is input or allow the player to enter another.")]
        private bool m_CloseOnFail = true;

        [Header("Discovered")]

        [SerializeField, Tooltip("If the keycode is known it will be shown here.")]
        private Text m_DiscoveredText = null;
        [SerializeField, Tooltip("If the keycode is known then this object will be activated.")]
        private GameObject m_DiscoveredObject = null;

        private AudioSource m_AudioSource = null;
        private int[] m_PassCode = null;
        private List<int> m_CurrentEntry = new List<int>();
        private UnityAction m_OnFail = null;
        private UnityAction m_OnUnlock = null;
        private bool m_Pending = false;

        enum MissingCharacter
        {
            Blank,
            Dash,
            Underscore,
            Asterisk
        }

        public Selectable startingSelection
        {
            get { return m_StartingSelection; }
        }

        public BaseMenu menu
        {
            get;
            private set;
        }

        public void OnShow(BaseMenu m)
        {
            menu = m;
        }

        private void OnValidate()
        {
            // Check maximum buttons length
            if (m_DigitButtons.Length > 10)
            {
                var temp = new Button[10];
                for (int i = 0; i < 10; ++i)
                    temp[i] = m_DigitButtons[i];
                m_DigitButtons = temp;
            }

            // Check starting digit
            m_StartingDigit = Mathf.Clamp(m_StartingDigit, 0, 10 - m_DigitButtons.Length);

            // Check discovered object
            if (m_DiscoveredObject == null && m_DiscoveredText != null)
                m_DiscoveredObject = m_DiscoveredText.gameObject;

            // Check delay
            if (m_CompletionDelay < 0f)
                m_CompletionDelay = 0f;
        }

        public class HealthEventArgs : EventArgs
        {
            public int health;

            public HealthEventArgs(int health)
            {
                this.health = health;
            }
        }

        private void Start()
        {
            // Add event listeners to buttons
            for (int i = 0; i < m_DigitButtons.Length; ++i)
            {
                int buttonNum = m_StartingDigit + i;
                m_DigitButtons[i].onClick.AddListener(() => { AddDigit(buttonNum); });
            }
            if (m_DeleteButton != null)
                m_DeleteButton.onClick.AddListener(Delete);
            if (m_ClearButton != null)
                m_ClearButton.onClick.AddListener(Clear);
        }

        public void Initialise(int[] passCode, UnityAction onUnlock, UnityAction onFail, bool known = false)
        {
            m_AudioSource = GetComponent<AudioSource>();

            m_PassCode = passCode;

            // Check minimum passcode length
            if (m_PassCode == null || m_PassCode.Length == 0)
                m_PassCode = new int[] { 0 };

            // Check maximum passcode length
            if (m_PassCode.Length > 10)
            {
                var temp = new int[10];
                for (int i = 0; i < 10; ++i)
                    temp[i] = m_PassCode[i];
                m_PassCode = temp;
            }

            // Check passcode digits
            if (m_DigitButtons.Length > 0)
            {
                for (int i = 0; i < m_PassCode.Length; ++i)
                    m_PassCode[i] = Mathf.Clamp(m_PassCode[i], 0, m_DigitButtons.Length - 1);
            }

            // Set callbacks
            m_OnFail = onFail;
            m_OnUnlock = onUnlock;

            // Show code if known
            if (m_DiscoveredObject != null)
            {
                if (known && m_DiscoveredText != null)
                {
                    m_DiscoveredObject.SetActive(true);

                    string readout = string.Empty;

                    // Print existing digits
                    for (int i = 0; i < m_PassCode.Length; ++i)
                        readout += m_PassCode[i];

                    // Apply to readout
                    m_DiscoveredText.text = readout;
                }
                else
                    m_DiscoveredObject.SetActive(false);
            }

            m_CurrentEntry.Clear();
            OnPasscodeChanged();
        }

        void AddDigit(int digit)
        {
            if (!m_Pending && m_CurrentEntry.Count < m_PassCode.Length)
            {
                m_CurrentEntry.Add(digit);
                OnPasscodeChanged();
            }
            PlayButtonSound();
        }

        void Delete()
        {
            if (!m_Pending && m_CurrentEntry.Count > 0)
            {
                m_CurrentEntry.RemoveAt(m_CurrentEntry.Count - 1);
                OnPasscodeChanged();
            }
            PlayButtonSound();
        }

        void Clear()
        {
            if (!m_Pending)
            {
                m_CurrentEntry.Clear();
                OnPasscodeChanged();
            }
            PlayButtonSound();
        }

        void PlayButtonSound()
        {
            if (m_ButtonAudio.Length > 0)
            {
                int i = UnityEngine.Random.Range(0, m_ButtonAudio.Length);
                if (m_ButtonAudio[i] != null)
                    m_AudioSource.PlayOneShot(m_ButtonAudio[i]);
            }
        }

        protected virtual void OnPasscodeChanged()
        {
            // Rebuild string
            if (m_ReadoutText != null)
            {
                string readout = string.Empty;

                // Print existing digits
                int counter = 0;
                for (; counter < m_CurrentEntry.Count; ++counter)
                    readout += m_CurrentEntry[counter];

                // Print missing digits
                switch (m_MissingCharacters)
                {
                    case MissingCharacter.Dash:
                        for (; counter < m_PassCode.Length; ++counter)
                            readout += '-';
                        break;
                    case MissingCharacter.Underscore:
                        for (; counter < m_PassCode.Length; ++counter)
                            readout += '_';
                        break;
                    case MissingCharacter.Asterisk:
                        for (; counter < m_PassCode.Length; ++counter)
                            readout += '*';
                        break;
                }

                // Apply to readout
                m_ReadoutText.text = readout;
            }

            // Check passcode when correct length is reached
            if (m_CurrentEntry.Count == m_PassCode.Length)
            {
                // Check digits
                bool correct = true;
                for (int i = 0; i < m_PassCode.Length; ++i)
                {
                    if (m_CurrentEntry[i] != m_PassCode[i])
                    {
                        correct = false;
                        break;
                    }
                }

                if (correct)
                {
                    // Play pass audio
                    if (m_PassAudio != null)
                        AudioSource.PlayClipAtPoint(m_PassAudio, transform.position);

                    // Signal completion
                    if (m_CompletionDelay > 0f)
                        Invoke("PasswordCorrect", m_CompletionDelay);
                    else
                        PasswordCorrect();
                }
                else
                {
                    // Play fail audio
                    if (m_FailAudio != null)
                        AudioSource.PlayClipAtPoint(m_FailAudio, transform.position);

                    // Signal completion
                    if (m_CompletionDelay > 0f)
                        Invoke("PasswordIncorrect", m_CompletionDelay);
                    else
                        PasswordIncorrect();
                }
            }
        }

        void PasswordCorrect()
        {
            // Invoke callback
            if (m_OnUnlock != null)
                m_OnUnlock();

            // Hide
            menu.ShowPopup(null);
        }

        void PasswordIncorrect()
        {
            // Invoke callback
            if (m_OnFail != null)
                m_OnFail();

            // Hide or reset
            if (m_CloseOnFail)
                menu.ShowPopup(null);
            else
            {
                m_Pending = false;
                Clear();
            }
        }

        public void Back()
        {
            if (m_OnFail != null)
                m_OnFail();

            // Hide
            menu.ShowPopup(null);
        }
    }
}