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
    [MotionGraphElement("Audio/SurfaceAudio", "SurfaceAudioBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-surfaceaudiobehaviour.html")]
    public class SurfaceAudioBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The surface audio library for the audio clips.")]
		private SurfaceAudioData m_AudioData = null;

        [SerializeField, Tooltip("When should the audio be played.")]
        private When m_When = When.OnEnter;

        [SerializeField, Tooltip("The direction to cast in to get the surface type.")]
        private Direction m_CastDirection = Direction.Down;

        [SerializeField, Tooltip("The direction vector for the ray to cast when detecting surface type.")]
        private Vector3 m_CastVector = Vector3.down;

        [SerializeField, Tooltip("The direction vector parameter for the ray to cast when detecting surface type. If none is selected, defaults to character down.")]
        private VectorParameter m_VectorParameter = null;

        [SerializeField, Tooltip("The downward raycast length for the ground surface test.")]
		private float m_MaxRayDistance = 1f;
        
        private ICharacterAudioHandler m_AudioHandler = null;
        private RaycastHit m_Hit = new RaycastHit();

        public enum Direction
        {
            Down,
            LocalVector,
            WorldVector,
            WorldParameter,
            WorldParameterInverse,
            LocalParameter,
            LocalParameterInverse
        }

        public enum When
        {
            OnEnter,
            OnExit,
            Both
        }

        public override void OnEnter()
        {
            // Get audio handler
            if (m_AudioHandler == null)
                m_AudioHandler = controller.localTransform.GetComponent<ICharacterAudioHandler>();
                
            // Play the relevant audio clip
            if (m_When != When.OnExit && m_AudioHandler != null && m_AudioData != null)
                PlayAudio(GetGroundSurface());
        }

        public override void OnExit()
        {
            if (m_When != When.OnEnter && m_AudioHandler != null && m_AudioData != null)
                PlayAudio(GetGroundSurface());
        }
        
		FpsSurfaceMaterial GetGroundSurface ()
		{
			FpsSurfaceMaterial result = FpsSurfaceMaterial.Default;

#if NEOFPS_LIGHTWEIGHT

            // Get raycast direction
            Vector3 direction = Vector3.down;
            switch (m_CastDirection)
            {
                case Direction.LocalVector:
                    direction = controller.localTransform.rotation * m_CastVector;
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
                        direction = controller.localTransform.rotation * m_VectorParameter.value.normalized;
                    break;
                case Direction.LocalParameterInverse:
                    if (m_VectorParameter != null)
                        direction = controller.localTransform.rotation * -m_VectorParameter.value.normalized;
                    break;
            }

#else

            // Get raycast direction
            Vector3 direction = Vector3.zero;
            switch (m_CastDirection)
            {
                case Direction.Down:
                    direction = -controller.characterController.up;
                    break;
                case Direction.LocalVector:
                    direction = controller.localTransform.rotation * m_CastVector;
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
                        direction = controller.localTransform.rotation * m_VectorParameter.value.normalized;
                    else
                        direction = -controller.characterController.up;
                    break;
                case Direction.LocalParameterInverse:
                    if (m_VectorParameter != null)
                        direction = controller.localTransform.rotation * -m_VectorParameter.value.normalized;
                    else
                        direction = -controller.characterController.up;
                    break;
            }

#endif

            // Get charactercontroller info
            var cc = controller.characterController;
            var radius = cc.radius;
            var normalisedHeight = radius / cc.height;

            // Raycast for surface type
            if (cc.RayCast(normalisedHeight, direction * (m_MaxRayDistance + radius), Space.World, out m_Hit, PhysicsFilter.Masks.BulletBlockers, QueryTriggerInteraction.Ignore))
            {
                // Get surface ID from ray hit
                Transform t = m_Hit.transform;
                if (t != null)
                {
                    BaseSurface s = t.GetComponent<BaseSurface>();
                    if (s != null)
                        result = s.GetSurface(m_Hit);
                    else
                        result = 0;
                }
            }
            
			return result;
		}
        
        void PlayAudio(FpsSurfaceMaterial surface)
        {
            float volume = 1f;
            AudioClip clip = m_AudioData.GetAudioClip(surface, out volume);
            if (clip != null)
                m_AudioHandler.PlayClip(clip, FpsCharacterAudioSource.Feet, volume);
        }
    }
}