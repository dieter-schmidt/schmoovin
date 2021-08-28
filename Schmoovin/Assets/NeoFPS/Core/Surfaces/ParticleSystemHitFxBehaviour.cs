using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/surfacesref-mb-particlesystemhitfxbehaviour.html")]
    public class ParticleSystemHitFxBehaviour : BaseHitFxBehaviour
	{
		[Header ("Impact Effects")]

		[SerializeField, Tooltip("The particle system for chunks of surface material (eg. wood splinters, paint chips).")]
		private ParticleSystem m_ChipsSystem = null;

        [SerializeField, Range (1, 50), Tooltip("The number of chip particles for a bullet hit with size 1.")]
		private int m_DefaultChipCount = 5;

		[SerializeField, Tooltip("The particle system for fine dust spray or similar.")]
		private ParticleSystem m_DustSystem = null;

        [SerializeField, Range (1, 50), Tooltip("The number of dust particles for a bullet hit with size 1.")]
		private int m_DefaultDustCount = 12;

		[Header ("Decals")]

		[SerializeField, Tooltip("The particle system used to place a decal.")]
		private ParticleSystem m_DecalSystem = null;

        [SerializeField, Range(0.01f, 5f), Tooltip("The decal size multiplier before modification by the hit size.")]
		private float m_DefaultDecalSize = 0.1f;

		[Header ("Control")]

		[SerializeField, Range (0f, 1f), Tooltip("The amount of deflection for the hit system (0 follows the impact normal, 1 follows the reflected impact ray).")]
		private float m_Deflection = 0.25f;

		[SerializeField, Range (0f, 0.1f), Tooltip("The distance from the surface to place the decal (prevents z-fighting).")]
		private float m_SurfaceOffset = 0.01f;

		private const float k_2Pi = 6.283185307179586476925286766559f;

        public override bool forceInitialise { get { return true; } }

        public override void OnActiveSceneChange()
        {
            if (m_ChipsSystem != null)
                m_ChipsSystem.Clear(true);
            if (m_DustSystem != null)
                m_DustSystem.Clear(true);
            if (m_DecalSystem != null)
                m_DecalSystem.Clear(true);
        }

        public override void Hit (GameObject hitObject, Vector3 position, Vector3 normal)
		{
			Hit (hitObject, position, normal, Vector3.zero, 1f, false);
		}

		public override void Hit (GameObject hitObject, Vector3 position, Vector3 normal, float size)
		{
			Hit (hitObject, position, normal, Vector3.zero, size, false);
		}

		public override void Hit (GameObject hitObject, Vector3 position, Vector3 normal, Vector3 ray, float size)
		{
			Hit (hitObject, position, normal, ray, size, false);
		}

		public override void Hit (GameObject hitObject, Vector3 position, Vector3 normal, Vector3 ray, float size, bool decal)
		{
			Quaternion normalRotation = Quaternion.LookRotation (normal);
			Quaternion deflected = normalRotation;
			if (m_Deflection > 0f && ray != Vector3.zero)
			{
				Vector3 reflection = Vector3.Reflect (ray, normal);
				deflected = Quaternion.Lerp (normalRotation, Quaternion.LookRotation (reflection), m_Deflection);
			}

			transform.position = position + (normal * m_SurfaceOffset);
			transform.rotation = deflected;

			if (m_ChipsSystem != null && m_DefaultChipCount > 0)
				m_ChipsSystem.Emit ((int)((float)m_DefaultChipCount * size));
			if (m_DustSystem != null && m_DefaultDustCount > 0)
				m_DustSystem.Emit ((int)((float)m_DefaultDustCount * size));

			if (decal && m_DecalSystem != null)
			{
                var details = m_DecalSystem.main;
				var euler = normalRotation.eulerAngles;
				details.startRotationX = euler.x * Mathf.Deg2Rad;
				details.startRotationY = euler.y * Mathf.Deg2Rad;
				details.startRotationZ = Random.Range (0f, k_2Pi);
				details.startSize = m_DefaultDecalSize * size;
				m_DecalSystem.Emit (1);
			}
		}
	}
}