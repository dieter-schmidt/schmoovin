#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Audio/FootstepAudio", "FootstepAudioBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-footstepaudiobehaviour.html")]
    public class FootstepAudioBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The surface audio library for the slide audio clips.")]
		private SurfaceAudioData m_AudioData = null;

        [SerializeField, Tooltip("The direction to cast in to get the surface type.")]
        private Direction m_CastDirection = Direction.Down;

        [SerializeField, Tooltip("The direction vector for the ray to cast when detecting surface type.")]
        private Vector3 m_CastVector = Vector3.down;

        [SerializeField, Tooltip("The direction vector parameter for the ray to cast when detecting surface type. If none is selected, defaults to character down.")]
        private VectorParameter m_VectorParameter = null;

        [SerializeField, Tooltip("The interval between steps. Higher numbers mean the steps are further apart.")]
		private float m_StepInterval = 3f;

        [SerializeField, Tooltip("The speed below which no footstep audio will be played.")]
        private float m_MinimumSpeed = 0.01f;

        [SerializeField, Tooltip("The maximum speed that the actual speed will be clamped to. Prevents rapid fire footsteps.")]
		private float m_MaximumSpeed = 10f;

		[SerializeField, Range (0.05f, 2f), Tooltip("The downward raycast length for the ground surface test.")]
		private float m_MaxRayDistance = 1f;

        private static readonly NeoSerializationKey k_CycleKey = new NeoSerializationKey("cycle");
        
        private ICharacterAudioHandler m_AudioHandler = null;
        private RaycastHit m_Hit = new RaycastHit();
        private float m_StepCycle = 0f;
        private FpsSurfaceMaterial m_LastSurfaceId = FpsSurfaceMaterial.Default;

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
			// Clamp values
			m_MaximumSpeed = Mathf.Clamp (m_MaximumSpeed, m_MinimumSpeed + 0.01f, 25f);
            m_StepInterval = Mathf.Clamp(m_StepInterval, 0.1f, 10f);
        }

        public override void OnEnter()
        {
            // Get audio handler
            if (m_AudioHandler == null)
                m_AudioHandler = controller.localTransform.GetComponent<ICharacterAudioHandler>();

            m_StepCycle = 0f;
        }

        public override void OnExit()
        {
        }

        public override void Update()
        {
            if (m_AudioHandler == null || m_AudioData == null)
                return;

            float speed = controller.characterController.velocity.magnitude;
            if (speed > m_MinimumSpeed)
            {
                // Cap speed
                if (speed > m_MaximumSpeed)
                    speed = m_MaximumSpeed;

                // Update step cycle & play audio on step
                m_StepCycle += (speed * Time.deltaTime) / m_StepInterval;
                while (m_StepCycle > 1f)
                {
                    m_StepCycle -= 1f;
                    PlayAudio (GetGroundSurface ());
                }
            }
        }

        FpsSurfaceMaterial GetGroundSurface()
        {
            Space space = Space.World;
            FpsSurfaceMaterial result = m_LastSurfaceId;

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

                m_LastSurfaceId = result;
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

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_CycleKey, m_StepCycle);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_CycleKey, out m_StepCycle, m_StepCycle);
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_VectorParameter = map.Swap(m_VectorParameter);
            base.CheckReferences(map);
        }
    }
}