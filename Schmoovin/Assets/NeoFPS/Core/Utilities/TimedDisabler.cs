using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-timeddisabler.html")]
    public class TimedDisabler : MonoBehaviour, INeoSerializableComponent
	{
		[SerializeField, Tooltip("The time after starting that the action will be performed.")]
		private float m_Timeout = 5f;

        [SerializeField, Tooltip("The action to performm on timeout (disable or destroy the object).")]
		private TimeoutAction m_Action = TimeoutAction.Disable;

        private static readonly NeoSerializationKey k_TimerKey = new NeoSerializationKey("timer");

        private float m_Timer = 0f;
        private Coroutine m_TimedDisableCoroutine = null;

		public enum TimeoutAction
		{
			Disable,
			Destroy
		}

#if UNITY_EDITOR
        void OnValidate ()
        {
            if (m_Timeout < 0f)
                m_Timeout = 0f;
        }
#endif
        
		void OnEnable ()
        {
            m_TimedDisableCoroutine = StartCoroutine(TimedDisable(0f));
		}
        
        IEnumerator TimedDisable(float timer)
        {
            m_Timer = timer;
            while (m_Timer < m_Timeout)
            {
                yield return null;
                m_Timer += Time.deltaTime;
            }

            m_TimedDisableCoroutine = null;

            if (m_Action == TimeoutAction.Disable)
                gameObject.SetActive(false);
            else
                Destroy(gameObject);
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_TimedDisableCoroutine != null)
                writer.WriteValue(k_TimerKey, m_Timer);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            if (reader.TryReadValue(k_TimerKey, out m_Timer, m_Timer))
            {
                if (m_TimedDisableCoroutine == null)
                    m_TimedDisableCoroutine = StartCoroutine(TimedDisable(m_Timer));
            }
        }
    }
}