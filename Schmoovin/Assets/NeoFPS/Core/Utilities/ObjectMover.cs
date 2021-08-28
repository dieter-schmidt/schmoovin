using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-objectmover.html")]
	public class ObjectMover : MonoBehaviour, INeoSerializableComponent
	{
        [SerializeField, Tooltip("The offset from the starting position to move to.")]
        private Vector3 m_OffsetPosition = Vector3.zero;

        [SerializeField, Tooltip("The time taken to move from position to position.")]
		private float m_Duration = 1f;

        private static readonly NeoSerializationKey k_PositionsKey = new NeoSerializationKey("positions");
        private static readonly NeoSerializationKey k_LerpKey = new NeoSerializationKey("lerp");
        private static readonly NeoSerializationKey k_LerpTargetKey = new NeoSerializationKey("lerpTarget");

        Transform m_LocalTransform = null;
        Vector3 [] m_Positions = null;
        float m_Lerp = 0f;
        float m_LerpTarget = 0f;

#if UNITY_EDITOR
        void OnValidate ()
        {
            if (m_Duration < 0.5f)
                m_Duration = 0.5f;
        }
#endif

        void Start ()
		{
			m_Positions = new Vector3[2];

			m_LocalTransform = transform;

			m_Positions [0] = m_LocalTransform.localPosition;
			m_Positions [1] = m_LocalTransform.localPosition + (m_LocalTransform.localRotation * m_OffsetPosition);
		}

		public void SwapState ()
		{
			if (m_LerpTransform != null)
				StopCoroutine (m_LerpTransform);
			
			m_LerpTarget = 1f - m_LerpTarget;

			m_LerpTransform = StartCoroutine (LerpTransform ());
		}

		private Coroutine m_LerpTransform = null;
        IEnumerator LerpTransform ()
		{
			float increment = 1f / m_Duration;
			if (m_Lerp > m_LerpTarget)
				increment *= -1f;

			while (m_Lerp != m_LerpTarget)
			{
				yield return null;

				m_Lerp = Mathf.Clamp01 (m_Lerp + (Time.deltaTime * increment));
				m_LocalTransform.localPosition = Vector3.Lerp (m_Positions [0], m_Positions [1], m_Lerp);
			}

			m_LerpTransform = null;
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValues(k_PositionsKey, m_Positions);
            if (m_LerpTransform != null)
            {
                writer.WriteValue(k_LerpKey, m_Lerp);
                writer.WriteValue(k_LerpTargetKey, m_LerpTarget);
            }
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValues(k_PositionsKey, out m_Positions, m_Positions);

            if (reader.TryReadValue(k_LerpKey, out m_Lerp, m_Lerp))
            {
                reader.TryReadValue(k_LerpTargetKey, out m_LerpTarget, m_LerpTarget);
                m_LerpTransform = StartCoroutine(LerpTransform());
            }
        }
    }
}