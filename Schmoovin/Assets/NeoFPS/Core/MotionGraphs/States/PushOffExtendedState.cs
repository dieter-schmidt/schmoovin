#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Instant/Push Off Extended", "Push Off Extended")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-pushoffstate.html")]
    public class PushOffExtendedState : MotionGraphState
    {
        [SerializeField, Tooltip("The vector parameter containing the wall normal. This will be read AND written to each frame")]
        private VectorParameter m_WallNormal = null;
        [SerializeField, Tooltip("The world direction to push in (you can fill this parameter using enhanced cast conditions)")]
        private VectorParameter m_PushDirection = null;
        [SerializeField, Tooltip("An additional upward rotation applied to the push direction. Resulting direction won't rotate past up/down.")]
        private FloatDataReference m_PushUpAngle = new FloatDataReference(30f);
        [SerializeField, Tooltip("The speed to along the rotated push direction.")]
        private FloatDataReference m_PushSpeed = new FloatDataReference(5f);
        [SerializeField, Tooltip("Should the resulting velocity be additive to the original character velocity or ignore it.")]
        private bool m_Additive = true;
        //DS
        [SerializeField, Tooltip("Should the resulting vertical velocity be additive to the original character velocity or ignore it.")]
        private bool m_AdditiveVertical = true;
        [SerializeField, Tooltip("Should the resulting horizontal velocity be additive to the original character velocity or ignore it.")]
        private bool m_AdditiveHorizontal = true;
        [SerializeField, Tooltip("Should the resulting vertical velocity be clamped to the default push speed, regardless of character velocity.")]
        private bool m_ClampVerticalPush = true;
        //DS

        private Vector3 m_OutVelocity = Vector3.zero;
        private bool m_Completed = false;


        public override bool completed
        {
            get { return m_Completed; }
        }

        public override Vector3 moveVector
        {
            get { return m_OutVelocity * Time.deltaTime; }
        }

        public override bool applyGravity
        {
            get { return false; }
        }

        public override bool applyGroundingForce
        {
            get { return false; }
        }

        public override bool ignorePlatformMove
        {
            get { return false; }
        }

        public override void OnValidate()
        {
            base.OnValidate();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            m_Completed = false;
        }

        public override void OnExit()
        {
            base.OnExit();
            m_Completed = false;
            m_OutVelocity = Vector3.zero;
        }

        public override void Update()
        {
            base.Update();

            if (!m_Completed)
            {
                //DS
                // Perform cast to check for contact
                //RaycastHit hit;
                //bool didHit = controller.characterController.RayCast(0.25f, -m_WallNormal.value, Space.World, out hit, PhysicsFilter.Masks.CharacterBlockers, QueryTriggerInteraction.Ignore);
                //if (!didHit)
                //    m_Completed = true;
                //DS

                    if (m_PushDirection != null)
                {
                    Vector3 dir = m_PushDirection.value.normalized;
                    
                    // Rotate the push direction upwards
                    if (Mathf.Abs(m_PushUpAngle.value) > 0.1f)
                        dir = Vector3.RotateTowards(dir, characterController.up, m_PushUpAngle.value * Mathf.Deg2Rad, 0f);

                    // Apply the push velocity
                    if (m_Additive)
                    {
                        //DS
                        //if (m_AdditiveHorizontal)
                        //{
                        //    //m_OutVelocity.x = characterController.velocity.x + (m_PushSpeed.value * Mathf.Cos(m_PushUpAngle.value * Mathf.Deg2Rad));
                        //    //m_OutVelocity.z = characterController.velocity.z + (m_PushSpeed.value * Mathf.Cos(m_PushUpAngle.value * Mathf.Deg2Rad));
                        //    m_OutVelocity = characterController.velocity + (dir * m_PushSpeed.value);
                        //}
                        //else
                        //{
                        //    //m_OutVelocity.x = characterController.velocity.x;
                        //    //m_OutVelocity.z = characterController.velocity.z;
                        //    m_OutVelocity = (dir * m_PushSpeed.value);
                        //}
                        //if (m_AdditiveVertical)
                        //{
                        //    if (m_ClampVerticalPush)
                        //    {
                        //        //m_OutVelocity.y = Mathf.Max(characterController.velocity.y + (m_PushSpeed.value * Mathf.Sin(m_PushUpAngle.value * Mathf.Deg2Rad)),
                        //        //                    m_PushSpeed.value * Mathf.Sin(m_PushUpAngle.value * Mathf.Deg2Rad));

                        //    }
                        //    else
                        //    {
                        //        m_OutVelocity.y = characterController.velocity.y + (m_PushSpeed.value * Mathf.Sin(m_PushUpAngle.value * Mathf.Deg2Rad));
                        //    }

                        //}
                        //else
                        //{
                        //    m_OutVelocity.y = characterController.velocity.y;
                        //}
                        //DS
                        m_OutVelocity = characterController.velocity + (dir * m_PushSpeed.value);
                        //DS
                        m_OutVelocity.y = Mathf.Max(characterController.velocity.y + (m_PushSpeed.value * Mathf.Sin(m_PushUpAngle.value * Mathf.Deg2Rad)),
                                        m_PushSpeed.value * Mathf.Sin(m_PushUpAngle.value * Mathf.Deg2Rad));
                        //DS
                    }

                    else
                        m_OutVelocity = (dir * m_PushSpeed.value);
                }
                else
                    m_OutVelocity = characterController.velocity;

                m_Completed = true;

                Debug.Log("PUSH OFF: "+m_OutVelocity);
                Debug.Log("VEL: " + characterController.velocity);
                Debug.Log(characterController.GetComponent<MotionController>().motionGraph.GetVectorProperty(Animator.StringToHash("wallNormal")).value);
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            //m_WallNormal = map.Swap(m_WallNormal);
            m_PushDirection = map.Swap(m_PushDirection);
            m_PushUpAngle.CheckReference(map);
            m_PushSpeed.CheckReference(map);
            base.CheckReferences(map);
        }
    }
}