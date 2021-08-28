using NeoSaveGames.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class PoseHandler : IPoseHandler
    {
        private Transform m_PoseTransform = null;
        private Vector3 m_IdlePosition = Vector3.zero;
        private Vector3 m_SourcePosition = Vector3.zero;
        private Vector3 m_TargetPosition = Vector3.zero;
        private Quaternion m_IdleRotation = Quaternion.identity;
        private Quaternion m_SourceRotation = Quaternion.identity;
        private Quaternion m_TargetRotation = Quaternion.identity;
        private CustomPositionInterpolation m_CustomPositionInterpolation = null;
        private CustomRotationInterpolation m_CustomRotationInterpolation = null;
        private float m_InverseTransitionDuration = 1f;
        private float m_PoseLerp = 1f;
        
        public PoseHandler(Transform t)
        {
            m_PoseTransform = t;
        }

        public PoseHandler (Transform t, Vector3 idlePosition, Quaternion idleRotation)
        {
            m_PoseTransform = t;
            m_IdlePosition = idlePosition;
            m_IdleRotation = idleRotation;
        }

        public void OnDisable()
        {
            m_SourcePosition = m_TargetPosition;
            m_SourceRotation = m_TargetRotation;
            m_PoseTransform.localPosition = m_TargetPosition;
            m_PoseTransform.localRotation = m_TargetRotation;
            m_CustomPositionInterpolation = null;
            m_CustomRotationInterpolation = null;
            m_PoseLerp = 1f;
        }

        public void SetPose(Vector3 position, Quaternion rotation, float duration)
        {
            SetPose(position, null, rotation, null, duration);
        }

        public void SetPose(Vector3 position, CustomPositionInterpolation posInterp, Quaternion rotation, CustomRotationInterpolation rotInterp, float duration)
        {
            m_SourcePosition = m_PoseTransform.localPosition;
            m_SourceRotation = m_PoseTransform.localRotation;
            m_TargetPosition = position;
            m_TargetRotation = rotation;

            // Assign interpolation methods
            m_CustomPositionInterpolation = posInterp;
            m_CustomRotationInterpolation = rotInterp;

            // Reset lerp
            m_PoseLerp = 0f;

            // Get inverse duration
            if (duration < 0.001f)
                m_InverseTransitionDuration = 0f;
            else
                m_InverseTransitionDuration = 1f / duration;
        }

        public void ResetInstant()
        {
        }

        public void ResetPose(float duration)
        {
            SetPose(m_IdlePosition, null, m_IdleRotation, null, duration);
        }

        public void ResetPose(CustomPositionInterpolation posInterp, CustomRotationInterpolation rotInterp, float duration)
        {
            SetPose(m_IdlePosition, posInterp, m_IdleRotation, rotInterp, duration);
        }

        public void UpdatePose()
        {
            if (m_PoseLerp < 1f)
            {
                // Progress time
                if (m_InverseTransitionDuration != 0f)
                    m_PoseLerp += Time.deltaTime * m_InverseTransitionDuration;
                else
                    m_PoseLerp = 1f;

                // Check if completed
                if (m_PoseLerp >= 1f)
                {
                    m_PoseLerp = 1f;
                    m_PoseTransform.localPosition = m_TargetPosition;
                    m_PoseTransform.localRotation = m_TargetRotation;
                }
                else
                {
                    // Interpolate position
                    if (m_CustomPositionInterpolation != null)
                        m_PoseTransform.localPosition = m_CustomPositionInterpolation(m_SourcePosition, m_TargetPosition, m_PoseLerp);
                    else
                        m_PoseTransform.localPosition = Vector3.Lerp(m_SourcePosition, m_TargetPosition, EasingFunctions.EaseInOutQuadratic(m_PoseLerp));

                    // Interpolate rotation
                    if (m_CustomRotationInterpolation != null)
                        m_PoseTransform.localRotation = m_CustomRotationInterpolation(m_SourceRotation, m_TargetRotation, m_PoseLerp);
                    else
                        m_PoseTransform.localRotation = Quaternion.Slerp(m_SourceRotation, m_TargetRotation, EasingFunctions.EaseInOutQuadratic(m_PoseLerp));
                }
            }
        }

        private static readonly NeoSerializationKey k_PoseLerpKey = new NeoSerializationKey("poseLerp");
        private static readonly NeoSerializationKey k_SourcePositionKey = new NeoSerializationKey("sourcePos");
        private static readonly NeoSerializationKey k_TargetPositionKey = new NeoSerializationKey("targetPos");
        private static readonly NeoSerializationKey k_SourceRotationKey = new NeoSerializationKey("sourceRot");
        private static readonly NeoSerializationKey k_TargetRotationKey = new NeoSerializationKey("targetRotation");
        private static readonly NeoSerializationKey k_InvDurationKey = new NeoSerializationKey("invDuration");

        public void WriteProperties(INeoSerializer writer)
        {
            // NB: Can't save/load delegates, so custom interpolation will be discarded

            writer.WriteValue(k_TargetPositionKey, m_TargetPosition);
            writer.WriteValue(k_TargetRotationKey, m_TargetRotation);

            if (m_PoseLerp < 1f)
            {
                writer.WriteValue(k_PoseLerpKey, m_PoseLerp);
                writer.WriteValue(k_SourcePositionKey, m_SourcePosition);
                writer.WriteValue(k_SourceRotationKey, m_SourceRotation);
                writer.WriteValue(k_InvDurationKey, m_InverseTransitionDuration);
            }
        }

        public void ReadProperties(INeoDeserializer reader)
        {
            reader.TryReadValue(k_TargetPositionKey, out m_TargetPosition, m_TargetPosition);
            reader.TryReadValue(k_TargetRotationKey, out m_TargetRotation, m_TargetRotation);

            if (reader.TryReadValue(k_PoseLerpKey, out m_PoseLerp, m_PoseLerp))
            {
                reader.TryReadValue(k_SourcePositionKey, out m_SourcePosition, m_SourcePosition);
                reader.TryReadValue(k_SourceRotationKey, out m_SourceRotation, m_SourceRotation);
                reader.TryReadValue(k_InvDurationKey, out m_InverseTransitionDuration, m_InverseTransitionDuration);
                m_PoseTransform.localPosition = Vector3.Lerp(m_SourcePosition, m_TargetPosition, EasingFunctions.EaseInOutQuadratic(m_PoseLerp));
                m_PoseTransform.localRotation = Quaternion.Slerp(m_SourceRotation, m_TargetRotation, EasingFunctions.EaseInOutQuadratic(m_PoseLerp));
            }
            else
            {
                m_PoseTransform.localPosition = m_TargetPosition;
                m_PoseTransform.localRotation = m_TargetRotation;
            }
        }
    }
}