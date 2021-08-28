#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Audio/SlidingAudio", "SlidingAudioBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-slidingaudiobehaviour.html")]
    public class SlidingAudioBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The surface audio library for the slide audio clips.")]
		private SurfaceAudioData m_AudioData = null;

        [SerializeField, Tooltip("The direction to cast in to get the surface type.")]
        private Direction m_CastDirection = Direction.Down;

        [SerializeField, Tooltip("The direction vector for the ray to cast when detecting surface type.")]
        private Vector3 m_CastVector = Vector3.down;

        [SerializeField, Tooltip("The direction vector parameter for the ray to cast when detecting surface type. If none is selected, defaults to character down.")]
        private VectorParameter m_VectorParameter = null;

        [SerializeField, Tooltip("The speed below which the pitch will be at its minimum.")]
		private float m_MinimumSpeed = 5f;

        [SerializeField, Tooltip("The speed above which the pitch will be at its maximum.")]
		private float m_MaximumSpeed = 10f;

        [SerializeField, Tooltip("The minimum pitch for the slide loop.")]
		private float m_MinimumPitch = 0.25f;

        [SerializeField, Tooltip("The maximum pitch for the slide loop.")]
		private float m_MaximumPitch = 1.1f;

        [SerializeField, Tooltip("The downward raycast length for the ground surface test.")]
		private float m_MaxRayDistance = 1f;

#if NEOFPS_LIGHTWEIGHT
        private const int k_SurfaceTestInterval = 50;
#else
        private const int k_SurfaceTestInterval = 10;
#endif

        private ICharacterAudioHandler m_AudioHandler = null;
        private RaycastHit m_Hit = new RaycastHit();
        private int m_TestCounter = 0;
        private bool m_FirstFrame = false;
        private float m_LoopVolume = 1f;
        private FpsSurfaceMaterial m_Surface = FpsSurfaceMaterial.Default;
        
        public enum Direction
        {
            Down,
            InverseGroundNormal,
            LocalVector,
            WorldVector,
            WorldParameter,
            WorldParameterInverse,
            LocalParameter,
            LocalParameterInverse
        }

        public override void OnValidate()
        {
			// Check minimum speed
			if (m_MinimumSpeed < 0f)
				m_MinimumSpeed = 0f;
			// Check maximum speed
			m_MaximumSpeed = Mathf.Clamp (m_MaximumSpeed, m_MinimumSpeed + 0.01f, 25f);
        }

        public override void OnEnter()
        {
            // Get audio handler
            if (m_AudioHandler == null)
                m_AudioHandler = controller.localTransform.GetComponent<ICharacterAudioHandler>();

            m_TestCounter = 0;
            m_FirstFrame = true;
        }

        public override void OnExit()
        {
            if (m_AudioHandler != null)
                m_AudioHandler.StopLoop(FpsCharacterAudioSource.Feet);
        }

        FpsSurfaceMaterial GetGroundSurface()
        {
            Space space = Space.World;
            FpsSurfaceMaterial result = m_Surface;

            // Get charactercontroller info
            var cc = controller.characterController;

#if NEOFPS_LIGHTWEIGHT

            // Get raycast direction
            Vector3 direction = Vector3.down;
            switch (m_CastDirection)
            {
                case Direction.InverseGroundNormal:
                    if (cc.isGrounded)
                        direction = -cc.groundNormal;
                    break;
                case Direction.LocalVector:
                    direction = m_CastVector;
                    space = Space.Self;
                    break;
                case Direction.WorldVector:
                    direction = m_CastVector;
                    break;
                case Direction.WorldParameter:
                    if (m_VectorParameter != null)
                        direction = m_VectorParameter.value.normalized;
                    break;
                case Direction.WorldParameterInverse:
                    if (m_VectorParameter != null)
                        direction = -m_VectorParameter.value.normalized;
                    break;
                case Direction.LocalParameter:
                    if (m_VectorParameter != null)
                    {
                        direction = m_VectorParameter.value.normalized;
                        space = Space.Self;
                    }
                    break;
                case Direction.LocalParameterInverse:
                    if (m_VectorParameter != null)
                    {
                        direction = -m_VectorParameter.value.normalized;
                        space = Space.Self;
                    }
                    break;
            }

#else
            
            // Get raycast direction
            Vector3 direction = Vector3.zero;
            switch (m_CastDirection)
            {
                case Direction.Down:
                    direction = Vector3.down;
                    space = Space.Self;
                    break;
                case Direction.InverseGroundNormal:
                    if (cc.isGrounded)
                        direction = -cc.groundNormal;
                    else
                    {
                        direction = Vector3.down;
                        space = Space.Self;
                    }
                    break;
                case Direction.LocalVector:
                    direction = m_CastVector;
                    space = Space.Self;
                    break;
                case Direction.WorldVector:
                    direction = m_CastVector;
                    break;
                case Direction.WorldParameter:
                    if (m_VectorParameter != null)
                        direction = m_VectorParameter.value.normalized;
                    else
                        direction = -controller.characterController.up;
                    break;
                case Direction.WorldParameterInverse:
                    if (m_VectorParameter != null)
                        direction = -m_VectorParameter.value.normalized;
                    else
                        direction = -controller.characterController.up;
                    break;
                case Direction.LocalParameter:
                    if (m_VectorParameter != null)
                        direction = m_VectorParameter.value.normalized;
                    else
                        direction = Vector3.down;
                    space = Space.Self;
                    break;
                case Direction.LocalParameterInverse:
                    if (m_VectorParameter != null)
                        direction = -m_VectorParameter.value.normalized;
                    else
                        direction = Vector3.down;
                    space = Space.Self;
                    break;
            }

#endif

            var radius = cc.radius;
            var normalisedHeight = radius / cc.height;

            // Raycast for surface type
            if (cc.RayCast(normalisedHeight, direction * (m_MaxRayDistance + radius), space, out m_Hit, PhysicsFilter.Masks.BulletBlockers, QueryTriggerInteraction.Ignore))
            {
                // Get surface ID from ray hit
                Transform t = m_Hit.transform;
                if (t != null)
                {
                    BaseSurface s = t.GetComponent<BaseSurface>();
                    if (s != null)
                        result = s.GetSurface(m_Hit);
                    else
                        result = FpsSurfaceMaterial.Default;
                }

                m_Surface = result;
            }

            return result;
        }

        public override void Update()
        {
            if (m_AudioHandler == null || m_AudioData == null)
                return;

            // Get pitch from speed
            float speed = controller.characterController.velocity.magnitude;
            speed -= m_MinimumSpeed;
            speed /= m_MaximumSpeed - m_MinimumSpeed;
            float pitch = Mathf.Lerp (m_MinimumPitch, m_MaximumPitch, speed) * Time.timeScale;
            
            // Check surface
            int lastSurface = m_Surface;
            if (m_TestCounter-- == 0)
            {
                m_Surface = GetGroundSurface();
                m_TestCounter = k_SurfaceTestInterval;
            }

            // If surface has changed, restart the loop
            if (m_Surface != lastSurface || m_FirstFrame)
            {
                m_FirstFrame = false;

                AudioClip clip = m_AudioData.GetAudioClip(m_Surface, out m_LoopVolume);
                if (clip != null)
                    m_AudioHandler.StartLoop(clip, FpsCharacterAudioSource.Feet, m_LoopVolume * speed, pitch);
            }
            else
            {
                m_AudioHandler.SetLoopPitch(FpsCharacterAudioSource.Feet, pitch);
                m_AudioHandler.SetLoopVolume(FpsCharacterAudioSource.Feet, m_LoopVolume * speed);
            }
        }
    }
}