using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.Samples.SinglePlayer
{
	[HelpURL("https://docs.neofps.com/manual/samplesref-mb-demofacilitytargetdamagetracker.html")]
	public class DemoFacilityTargetDamageTracker : MonoBehaviour, INeoSerializableComponent
	{
        [SerializeField, Tooltip("The text readout for target damage.")]
        private Text m_Readout = null;
        
		[SerializeField, Range (1, 5), Tooltip("The number of targets to track.")]
		private int m_TargetCount = 1;

		private float[] m_DamageValues = null;
        private StringBuilder m_StringBuilder = null;
        private bool m_Initialised = false;

        void OnValidate ()
		{
			if (m_Readout == null)
				m_Readout = GetComponentInChildren<Text> ();
		}

		void Start ()
        {
            Initialse();
        }

        void Initialse()
		{
            if (!m_Initialised)
            {
                m_StringBuilder = new StringBuilder();
                m_DamageValues = new float[m_TargetCount];
                Reset();

                m_Initialised = true;
            }
		}

		public void Reset ()
		{
			for (int i = 0; i < m_TargetCount; ++i)
				m_DamageValues [i] = 0;
			BuildString ();
		}

		public void AddDamage (int index, float damage)
		{
			int i = Mathf.Clamp (index, 0, m_TargetCount);
			m_DamageValues [i] = Mathf.Clamp (m_DamageValues[i] + damage, 0, 99999);
			BuildString ();
		}

		void BuildString ()
		{
			if (m_Readout != null)
			{
				m_StringBuilder.Length = 0;
				for (int i = 0; i < m_TargetCount; ++i)
				{
					if (i > 0)
						m_StringBuilder.AppendLine ();
					m_StringBuilder.Append ("Target ");
					m_StringBuilder.Append (i + 1);
					m_StringBuilder.Append (": ");
					m_StringBuilder.Append (((int)(m_DamageValues [i] + 0.5f)).ToString ("D5"));
				}

				m_Readout.text = m_StringBuilder.ToString ();
			}
		}

        private static readonly NeoSerializationKey k_DamageKey = new NeoSerializationKey("damage");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValues(k_DamageKey, m_DamageValues);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            if (reader.TryReadValues(k_DamageKey, out m_DamageValues, m_DamageValues))
            {
                Initialse();
                BuildString();
            }
        }
    }
}