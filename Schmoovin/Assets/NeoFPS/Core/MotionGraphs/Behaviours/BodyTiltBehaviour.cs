using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/BodyTilt", "BodyTiltBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-bodytiltbehaviour.html")]
    public class BodyTiltBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("The maximum tilt angle from vertical")]
        private float m_TiltAngle = 15f;
        [SerializeField, Range(0f, 1f), Tooltip("The point to tilt from normalised between the base of the character (0) and the head position of the character (1)")]
        private float m_NormalisedTiltPoint = 0f;
        [SerializeField, Tooltip("How is the tilt direction calculated. \"CharacterRelative\" uses the direction vector in the character's local space. \"WorldSpace\" ignores character direction. \"MoveDirection\" tilts away from the direction of movement")]
        private TiltMode m_TiltMode = TiltMode.CharacterRelative;
        [SerializeField, Tooltip("The direction parameter to use to drive the tilt")]
        private VectorParameter m_DirectionVector = null;
        [SerializeField, Tooltip("Is the tilt amount based on the velocity of the character")]
        private bool m_VelocityBased = true;
        [SerializeField, Tooltip("The speed below which the tilt amount will be 0")]
        private float m_MinSpeed = 0f;
        [SerializeField, Tooltip("The speed above which the tilt amount will be 1")]
        private float m_MaxSpeed = 10f;

        public enum TiltMode
        {
            CharacterRelative,
            WorldSpace,
            Velocity,
            VelocityLateral,
            Input,
            InputLateral,
            TiltTowards
        }

        private BodyTilt m_BodyTilt = null;
        
        public override void OnValidate()
        {
            m_TiltAngle = Mathf.Clamp(m_TiltAngle, -45f, 45f);
            // Clamp speeds
            if (m_MinSpeed < 0f)
                m_MinSpeed = 0f;
            m_MaxSpeed = Mathf.Clamp(m_MaxSpeed, Mathf.Max(0.1f, m_MinSpeed), float.MaxValue);
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            // Get the body tilt component
            var character = controller.GetComponent<ICharacter>();
            if (character != null && character.bodyTransformHandler != null)
                m_BodyTilt = character.bodyTransformHandler.GetComponent<BodyTilt>();

            if (m_BodyTilt == null)
                Debug.Log("Body tilt not found");

            if (m_TiltMode == TiltMode.TiltTowards)
                m_VelocityBased = false;
        }

        public override void OnExit()
        {
            if (m_BodyTilt != null)
                m_BodyTilt.ResetTilt();
        }

        public override void Update()
        {
            if (m_BodyTilt == null)
                return;

            // Tilt amount
            float amount = 1f;

            // Get the tilt direction
            Vector2 tiltDirection = Vector2.zero;
            switch (m_TiltMode)
            {
                case TiltMode.CharacterRelative:
                    {
                        Vector3 v = m_DirectionVector.value;
                        tiltDirection.x = v.x;
                        tiltDirection.y = v.z;
                    }
                    break;
                case TiltMode.WorldSpace:
                    {
                        Vector3 v = m_DirectionVector.value;
                        tiltDirection.x = Vector3.Dot(v, controller.characterController.right);
                        tiltDirection.y = Vector3.Dot(v, controller.characterController.forward);
                    }
                    break;
                case TiltMode.Velocity:
                    {
                        Vector3 v = controller.characterController.velocity;
                        tiltDirection.x = Vector3.Dot(v, controller.characterController.right);
                        tiltDirection.y = Vector3.Dot(v, controller.characterController.forward);
                    }
                    break;
                case TiltMode.VelocityLateral:
                    {
                        Vector3 v = controller.characterController.velocity;
                        tiltDirection.x = Vector3.Dot(v, controller.characterController.right);
                    }
                    break;
                case TiltMode.Input:
                    tiltDirection = controller.inputMoveDirection * controller.inputMoveScale;
                    break;
                case TiltMode.InputLateral:
                    tiltDirection.x = controller.inputMoveDirection.x * controller.inputMoveScale;
                    break;
                case TiltMode.TiltTowards:
                    {
                        Vector3 v = (m_DirectionVector.value - controller.localTransform.position).normalized;
                        float up = Vector3.Dot(v, controller.characterController.up);
                        tiltDirection.x = Vector3.Dot(v, controller.characterController.right);
                        tiltDirection.y = Vector3.Dot(v, controller.characterController.forward);

                        amount = (1f - Mathf.Abs(up));
                    }
                    break;
            }

            float magnitude = tiltDirection.magnitude;
            if (magnitude < 0.001f)
            {
                // Reset tilt
                m_BodyTilt.ResetTilt();
            }
            else
            {
                // Normalize the tilt direction
                tiltDirection /= magnitude;

                // Get tilt amount based on velocity
                if (m_VelocityBased)
                {
                    // Get the speed
                    float speed = 0f;
                    switch (m_TiltMode)
                    {
                        case TiltMode.VelocityLateral:
                            speed = magnitude;
                            break;
                        case TiltMode.Input:
                            {
                                float forward = Vector3.Dot(
                                    controller.characterController.velocity,
                                    controller.characterController.forward
                                    ) * tiltDirection.y;
                                if (forward < 0f)
                                    forward = 0f;

                                float lateral = Vector3.Dot(
                                    controller.characterController.velocity,
                                    controller.characterController.right
                                    ) * tiltDirection.x;
                                if (lateral < 0f)
                                    lateral = 0f;

                                speed = forward + lateral;
                            }
                            break;
                        case TiltMode.InputLateral:
                            {
                                speed = Vector3.Dot(
                                    controller.characterController.velocity,
                                    controller.characterController.right
                                    ) * tiltDirection.x;
                                if (speed < 0f)
                                    speed = 0f;
                            }
                            break;
                        default:
                            {
                                Vector3 flattenedVelocity = Vector3.ProjectOnPlane(
                                    controller.characterController.velocity,
                                    controller.characterController.up
                                    );
                                speed = flattenedVelocity.magnitude;
                            }
                            break;
                    }

                    // Get the tilt amount
                    amount = Mathf.Clamp01((speed - m_MinSpeed) / (m_MaxSpeed - m_MinSpeed));
                }

                m_BodyTilt.SetTilt(tiltDirection, m_TiltAngle * amount, m_NormalisedTiltPoint);
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_DirectionVector = map.Swap(m_DirectionVector);
        }
    }
}