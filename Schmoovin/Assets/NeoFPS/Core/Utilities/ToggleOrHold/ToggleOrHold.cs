using System;
using UnityEngine;
using NeoSaveGames.Serialization;

namespace NeoFPS
{
	/// <summary>
	/// A simple class for tracking on/off inputs in either toggle mode or hold down.
	/// Also allows for simple reference counted blockers that can interrupt and toggle the output, though care must be taken to make sure each blocker is removed when appropriate.
	/// For a safer but less optimal implementation that allows tracking of specific blockers, use the SafeToggleOrHold base class.
	/// </summary>  `
    [Serializable]
	public class ToggleOrHold
	{
        protected ToggleOrHold(Func<bool> isBlockedCallback)
        {
            this.isBlockedCallback = isBlockedCallback;
        }

        public bool isBlocked
        {
            get { return isBlockedCallback != null && isBlockedCallback(); }
        }

        private bool m_Combined = false;
        private bool m_Hold = false;
        private bool m_On = false;
		public bool on
		{
			get { return !isBlocked && (m_On || m_Hold); }
			set
			{
				// Check for difference
				if (m_On != value)
                {
                    bool previous = on;

                    // Set value
                    m_On = value;

                    // Handle change
                    bool result = on;
                    if (!isBlocked && result != previous)
                    {
                        if (result)
                            OnActivate();
                        else
                            OnDeactivate();
                    }
                }
			}
		}

        Func<bool> isBlockedCallback = null;

        public void Toggle()
        {
            m_On = !m_On;

            bool result = on;
            if (!isBlocked && result != m_Combined)
            {
                if (result)
                    OnActivate();
                else
                    OnDeactivate();
            }

            m_Combined = result;
        }

        public void Hold(bool hold = true)
        {
            m_Hold = hold;

            bool result = on;
            if (!isBlocked && result != m_Combined)
            {
                if (result)
                    OnActivate();
                else
                    OnDeactivate();
            }

            m_Combined = result;
        }

        public void SetInput(bool toggle, bool hold)
        {
            if (isBlocked)
            {
                m_Hold = false;

                if (m_Combined)
                {
                    OnDeactivate();
                    m_Combined = false;
                }
            }
            else
            {
                if (toggle)
                    m_On = !m_On;
                m_Hold = hold;

                bool result = m_Hold || m_On;
                if (result != m_Combined)
                {
                    if (result)
                        OnActivate();
                    else
                        OnDeactivate();
                }

                m_Combined = result;
            }
        }

        protected virtual void OnActivate () {}
		protected virtual void OnDeactivate () {}

        #region INeoSerializableObject IMPLEMENTATION

        private static readonly NeoSerializationKey k_OnKey = new NeoSerializationKey("on");
        private static readonly NeoSerializationKey k_Combined = new NeoSerializationKey("combined");

        public void WriteProperties(INeoSerializer writer, string key, bool writeBlockers)
        {
            WriteProperties(writer, Animator.StringToHash(key), writeBlockers);
        }

        public void ReadProperties(INeoDeserializer reader, string key)
        {
            ReadProperties(reader, Animator.StringToHash(key));
        }

        public void WriteProperties(INeoSerializer writer, int hash, bool writeBlockers)
        {
            writer.PushContext(SerializationContext.ObjectNeoSerialized, hash);

            writer.WriteValue(k_OnKey, m_On);
            writer.WriteValue(k_Combined, m_Combined);

            writer.PopContext(SerializationContext.ObjectNeoSerialized);
        }

        public void ReadProperties(INeoDeserializer reader, int hash)
        {
            if (reader.PushContext(SerializationContext.ObjectNeoSerialized, hash))
            {
                reader.TryReadValue(k_OnKey, out m_On, m_On);
                reader.TryReadValue(k_Combined, out m_Combined, m_Combined);

                reader.PopContext(SerializationContext.ObjectNeoSerialized, hash);

                if (m_Combined)
                    OnActivate();
                else
                    OnDeactivate();
            }
        }

        #endregion
    }
}