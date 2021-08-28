using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.Samples.SinglePlayer
{
	[HelpURL("https://docs.neofps.com/manual/samplesref-mb-firingrangemovingtarget.html")]
	public class FiringRangeMovingTarget : FiringRangeTarget
	{
        [SerializeField, Tooltip("The offset to move to.")]
        private Vector3 m_MoveOffset = Vector3.zero;
        
		[SerializeField, Range(1f, 20f), Tooltip("The time taken to reach the offset.")]
		private float m_MoveDuration = 4f;

        private Transform m_PositionTransform = null;
        private Vector3 m_StartPosition = Vector3.zero;
        private Vector3 m_EndPosition = Vector3.zero;

        // Movement properties
        private Coroutine m_MovementCoroutine = null;
        private float m_MoveProgress = 0f;

        // Return properties
        private Coroutine m_ReturnCoroutine = null;
        private float m_MoveLerp = 0f;

        public override bool hidden
        {
            get { return base.hidden && m_MovementCoroutine == null && m_ReturnCoroutine == null; }
        }

        public override void Initialise (FiringRangeSequencer sequencer)
		{
			m_PositionTransform = transform.parent;
			m_StartPosition = m_PositionTransform.localPosition;
			m_EndPosition = m_StartPosition + m_MoveOffset;

			base.Initialise (sequencer);
		}
		
		public override void Popup (float duration)
		{
			base.Popup (duration);
			if (m_MovementCoroutine != null)
				StopCoroutine (m_MovementCoroutine);
            if (m_ReturnCoroutine != null)
                StopCoroutine(m_ReturnCoroutine);
            //m_MovementCoroutine = StartCoroutine (MoveCoroutine (0f));
		}

        public override void ResetTarget ()
		{
			base.ResetTarget();
            if (m_ReturnCoroutine != null)
                StopCoroutine(m_ReturnCoroutine);
            if (m_MovementCoroutine != null)
			{
				StopCoroutine (m_MovementCoroutine);
                m_MovementCoroutine = null;
                m_ReturnCoroutine = StartCoroutine (ReturnCoroutine(0f));
			}
		}

        protected override void StartRaisedSequence()
        {
            base.StartRaisedSequence();
            m_MovementCoroutine = StartCoroutine(MoveCoroutine(0f));
        }

        protected override void StartResetSequence()
        {
            base.StartResetSequence();
            if (m_MovementCoroutine != null)
            {
                StopCoroutine(m_MovementCoroutine);
                m_MovementCoroutine = null;
            }
        }

        IEnumerator MoveCoroutine (float start)
		{
            m_MoveProgress = start;
			float inverseDuration = 1f / m_MoveDuration;

            yield return null;
            while (true)
			{
				yield return null;
                m_MoveProgress += Time.deltaTime * inverseDuration;
				m_PositionTransform.localPosition = Vector3.Lerp (m_StartPosition, m_EndPosition, Mathf.PingPong (m_MoveProgress, 1f));
			}
        }

		IEnumerator ReturnCoroutine (float lerp)
		{
			m_MoveLerp = lerp;
			Vector3 pos = m_PositionTransform.localPosition;

            yield return null;
            while (m_MoveLerp < 1f)
			{
				yield return null;
				m_MoveLerp += Time.deltaTime;
				m_PositionTransform.localPosition = Vector3.Lerp (pos, m_StartPosition, m_MoveLerp);
			}

            m_ReturnCoroutine = null;
            m_MovementCoroutine = null;
        }

        private static readonly NeoSerializationKey k_MoveKey = new NeoSerializationKey("move");
        private static readonly NeoSerializationKey k_ReturnPosKey = new NeoSerializationKey("returnPos");
        private static readonly NeoSerializationKey k_ReturnKey = new NeoSerializationKey("return");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

            if (m_MovementCoroutine != null)
                writer.WriteValue(k_MoveKey, m_MoveProgress);

            if (m_ReturnCoroutine != null)
            {
                writer.WriteValue(k_ReturnPosKey, m_PositionTransform.localPosition);
                writer.WriteValue(k_ReturnKey, m_MoveLerp);
            }
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            if (reader.TryReadValue(k_MoveKey, out m_MoveProgress, m_MoveProgress))
            {
                // Set position
                m_PositionTransform.localPosition = Vector3.Lerp(m_StartPosition, m_EndPosition, Mathf.PingPong(m_MoveProgress, 1f));

                // Start coroutine
                m_MovementCoroutine = StartCoroutine(MoveCoroutine(m_MoveProgress));
            }

            if (reader.TryReadValue(k_ReturnKey, out m_MoveLerp, m_MoveLerp))
            {
                // Set position
                Vector3 pos = Vector3.zero;
                reader.TryReadValue(k_ReturnPosKey, out pos, pos);
                m_PositionTransform.localPosition = pos;

                // Start coroutine
                m_ReturnCoroutine = StartCoroutine(ReturnCoroutine(m_MoveLerp));
            }
        }
    }
}