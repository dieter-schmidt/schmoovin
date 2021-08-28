using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class HudTargetLock : WorldSpaceHudMarkerItemBase, IHudTargetLock
    {
		[SerializeField, Tooltip("The transform used to show the progress of the target lock. This size will reduce as the lock strength increases.")]
        public RectTransform m_ProgressTransform = null;

        private Transform m_TargetTransform = null;
        private Vector3 m_TargetOffset = Vector3.zero;
        private float m_LockStrength = 0f;
        
        public void SetLockTarget(Collider c)
        {
            m_TargetTransform = c.transform;
            m_TargetOffset = Quaternion.Inverse(m_TargetTransform.rotation) * (c.bounds.center - m_TargetTransform.position);
            m_LockStrength = 0f;
            m_ProgressTransform.gameObject.SetActive(false);
        }

        public void SetPartialLockTarget(Collider c, float strength)
        {
            m_TargetTransform = c.transform;
            m_TargetOffset = Quaternion.Inverse(m_TargetTransform.rotation) * (c.bounds.center - m_TargetTransform.position);
            m_LockStrength = strength;
            m_ProgressTransform.gameObject.SetActive(true);
        }

        public void SetLockStrength(float strength)
        {
            m_LockStrength = strength;

            if (m_ProgressTransform != null)
            {
                float size = m_LockStrength == 1f ? 96f : Mathf.Lerp(256f, 128f, m_LockStrength);
                m_ProgressTransform.sizeDelta = new Vector2(size, size);
            }
        }

        public override Vector3 GetWorldPosition()
        {
            if (m_TargetTransform != null)
                return m_TargetTransform.position + m_TargetTransform.rotation * m_TargetOffset;
            else
                return Vector3.zero;
        }
    }
}
