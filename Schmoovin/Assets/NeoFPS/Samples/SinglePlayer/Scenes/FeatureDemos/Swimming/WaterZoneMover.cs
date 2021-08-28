using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.Samples.SinglePlayer
{
    public class WaterZoneMover : MonoBehaviour
    {
        [SerializeField, Tooltip("")]
        private float m_PauseDuration = 4f;
        [SerializeField, Tooltip("")]
        private float m_MoveDuration = 5f;
        [SerializeField, Tooltip("")]
        private Vector3 m_MoveOffset = new Vector3(0f, -10f, 0f);

        private Transform m_LocalTransform = null;
        private Vector3 m_StartingPosition = Vector3.zero;

        private void Start()
        {
            m_LocalTransform = transform;
            m_StartingPosition = m_LocalTransform.localPosition;
        }

        private void Update()
        {
            float scale = 1f / m_MoveDuration;
            float pause = m_PauseDuration * scale;
            float lerp = Mathf.PingPong(Time.timeSinceLevelLoad * scale, pause + 1f) - pause * 0.5f;
            m_LocalTransform.localPosition = Vector3.Lerp(m_StartingPosition, m_StartingPosition + m_MoveOffset, lerp);
        }
    }
}