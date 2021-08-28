using UnityEngine;
using NeoCC;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    /// <summary>
    /// MovingPlatform is a simple implementation of the IMovingPlatform interface.
    /// A moving platform is an environment object that will move a INeoCharacterController when it is in contact.
    /// </summary>
    [RequireComponent(typeof (Rigidbody))]
    public abstract class BaseMovingPlatform : MonoBehaviour, IMovingPlatform, INeoSerializableComponent
    {
        /// <summary>
        /// The current fixed update position of the platform in world space (used for interpolation).
        /// </summary>
        public Vector3 fixedPosition { get; private set; }

        /// <summary>
        /// The position of the platform in world space on the last fixed update frame (used for interpolation).
        /// </summary>
        public Vector3 previousPosition { get; private set; }

        /// <summary>
        /// The current fixed update rotation of the platform in world space (used for interpolation).
        /// </summary>
        public Quaternion fixedRotation { get; private set; }

        /// <summary>
        /// The rotation of the platform in world space on the last fixed update frame (used for interpolation).
        /// </summary>
        public Quaternion previousRotation { get; private set; }

        /// <summary>
        /// The transform of the object (skip the overhead of accessing via the transform property).
        /// </summary>
        public Transform localTransform { get; private set; }

        /// <summary>
        /// The rigidbody of the object
        /// </summary>
        public Rigidbody attachedRigidbody { get; private set; }

        private static readonly NeoSerializationKey k_FixedPositionKey = new NeoSerializationKey("fixedPosition");
        private static readonly NeoSerializationKey k_FixedRotationKey = new NeoSerializationKey("fixedRotation");

        private bool m_Initialised = false;

        protected virtual void Awake()
        {
            localTransform = transform;
            attachedRigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void Start()
        {
            Initialise();
        }

        void Initialise()
        {
            // Initialise the rigidbody
            attachedRigidbody.isKinematic = true;
            attachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            // Initialise the properties
            fixedPosition = GetStartingPosition();
            fixedRotation = GetStartingRotation();
            previousPosition = fixedPosition;
            previousRotation = fixedRotation;

            m_Initialised = true;
        }

        void FixedUpdate ()
        {
            if (!m_Initialised)
                Initialise();

            // Update previous transform properties
            previousPosition = fixedPosition;
            previousRotation = fixedRotation;

            PreFixedUpdate();

            // Get next transform properties
            fixedPosition = GetNextPosition();
            fixedRotation = GetNextRotation();

            // Move to position
            attachedRigidbody.MovePosition(fixedPosition);
            attachedRigidbody.MoveRotation(fixedRotation);

            PostFixedUpdate();
        }

        protected virtual void PreFixedUpdate()
        { }

        protected virtual void PostFixedUpdate()
        { }

        protected virtual Vector3 GetStartingPosition()
        {
            return localTransform.position;
        }

        protected virtual Quaternion GetStartingRotation()
        {
            return localTransform.rotation;
        }

        protected abstract Vector3 GetNextPosition();
        protected abstract Quaternion GetNextRotation();

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_FixedPositionKey, fixedPosition);
            writer.WriteValue(k_FixedRotationKey, fixedRotation);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            localTransform = transform;
            attachedRigidbody = GetComponent<Rigidbody>();

            Vector3 position;
            if (reader.TryReadValue(k_FixedPositionKey, out position, Vector3.zero))
            {
                fixedPosition = position;
                previousPosition = position;
                attachedRigidbody.position = position;
            }

            Quaternion rotation;
            if (reader.TryReadValue(k_FixedRotationKey, out rotation, Quaternion.identity))
            {
                fixedRotation = rotation;
                previousRotation = rotation;
                attachedRigidbody.rotation = fixedRotation;
            }
        }
    }
}