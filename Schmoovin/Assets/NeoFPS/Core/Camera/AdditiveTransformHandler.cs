using System;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
	[HelpURL("https://docs.neofps.com/manual/fpcamref-mb-additivetransformhandler.html")]
	public class AdditiveTransformHandler : MonoBehaviour, IAdditiveTransformHandler, INeoSerializableComponent
	{
		[SerializeField, NeoObjectInHierarchyField(true), Tooltip("The transform to move (if null, it will move the transform of the gameObject the behaviour is attached to).")]
		private Transform m_TargetTransform = null;

		[SerializeField, Tooltip("The pivot point to rotate and move around")]
		private Vector3 m_PivotOffset = Vector3.zero;

		[SerializeField, Tooltip("When to update the transform.")]// FixedAndLerp will update in fixed, but lerp between results in Update for a smooth result.")]
		private UpdateType m_UpdateWhen = UpdateType.LateUpdate;

		private static readonly NeoSerializationKey k_PosMultKey = new NeoSerializationKey("posMult");
		private static readonly NeoSerializationKey k_RotMultKey = new NeoSerializationKey("rotMult");

		private Vector3 m_PositionLerpFrom = Vector3.zero;
		private Vector3 m_PositionLerpTo = Vector3.zero;
		private Quaternion m_RotationLerpFrom = Quaternion.identity;
		private Quaternion m_RotationLerpTo = Quaternion.identity;

		public enum UpdateType
		{
			Update,
			LateUpdate,
			FixedUpdate,
			FixedAndLerp,
			FixedAndLateLerp
		}

		private List<IAdditiveTransform> m_AdditiveEffects = new List<IAdditiveTransform>(4);
		private Dictionary<Type, List<IAdditiveTransform>> m_AdditiveTransformDictionary = new Dictionary<Type, List<IAdditiveTransform>>();

		public Transform targetTransform
		{
			get { return m_TargetTransform; }
		}

		private float m_SpringPositionMultiplier = 1f;
		public float springPositionMultiplier
		{
			get { return m_SpringPositionMultiplier; }
			set	{ m_SpringPositionMultiplier = value; }
		}

		private float m_SpringRotationMultiplier = 1f;
		public float springRotationMultiplier
		{
			get { return m_SpringRotationMultiplier; }
			set { m_SpringRotationMultiplier = value; }
		}

		public void ApplyAdditiveEffect (IAdditiveTransform add)
		{
			if (add != null && !m_AdditiveEffects.Contains (add))
			{
				// Add the effect
				m_AdditiveEffects.Add (add);
				// Add to the type dictionary
				Type t = add.GetType ();
				List<IAdditiveTransform> list;
				if (m_AdditiveTransformDictionary.TryGetValue (t, out list))
				{
					list.Add (add);
				}
				else
				{
					list = new List<IAdditiveTransform> (1);
					list.Add (add);
					m_AdditiveTransformDictionary.Add (t, list);
				}
			}
		}

		public void RemoveAdditiveEffect (IAdditiveTransform add)
		{
			int index = m_AdditiveEffects.IndexOf (add);
			if (index != -1)
			{
				// Remove the effect
				m_AdditiveEffects.RemoveAt (index);
				// Remove from the type dictionary
				Type t = add.GetType ();
				List<IAdditiveTransform> list;
				if (m_AdditiveTransformDictionary.TryGetValue (t, out list))
					list.Remove (add);
			}
		}

		void OnEnable()
        {
			if (m_TargetTransform == null)
				m_TargetTransform = transform;
			m_PositionLerpFrom = m_PositionLerpTo = m_TargetTransform.localPosition;
			m_RotationLerpFrom = m_RotationLerpTo = m_TargetTransform.localRotation;
		}

		void Update ()
		{
			if (m_UpdateWhen == UpdateType.Update)
				UpdateTransforms (false);
			else
			{
				if (m_UpdateWhen == UpdateType.FixedAndLerp)
                    Interpolate();
            }
		}

		void LateUpdate ()
		{
			if (m_UpdateWhen == UpdateType.LateUpdate)
				UpdateTransforms (false);
			else
            {
                if (m_UpdateWhen == UpdateType.FixedAndLateLerp)
                    Interpolate();
            }
        }

        void Interpolate()
        {
            // Interpolate position and rotation based on start & end calculated in fixed update
            float elapsed = Time.unscaledTime - Time.fixedUnscaledTime;
            if (elapsed > Time.fixedUnscaledDeltaTime)
            {
				m_TargetTransform.localPosition = m_PositionLerpTo;
				m_TargetTransform.localRotation = m_RotationLerpTo;
            }
            else
            {
                // Would store an inverse, but it seems to cause crazy jitter
                float lerp = Mathf.Clamp01(elapsed / Time.fixedUnscaledDeltaTime);
				m_TargetTransform.localPosition = Vector3.LerpUnclamped(m_PositionLerpFrom, m_PositionLerpTo, lerp);
				m_TargetTransform.localRotation = Quaternion.LerpUnclamped(m_RotationLerpFrom, m_RotationLerpTo, lerp);
            }
        }

		void FixedUpdate ()
		{
            if (m_UpdateWhen == UpdateType.FixedUpdate)
            {
                UpdateTransforms(false);
            }
            if (m_UpdateWhen == UpdateType.FixedAndLerp ||
			    m_UpdateWhen == UpdateType.FixedAndLateLerp)
            {
                UpdateTransforms(true);
            }
		}

        void UpdateTransforms (bool lerp)
		{
            if (lerp)
            {
                m_PositionLerpFrom = m_PositionLerpTo;
                m_RotationLerpFrom = m_RotationLerpTo;
            }

            Quaternion rotation = Quaternion.identity;
            Vector3 position = m_PivotOffset;

            // Accumulate effects
            for (int i = 0; i < m_AdditiveEffects.Count; ++i)
			{
				IAdditiveTransform effect = m_AdditiveEffects [i];
				effect.UpdateTransform ();

                // Apply position
                if (effect.bypassPositionMultiplier || Mathf.Approximately(springPositionMultiplier, 1f))
                    position += rotation * effect.position;
                else
                    position += rotation * effect.position * springPositionMultiplier;

                // Apply rotation
                if (effect.bypassRotationMultiplier || Mathf.Approximately(springRotationMultiplier, 1f))
                    rotation *= effect.rotation;
                else
                    rotation *= MathExtensions.ScaleRotation(effect.rotation, springRotationMultiplier);
            }

			// Pivot offset
            position -= rotation * m_PivotOffset;

            if (lerp)
            {
                m_PositionLerpTo = position;
                m_RotationLerpTo = rotation;
            }
            else
            {
				m_TargetTransform.localPosition = position;
				m_TargetTransform.localRotation = rotation;
            }
		}

		public T GetAdditiveTransform<T> () where T : class, IAdditiveTransform
		{
			// Get type
			Type t = typeof(T);
			// Get the list
			List<IAdditiveTransform> list;
			if (m_AdditiveTransformDictionary.TryGetValue (t, out list))
			{
				if (list.Count > 0)
					return list [0] as T;
				else
					return null;
			}
			else
			{
				return null;
			}
		}
		public T[] GetAdditiveTransforms<T> () where T : class, IAdditiveTransform
		{
			// Get type
			Type t = typeof(T);
			// Get the list
			List<IAdditiveTransform> list;
			if (m_AdditiveTransformDictionary.TryGetValue (t, out list))
			{
				if (list.Count > 0)
				{
					T[] result = new T[list.Count];
					for (int i = 0; i < list.Count; ++i)
						result [i] = list [i] as T;
					return result;
				}
				else
					return new T[0];
			}
			else
			{
				return new T[0];
			}
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_PosMultKey, m_SpringPositionMultiplier);
            writer.WriteValue(k_RotMultKey, m_SpringRotationMultiplier);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_PosMultKey, out m_SpringPositionMultiplier, m_SpringPositionMultiplier);
            reader.TryReadValue(k_RotMultKey, out m_SpringRotationMultiplier, m_SpringRotationMultiplier);
        }
    }
}