using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.CharacterMotion.States;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Misc/GrappleSwingState", "GrappleSwingState")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-grappleswingstate.html")]
    public class GrappleSwingState : MotionGraphState
    {
        [SerializeField, Tooltip("The point in space that the grapple is tethered to.")]
        private VectorParameter m_GrapplePoint = null;

        [SerializeField, Tooltip("When entering the state, a target distance will be calculated based on multiplying the current distance to the grapple point by this multiplier.")]
        private float m_TargetDistanceMultiplier = 0.75f;

        [SerializeField, Tooltip("Below this distance from the grapple point, the character will actually be pushed away.")]
        private float m_MinDistance = 2f;

        [SerializeField, Tooltip("The acceleration towards the grapple point per meter distance above the target distance.")]
        private float m_AccelerationPerMeter = 15f;

        [SerializeField, Tooltip("The maximum acceleration towards the grapple point.")]
        private float m_MaxAccel = 40f;

        private Vector3 m_OutVelocity = Vector3.zero;
        private float m_TargetLength = 0f;

        public override bool completed
        {
            get { return m_GrapplePoint == null || m_GrapplePoint.value == Vector3.zero; } 
        }

        public override Vector3 moveVector
        {
            get { return m_OutVelocity * Time.deltaTime; }
        }

        public override bool applyGravity
        {
            get { return true; }
        }

        public override bool applyGroundingForce
        {
            get { return false; }
        }

        public override void OnValidate()
        {
            base.OnValidate();
        }

        public override bool CheckCanEnter()
        {
            return m_GrapplePoint != null && m_GrapplePoint.value != Vector3.zero;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            m_TargetLength = Vector3.Magnitude(m_GrapplePoint.value - GetCenterPoint()) * m_TargetDistanceMultiplier;
            if (m_TargetLength < m_MinDistance)
                m_TargetLength = m_MinDistance;
        }

        public override void OnExit()
        {
            base.OnExit();
            m_OutVelocity = Vector3.zero;
        }

        Vector3 GetCenterPoint()
        {
            return controller.localTransform.position + controller.localTransform.up * characterController.height;
        }

        public override void Update()
        {
            base.Update();

            m_OutVelocity = characterController.velocity;


            if (m_GrapplePoint.value != Vector3.zero)
            {
                var deltaPos = m_GrapplePoint.value - GetCenterPoint();
                float distance = Vector3.Magnitude(deltaPos);
                if (distance > m_TargetLength)
                {
                    deltaPos.Normalize();
                    float pull = m_AccelerationPerMeter * (distance - m_TargetLength);
                    if (pull > m_MaxAccel)
                        pull = m_MaxAccel;
                    pull *= Time.deltaTime;

                    if (Vector3.Dot(m_OutVelocity, deltaPos) < 0f)
                    {
                        Vector3 target = Vector3.ProjectOnPlane(m_OutVelocity, deltaPos);
                        m_OutVelocity = Vector3.Lerp(m_OutVelocity, target, 0.25f);
                    }

                    m_OutVelocity += deltaPos * pull;
                }
                else
                {
                    if (distance < m_MinDistance)
                    {
                        deltaPos.Normalize();
                        float push = m_AccelerationPerMeter * (m_MinDistance - distance);
                        if (push > m_MaxAccel)
                            push = m_MaxAccel;
                        push *= -Time.deltaTime;

                        if (Vector3.Dot(m_OutVelocity, deltaPos) > 0f)
                        {
                            Vector3 target = Vector3.ProjectOnPlane(m_OutVelocity, deltaPos);
                            m_OutVelocity = Vector3.Lerp(m_OutVelocity, target, 0.25f);
                        }

                        m_OutVelocity += deltaPos * push;
                    }
                    else
                    {
                        if (Vector3.Dot(m_OutVelocity, deltaPos) < 0f)
                        {
                            Vector3 target = Vector3.ProjectOnPlane(m_OutVelocity, deltaPos);
                            m_OutVelocity = Vector3.Lerp(m_OutVelocity, target, 0.25f);
                        }
                    }
                }
            }
        }

        public override void ChangeFrameOfReference(Vector3 deltaPos, Quaternion deltaRot)
        {
            base.ChangeFrameOfReference(deltaPos, deltaRot);
            m_GrapplePoint.value = Vector3.zero;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_GrapplePoint = map.Swap(m_GrapplePoint);
            base.CheckReferences(map);
        }

        private static readonly NeoSerializationKey k_TargetLengthKey = new NeoSerializationKey("targetLength");

        /*
        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            // Optional - write any data saved by the NeoSave system here
            writer.WriteValue(k_OutVelocityKey, m_OutVelocity);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            // Optional - read any data saved by the NeoSave system here
            reader.TryReadValue(k_OutVelocityKey, out m_OutVelocity, m_OutVelocity);
        }
        */
    }
}