using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-demobuttonpresser.html")]
	public class DemoButtonPresser : MonoBehaviour, INeoSerializableComponent
	{
        [SerializeField, Tooltip("The button position offset when pressed.")]
        private Vector3 m_PressOffset = Vector3.zero;

        [SerializeField, Range(0f, 1f), Tooltip("The duration to hold the button down.")]
		private float m_HoldDuration = 0.25f;

        [SerializeField, Range(0f, 1f), Tooltip("The duration to spring back to the original position.")]
		private float m_SpringDuration = 0.25f;

        private static readonly NeoSerializationKey k_TimerKey = new NeoSerializationKey("timer");

        Vector3 m_StartPosition = Vector3.zero;
		Coroutine m_PressCoroutine = null;
        private float m_Timer = 0f;

        void Start ()
		{
			m_StartPosition = transform.localPosition;
		}

		public void Press ()
		{
			if (m_PressCoroutine != null)
				StopCoroutine (m_PressCoroutine);
			m_PressCoroutine = StartCoroutine (PressCoroutine(0f));
		}

		IEnumerator PressCoroutine(float timer)
		{
            m_Timer = timer;
            Transform t = transform;
			Vector3 downPosition = m_StartPosition + m_PressOffset;

			t.localPosition = downPosition;

            while (m_Timer < m_HoldDuration)
            {
                yield return null;
                m_Timer += Time.deltaTime;
            }

            float inverseSpringDuration = 1f / m_SpringDuration;
			while (m_Timer < m_HoldDuration + m_SpringDuration)
			{
				yield return null;
                m_Timer += Time.deltaTime;
                float lerp = (m_Timer - m_HoldDuration) * inverseSpringDuration;
				t.localPosition = Vector3.Lerp (downPosition, m_StartPosition, lerp);
			}

			m_PressCoroutine = null;
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_PressCoroutine != null)
                writer.WriteValue(k_TimerKey, m_Timer);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            if (reader.TryReadValue(k_TimerKey, out m_Timer, m_Timer))
                m_PressCoroutine = StartCoroutine(PressCoroutine(m_Timer));
        }
    }
}